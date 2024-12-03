using FFLSharp.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace FFLSharp.Veldrid
{
    /// <summary>
    /// Vertex uniforms used for 3D shaders.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexUniforms
    {
        public Matrix4x4 ModelView;
        public Matrix4x4 Projection;
    }

    /// <summary>
    /// Fragment uniforms used for the 2D and 3D shader.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct FragmentUniforms
    {
        public int ModulateMode;
        // Padding for alignment.
        private readonly int _padding1;
        private readonly int _padding2;
        private readonly int _padding3;
        // Directly casted from FFLColor.
        public RgbaFloat ColorR;
        public RgbaFloat ColorG;
        public RgbaFloat ColorB;
    }


    // Vertex Layouts
    // Note that semantics are all TextureCoordinate because of Veldrid.SPIRV.
    public static class VertexLayouts
    {
        /// <summary>
        /// Maps each attribute buffer type to its expected stride value.
        /// NOTE that this... may or may not change if FFL ever supports half precision floats.
        /// </summary>
        public static readonly uint[] AttributeToStrideMap = new uint[(int)FFLAttributeBufferType.FFL_ATTRIBUTE_BUFFER_TYPE_MAX]
        {
            // FFL_ATTRIBUTE_BUFFER_TYPE_POSITION
            FormatSizeHelpers.GetSizeInBytes(VertexElementFormat.Float4),      // 16
            // FFL_ATTRIBUTE_BUFFER_TYPE_TEXCOORD
            FormatSizeHelpers.GetSizeInBytes(VertexElementFormat.Float2),      // 8
            // FFL_ATTRIBUTE_BUFFER_TYPE_NORMAL
            FormatSizeHelpers.GetSizeInBytes(VertexElementFormat.SByte4_Norm), // 4 / GL_INT_2_10_10_10_REV
            // FFL_ATTRIBUTE_BUFFER_TYPE_TANGENT
            FormatSizeHelpers.GetSizeInBytes(VertexElementFormat.SByte4_Norm), // 4
            // FFL_ATTRIBUTE_BUFFER_TYPE_COLOR
            FormatSizeHelpers.GetSizeInBytes(VertexElementFormat.Byte4_Norm),  // 4
        };

        /*
        // Only position and texCoord, for 2D planes
        public static readonly VertexLayoutDescription[] VertexLayoutsPosTexOnly = new[] { new VertexLayoutDescription(
            // FFL_ATTRIBUTE_BUFFER_TYPE_POSITION
            new VertexElementDescription("a_position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
            // FFL_ATTRIBUTE_BUFFER_TYPE_TEXCOORD
            new VertexElementDescription("a_texCoord", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2)) };
        */

        // ^^ Same as above but in separate buffers.
        public static readonly VertexLayoutDescription[] VertexLayoutsPosTexSeparate = new[] {
            // FFL_ATTRIBUTE_BUFFER_TYPE_POSITION
            new VertexLayoutDescription(
                stride: AttributeToStrideMap[(int)FFLAttributeBufferType.FFL_ATTRIBUTE_BUFFER_TYPE_POSITION],
                new VertexElementDescription("a_position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4)),
            // FFL_ATTRIBUTE_BUFFER_TYPE_TEXCOORD
            new VertexLayoutDescription(
                stride: AttributeToStrideMap[(int)FFLAttributeBufferType.FFL_ATTRIBUTE_BUFFER_TYPE_TEXCOORD],
                new VertexElementDescription("a_texCoord", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2)),
        };

        /*
        // For default 3D shapes.
        public static readonly VertexLayoutDescription[] VertexLayoutsShapeDefault = new[] { new VertexLayoutDescription(
            // FFL_ATTRIBUTE_BUFFER_TYPE_POSITION stride = 16
            new VertexElementDescription("a_position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
            // FFL_ATTRIBUTE_BUFFER_TYPE_TEXCOORD stride = 8
            new VertexElementDescription("a_texCoord", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            // FFL_ATTRIBUTE_BUFFER_TYPE_NORMAL stride = 4 / GL_INT_2_10_10_10_REV
            new VertexElementDescription("a_normal", VertexElementSemantic.TextureCoordinate, VertexElementFormat.SByte4_Norm)
        ) };
        */

        // ^^ Same as above but in separate buffers.
        public static readonly VertexLayoutDescription[] VertexLayoutsShapeDefaultSeparate = new[] {
            // FFL_ATTRIBUTE_BUFFER_TYPE_POSITION
            new VertexLayoutDescription(
                stride: AttributeToStrideMap[(int)FFLAttributeBufferType.FFL_ATTRIBUTE_BUFFER_TYPE_POSITION],
                new VertexElementDescription("a_position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4)),
            // FFL_ATTRIBUTE_BUFFER_TYPE_TEXCOORD
            new VertexLayoutDescription(
                stride: AttributeToStrideMap[(int)FFLAttributeBufferType.FFL_ATTRIBUTE_BUFFER_TYPE_TEXCOORD],
                new VertexElementDescription("a_texCoord", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2)),
            // FFL_ATTRIBUTE_BUFFER_TYPE_NORMAL
            new VertexLayoutDescription(
                stride: AttributeToStrideMap[(int)FFLAttributeBufferType.FFL_ATTRIBUTE_BUFFER_TYPE_NORMAL],
                new VertexElementDescription("a_normal", VertexElementSemantic.TextureCoordinate, VertexElementFormat.SByte4_Norm)),
        };

        /*
        // For hair, with tangent and color.
        public static readonly VertexLayoutDescription[] VertexLayoutsShapeHair = new[] { new VertexLayoutDescription(
            // FFL_ATTRIBUTE_BUFFER_TYPE_POSITION stride = 16
            new VertexElementDescription("a_position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
            // FFL_ATTRIBUTE_BUFFER_TYPE_TEXCOORD stride = 8
            new VertexElementDescription("a_texCoord", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            // FFL_ATTRIBUTE_BUFFER_TYPE_NORMAL stride = 4 / GL_INT_2_10_10_10_REV
            new VertexElementDescription("a_normal", VertexElementSemantic.TextureCoordinate, VertexElementFormat.SByte4_Norm),
            // FFL_ATTRIBUTE_BUFFER_TYPE_TANGENT stride = 4
            new VertexElementDescription("a_tangent", VertexElementSemantic.TextureCoordinate, VertexElementFormat.SByte4_Norm),
            // FFL_ATTRIBUTE_BUFFER_TYPE_COLOR stride = 4
            new VertexElementDescription("a_color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Byte4_Norm)
        ) };
        */

        // ^^ Same as above but in separate buffers.
        public static readonly VertexLayoutDescription[] VertexLayoutsShapeHairSeparate = new[] {
            // FFL_ATTRIBUTE_BUFFER_TYPE_POSITION
            new VertexLayoutDescription(
                stride: AttributeToStrideMap[(int)FFLAttributeBufferType.FFL_ATTRIBUTE_BUFFER_TYPE_POSITION],
                new VertexElementDescription("a_position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4)),
            // FFL_ATTRIBUTE_BUFFER_TYPE_TEXCOORD
            new VertexLayoutDescription(
                stride: AttributeToStrideMap[(int)FFLAttributeBufferType.FFL_ATTRIBUTE_BUFFER_TYPE_TEXCOORD],
                new VertexElementDescription("a_texCoord", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2)),
            // FFL_ATTRIBUTE_BUFFER_TYPE_NORMAL
            new VertexLayoutDescription(
                stride: AttributeToStrideMap[(int)FFLAttributeBufferType.FFL_ATTRIBUTE_BUFFER_TYPE_NORMAL],
                new VertexElementDescription("a_normal", VertexElementSemantic.TextureCoordinate, VertexElementFormat.SByte4_Norm)),
            // FFL_ATTRIBUTE_BUFFER_TYPE_TANGENT
            new VertexLayoutDescription(
                stride: AttributeToStrideMap[(int)FFLAttributeBufferType.FFL_ATTRIBUTE_BUFFER_TYPE_TANGENT],
                new VertexElementDescription("a_tangent", VertexElementSemantic.TextureCoordinate, VertexElementFormat.SByte4_Norm)),
            // FFL_ATTRIBUTE_BUFFER_TYPE_COLOR
            new VertexLayoutDescription(
                stride: AttributeToStrideMap[(int)FFLAttributeBufferType.FFL_ATTRIBUTE_BUFFER_TYPE_COLOR],
                new VertexElementDescription("a_color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Byte4_Norm)),
        };
    }
}
