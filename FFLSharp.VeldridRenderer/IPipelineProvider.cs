using FFLSharp.Interop;
using Veldrid;

namespace FFLSharp.VeldridRenderer
{
    /// <summary>
    /// Provider for pipelines and setting uniforms.
    /// </summary>
    public interface IPipelineProvider
    {
        /// <summary>
        /// Blank texture bound to render passes without textures.
        /// </summary>
        Texture DefaultTexture { get; }
        /// <summary>
        /// Pipeline for faceline texture 2D plane drawing.
        /// </summary>
        Pipeline PipelineFaceline2DPlane { get; }
        /// <summary>
        /// Pipeline for mask texture 2D plane drawing.
        /// </summary>
        Pipeline PipelineMask2DPlane { get; }
        /// <summary>
        /// Pipeline for opaque shapes in DrawOpa.
        /// </summary>
        Pipeline PipelineShapeOpa { get; }
        /// <summary>
        /// Pipeline for transparent shapes in DrawXlu stage.
        /// </summary>
        Pipeline PipelineShapeXlu { get; }
        /// <summary>
        /// Pipeline specifically for XluGlass shape.
        /// </summary>
        Pipeline PipelineShapeXluGlass { get; }

        /// <summary>
        /// Resource layout for uniforms, textures, and samplers.
        /// </summary>
        ResourceLayout ResourceLayout { get; }

        /// <summary>
        /// Pixel format for swapchain, set only after pipelines are made.
        /// </summary>
        PixelFormat? SwapchainTexFormat { get; }

        /// <summary>
        /// Sampler bound to all shapes except OpaFaceline and XluGlass.
        /// </summary>
        Sampler Sampler { get; }
        /// <summary>
        /// Sampler bound to OpaFaceline and XluGlass. Must mirror UVs.
        /// </summary>
        Sampler SamplerMirror { get; }


        DeviceBuffer CreateVertexUniformBuffer();

        DeviceBuffer CreateFragmentUniformBuffer();

        /// <summary>
        /// Set fragment uniforms based on modulate parameters.
        /// </summary>
        /// <param name="modulateParam">FFLModulateParam containing mode/type, const colors, etc.</param>
        void UpdateFragmentUniformBuffer(DeviceBuffer uniformBuffer, FFLModulateParam modulateParam);

        void Dispose();
    }
}