using FFLSharp.Interop;
using Veldrid;

namespace FFLSharp.VeldridRenderer
{
    /// <summary>
    /// Defines vertex layouts for FFL shapes.
    /// Note that semantics are all TextureCoordinate because of Veldrid.SPIRV.
    /// </summary>
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

        // Only position and texCoord, for 2D planes.
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


        // For default 3D shapes.
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

        // For hair, with tangent and color.
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

        /*
        // Only position and texCoord, for 2D planes.
        public static readonly VertexLayoutDescription[] VertexLayoutsPosTexOnly = new[] { new VertexLayoutDescription(
            // FFL_ATTRIBUTE_BUFFER_TYPE_POSITION
            new VertexElementDescription("a_position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4),
            // FFL_ATTRIBUTE_BUFFER_TYPE_TEXCOORD
            new VertexElementDescription("a_texCoord", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2)) };
        */
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
    }
}
