using FFLSharp.Interop;
using Veldrid;

namespace FFLSharp.Veldrid
{
    public interface ITextureManager
    {
        UIntPtr AddTextureToMap(Texture texture);
        unsafe void DeleteTexture(void** ppTexture);
        void Dispose();
        bool DisposeTextureHandle(UIntPtr handle);
        unsafe FFLTextureCallback* GetTextureCallback();
        bool GetTextureFromMap(UIntPtr key, out Texture? value);
        void RegisterCallback();
    }
}