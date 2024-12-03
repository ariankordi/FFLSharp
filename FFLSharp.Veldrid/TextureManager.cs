using FFLSharp.Interop;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Veldrid;

namespace FFLSharp.Veldrid
{
    public unsafe class TextureManager : IDisposable, ITextureManager
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly ResourceFactory _factory;

        // Mapping from FFLTexture pointer to Veldrid Texture
        // ShaderCallbackHandler reads from this, so it is public.
        public ConcurrentDictionary<UIntPtr, Texture> TextureMap = new();

        // Counter for generating unique texture handles
        private ulong _nextTextureHandle = 1;
        private readonly object _handleLock = new(); // Lock for handle count.

        // Callback structure to register to FFL
        private FFLTextureCallback _textureCallback;
        // Handle to be pinned and used when setting the callback.
        private GCHandle? _gcHandle;

        public TextureManager(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _factory = _graphicsDevice.ResourceFactory;
        }

        /// <summary>
        /// Pins this structure with a GCHandle and registers it as a shader callback with FFL.
        /// Note: As of 2024-12-01 I still can't find a way to avoid this eventually getting
        /// garbage collected so, as a result,
        /// </summary>
        public void RegisterCallback()
        {
            // Ensure the FFLiManager is constructed before continuing.
            byte isAvailable = FFL.IsAvailable();
            if (isAvailable == 0) // false
            {
                throw new Exception($"FFLIsAvailable() returned {isAvailable}. Cannot set callback if FFLiManager is not constructed.");
            }

            _gcHandle?.Free(); // Free existing GCHandle.
            // Allocate a GCHandle for this instance to prevent GC from moving or collecting it.
            _gcHandle = GCHandle.Alloc(this);

            // Create an instance of the callback structure
            _textureCallback = new FFLTextureCallback
            {
                pObj = (void*)GCHandle.ToIntPtr(_gcHandle.Value),
                pCreateFunc = &CreateTextureCallback,
                pDeleteFunc = &DeleteTextureCallback // ^^ Static function pointers
            };
            // Get a pointer to the unmanaged structure
            fixed (FFLTextureCallback* p = &_textureCallback)
            {
                // Register the callback with FFL
                FFL.SetTextureCallback(p);
            }

        }

        /// <summary>
        /// Get FFLTextureCallback for use in functions like FFLInitCharModelCPUStepWithCallback.
        /// </summary>
        public FFLTextureCallback* GetTextureCallback()
        {
            _gcHandle?.Free(); // Free existing GCHandle.
            _gcHandle = GCHandle.Alloc(this, GCHandleType.Weak);
            // ^^ GCHandleType.Weak allows the GC to still collect this.

            // Create an instance of the callback structure
            _textureCallback = new FFLTextureCallback
            {
                pObj = (void*)GCHandle.ToIntPtr(_gcHandle.Value),
                pCreateFunc = &CreateTextureCallback,
                pDeleteFunc = &DeleteTextureCallback // ^^ Static function pointers
            };
            fixed (FFLTextureCallback* p = &_textureCallback)
            {
                return p;
            }
        }

        public bool GetTextureFromMap(UIntPtr key, out Texture? value)
        {
            return TextureMap.TryGetValue(key, out value);
        }

        public UIntPtr AddTextureToMap(Texture texture)
        {
            // Generate a unique texture handle
            UIntPtr textureHandle;
            lock (_handleLock)
            {
                textureHandle = (UIntPtr)_nextTextureHandle++; // ig this means 4 billion textures only
            }

            // Since Veldrid abstracts the actual GPU handle, we'll use
            // the Texture instance itself and store it in the map
            TextureMap[textureHandle] = texture;

            return textureHandle;
        }
        public bool DisposeTextureHandle(UIntPtr handle)
        {
            if (TextureMap.TryRemove(handle, out Texture? texture))
            {
                Console.WriteLine($"DeleteTexture: Deleting handle: {handle}");
                texture.Dispose();
                return true;
            }
            else if (handle == UIntPtr.Zero)
                Console.WriteLine($"DeleteTexture: Null texture handle passed in.");
            else
            {
                // deleting a texture that does not exist is a totally normal circumstance
                // because mask textures usually use the exact same handles/pointers
                Console.WriteLine($"DeleteTexture: Unknown handle:  {handle}");
            }
            return false; // could not be found
        }
        private void CreateTexture(FFLTextureInfo* pTextureInfo, void** ppTexture) // NOTE: pTexture is void**
        {
            //UIntPtr* textureHandle = (UIntPtr*)ppTexture; // Dereference to actual texture handle
            // Log creation request
            Console.WriteLine($"CreateTexture: width={pTextureInfo->width}, height={pTextureInfo->height}, format={pTextureInfo->format}");

            // Determine Veldrid PixelFormat based on FFL format
            PixelFormat pixelFormat = ConvertFFLFormatToVeldrid(pTextureInfo->format);

            // Create Veldrid Texture

            TextureDescription textureDesc = TextureDescription.Texture2D(
                pTextureInfo->width,
                pTextureInfo->height,
                1,//(uint)pTextureInfo->numMips, // TODO: no mipmaps
                1,
                pixelFormat,
                TextureUsage.Sampled | TextureUsage.Storage);

            Texture texture = _factory.CreateTexture(ref textureDesc);

            // Upload texture data
            if (pTextureInfo->imagePtr != null)
            {
                // Use the unmanaged pointer directly
                uint imageSize = pTextureInfo->size;
                IntPtr imagePtr = (IntPtr)pTextureInfo->imagePtr;

                // Update the texture using the pointer
                _graphicsDevice.UpdateTexture(
                    texture,
                    imagePtr,
                    imageSize,
                    0, 0, 0,
                    pTextureInfo->width,
                    pTextureInfo->height,
                    depth: 1,
                    mipLevel: 0,
                    arrayLayer: 0);
            }

            // Add the texture to TextureMap and assign it a handle
            UIntPtr textureHandle = AddTextureToMap(texture);

            *ppTexture = (void*)textureHandle;

            Console.WriteLine($"CreateTexture: Created handle:  {textureHandle}");
        }

        public void DeleteTexture(void** ppTexture)
        {
            UIntPtr textureHandle = *(UIntPtr*)ppTexture; // Dereference to actual texture handle
            //Console.WriteLine($"DeleteTexture called for texture handle={textureHandle}");
            Debug.Assert(textureHandle != UIntPtr.Zero);
            // ^^ FFL should not pass a null texture handle to this function...???

            // 10 million
            //Debug.Assert((ulong)textureHandle < 10000000, "Texture handle is huge. Is it corrupted?");

            DisposeTextureHandle(textureHandle); // Will log if handle was not found, etc.

            // Reset the pointer to the texture handle to zero.
            *ppTexture = (void*)UIntPtr.Zero;
            // NOTE: Although the original intent was to set the original DrawParam
            // texture2D properties to zero to avoid FFL from calling the pTextureDelete
            // function, well, turns out there are some properties that keep texture handles
            // such as FFLiFacelineTextureTempObject.pTextureFaceMake, not very accessible.
            // If the CreateTexture routine stored the original pointer and set that one as
            // well as the one in the DrawParam it maaay?? work but that's not being done RN
        }

        private static PixelFormat ConvertFFLFormatToVeldrid(FFLiTextureFormat fflFormat)
        {
            // Map FFL texture formats to Veldrid PixelFormats
            return fflFormat switch
            {
                FFLiTextureFormat.FFLI_TEXTURE_FORMAT_R8 => PixelFormat.R8_UNorm,
                FFLiTextureFormat.FFLI_TEXTURE_FORMAT_RG8 => PixelFormat.R8_G8_UNorm,
                FFLiTextureFormat.FFLI_TEXTURE_FORMAT_RGBA8 => PixelFormat.R8_G8_B8_A8_UNorm,
                _ => throw new NotSupportedException($"Unsupported FFL texture format: {fflFormat}"),
            };
        }

        // Delegate instances
        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static unsafe void CreateTextureCallback(void* pObj, FFLTextureInfo* pTextureInfo, void* pTexture)
        {
            GCHandle handle = GCHandle.FromIntPtr((IntPtr)pObj);
            Debug.Assert(handle.Target != null);
            var instance = (TextureManager)handle.Target;
            instance.CreateTexture(pTextureInfo, (void**)pTexture);
        }

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static unsafe void DeleteTextureCallback(void* pObj, void* pTexture)
        {
            GCHandle handle = GCHandle.FromIntPtr((IntPtr)pObj);
            Debug.Assert(handle.Target != null);
            var instance = (TextureManager)handle.Target;
            instance.DeleteTexture((void**)pTexture);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            // Free GCHandle if it was allocated.
            _gcHandle?.Free();
            foreach (var texture in TextureMap.Values)
            {
                Debug.Assert(false,
                    "All textures allocated should be deleted before TextureManager destruction happens.");
                texture.Dispose();
            }
            TextureMap.Clear();
        }
    }
}