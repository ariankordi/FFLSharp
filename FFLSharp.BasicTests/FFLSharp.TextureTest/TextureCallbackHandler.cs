using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using Veldrid;
using FFLSharp.Interop;
using System.Runtime.CompilerServices;

namespace FFLSharp.TextureTest
{
    public unsafe class TextureCallbackHandler : IDisposable
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly ResourceFactory _factory;

        // Mapping from FFLTexture pointer to Veldrid Texture
        // ShaderCallbackHandler reads from this, so it is public.
        public ConcurrentDictionary<UIntPtr, Texture> TextureMap = new ConcurrentDictionary<UIntPtr, Texture>();

        // Counter for generating unique texture handles
        private uint _nextTextureHandle = 1;

        // Lock object for thread-safe handle generation
        private readonly object _handleLock = new object();

        public TextureCallbackHandler(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _factory = _graphicsDevice.ResourceFactory;
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
                texture?.Dispose();
                return true;
            }
            return false; // could not be found
        }
        private void CreateTexture(FFLTextureInfo* pTextureInfo, void** ppTexture) // NOTE: pTexture is void**
        {
            //UIntPtr* textureHandle = (UIntPtr*)ppTexture; // Dereference to actual texture handle
            // Log creation request
            Console.WriteLine($"CreateTexture called with width={pTextureInfo->width}, height={pTextureInfo->height}, format={pTextureInfo->format}");

            // Determine Veldrid PixelFormat based on FFL format
            PixelFormat pixelFormat = ConvertFFLFormatToVeldrid(pTextureInfo->format);

            // Create Veldrid Texture

            TextureDescription textureDesc = TextureDescription.Texture2D(
                (uint)pTextureInfo->width,
                (uint)pTextureInfo->height,
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
                    (uint)pTextureInfo->width,
                    (uint)pTextureInfo->height,
                    depth: 1,
                    mipLevel: 0,
                    arrayLayer: 0);
            }

            // Add the texture to TextureMap and assign it a handle
            UIntPtr textureHandle = AddTextureToMap(texture);

            *ppTexture = (void*)textureHandle;

            Console.WriteLine($"CreateTexture: Assigned texture handle={textureHandle}");

            // Optionally, create a texture view if needed
            // var textureView = _factory.CreateTextureView(texture);
        }

        private void DeleteTexture(void** ppTexture)
        {
            UIntPtr* textureHandle = (UIntPtr*)ppTexture; // Dereference to actual texture handle
            Console.WriteLine($"DeleteTexture called for texture handle={*textureHandle}");

            if (DisposeTextureHandle(*textureHandle))
            {
                Console.WriteLine($"DeleteTexture: Texture handle {*textureHandle} deleted successfully.");
            }
            else
            {
                Console.WriteLine($"DeleteTexture: Texture handle {*textureHandle} not found.");
            }

            // Reset the pointer to the texture handle to zero
            *textureHandle = UIntPtr.Zero;
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
            var handler = (TextureCallbackHandler)GCHandle.FromIntPtr((IntPtr)pObj).Target;
            handler.CreateTexture(pTextureInfo, (void**)pTexture);
        }

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
        public static unsafe void DeleteTextureCallback(void* pObj, void* pTexture)
        {
            var handler = (TextureCallbackHandler)GCHandle.FromIntPtr((IntPtr)pObj).Target;
            handler.DeleteTexture((void**)pTexture);
        }

        public void Dispose()
        {
            foreach (var texture in TextureMap.Values)
            {
                texture.Dispose();
            }
            TextureMap.Clear();
        }
    }
}
