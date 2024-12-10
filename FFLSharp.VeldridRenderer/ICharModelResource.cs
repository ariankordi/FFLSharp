using Veldrid;

namespace FFLSharp.VeldridRenderer
{
    public interface ICharModelResource
    {
        Texture DefaultTexture { get; }
        Pipeline PipelineFaceline2DPlane { get; }
        Pipeline PipelineMask2DPlane { get; }
        Pipeline PipelineShapeOpa { get; }
        Pipeline PipelineShapeXlu { get; }
        Pipeline PipelineShapeXluGlass { get; }

        // Resource layout for uniforms, textures, and samplers.
        ResourceLayout ResourceLayout { get; }

        // Pixel format for swapchain, set only after pipelines are made.
        PixelFormat? SwapchainTexFormat { get; }

        Sampler Sampler { get; }
        Sampler SamplerMirror { get; }

        void Dispose();
    }
}