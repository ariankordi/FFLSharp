using FFLSharp.Interop;
using Veldrid;

namespace FFLSharp.VeldridRenderer
{
    public interface IUniformProvider
    {
        DeviceBuffer CreateVertexUniformBuffer();

        DeviceBuffer CreateFragmentUniformBuffer();

        /// <summary>
        /// Set fragment uniforms based on modulate parameters.
        /// </summary>
        /// <param name="modulateParam">FFLModulateParam containing mode/type, const colors, etc.</param>
        void UpdateFragmentUniformBuffer(DeviceBuffer uniformBuffer, FFLModulateParam modulateParam);
    }
}
