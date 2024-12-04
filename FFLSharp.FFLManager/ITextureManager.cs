using FFLSharp.Interop; // FFLTextureCallback

namespace FFLSharp
{
    /// <summary>
    /// Defines an interface for a callback to pass into FFL
    /// that creates, deletes and manages textures and handles.
    /// </summary>
    public interface ITextureManager
    {
        unsafe void DeleteTexture(void** ppTexture);
        void Dispose();
        bool DisposeTextureHandle(UIntPtr handle);
        unsafe FFLTextureCallback* GetTextureCallback();
        void RegisterCallback();
    }
}