using FFLSharp.Interop;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FFLSharp.VeldridRenderer
{
    public static class VertexInterleaver
    {
        /// <summary>
        /// Extracts and interleaves FFLAttributeBufferParam into a byte[] buffer.
        /// </summary>
        /// <param name="attributeBufferParam">FFLDrawParam.attributeBufferParam</param>
        /// <param name="for2DPlanes">If this is true, the normal attribute is excluded - just position and texCoord.</param>
        /// <returns>Byte array of interleaved attributes.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static unsafe byte[] CopyInterleaveAttrBufToBytes(FFLAttributeBufferParam attributeBufferParam,
            bool for2DPlanes = false) // TODO: potentially just pass vertex layout itself into this?
        {
            // Get attribute buffers
            var attrBuffers = attributeBufferParam.attributeBuffers;

            // Determine vertex count from position buffer (mandatory)
            if (attrBuffers[(int)FFLAttributeBufferType.FFL_ATTRIBUTE_BUFFER_TYPE_POSITION].ptr == null)
            {
                throw new InvalidOperationException("Position attribute is missing, cannot generate vertex data.");
            }

            uint vertexCount = attrBuffers[(int)FFLAttributeBufferType.FFL_ATTRIBUTE_BUFFER_TYPE_POSITION].size /
                               attrBuffers[(int)FFLAttributeBufferType.FFL_ATTRIBUTE_BUFFER_TYPE_POSITION].stride;

            // Calculate vertex size (stride for interleaved attributes)
            uint vertexStride = 0;
            // Position is mandatory
            vertexStride += VertexLayouts.AttributeToStrideMap[(int)FFLAttributeBufferType.FFL_ATTRIBUTE_BUFFER_TYPE_POSITION];
            // texCoord (optional)
            vertexStride += VertexLayouts.AttributeToStrideMap[(int)FFLAttributeBufferType.FFL_ATTRIBUTE_BUFFER_TYPE_TEXCOORD];
            if (!for2DPlanes)
                // Normal (GL_INT_2_10_10_10_REV)
                vertexStride += VertexLayouts.AttributeToStrideMap[(int)FFLAttributeBufferType.FFL_ATTRIBUTE_BUFFER_TYPE_NORMAL];

            // Optional: include commented code for tangent and color (future extension)
            // Tangent
            // vertexStride += VertexLayouts.AttributeToStrideMap[(int)FFLAttributeBufferType.FFL_ATTRIBUTE_BUFFER_TYPE_TANGENT];
            // Color
            // vertexStride += VertexLayouts.AttributeToStrideMap[(int)FFLAttributeBufferType.FFL_ATTRIBUTE_BUFFER_TYPE_COLOR];

            // Create interleaved vertex buffer
            byte[] vertexData = new byte[vertexCount * vertexStride];
            var builder = new VertexDataBuilder(vertexData, vertexStride);

            // Loop through vertices and copy interleaved data
            for (uint i = 0; i < vertexCount; i++)
            {
                uint offset = 0;

                // Position (mandatory)
                CopyAttribute(attrBuffers[(int)FFLAttributeBufferType.FFL_ATTRIBUTE_BUFFER_TYPE_POSITION], i, builder, offset);
                offset += VertexLayouts.AttributeToStrideMap[(int)FFLAttributeBufferType.FFL_ATTRIBUTE_BUFFER_TYPE_TEXCOORD];

                // texCoord (optional, default to zero if missing)
                if (attrBuffers[(int)FFLAttributeBufferType.FFL_ATTRIBUTE_BUFFER_TYPE_TEXCOORD].ptr != null)
                {
                    CopyAttribute(attrBuffers[(int)FFLAttributeBufferType.FFL_ATTRIBUTE_BUFFER_TYPE_TEXCOORD], i, builder, offset);
                }
                else
                {
                    builder.WriteVertexElement(i, offset, Vector2.Zero);
                }
                offset += VertexLayouts.AttributeToStrideMap[(int)FFLAttributeBufferType.FFL_ATTRIBUTE_BUFFER_TYPE_TEXCOORD];

                if (!for2DPlanes)
                {
                    // Normal (optional, default to zero if missing)
                    if (attrBuffers[(int)FFLAttributeBufferType.FFL_ATTRIBUTE_BUFFER_TYPE_NORMAL].ptr != null)
                    {
                        CopyAttribute(attrBuffers[(int)FFLAttributeBufferType.FFL_ATTRIBUTE_BUFFER_TYPE_NORMAL], i, builder, offset);
                    }
                    else
                    {
                        builder.WriteVertexElement(i, offset, 0);
                    }
                    // TODO: UNCOMMENT THE BELOW IF YOU ARE ADDING MORE ATTRIBUTES!!!!!!!!!!!!!!
                    //offset += VertexLayouts.AttributeToStrideMap[(int)FFLAttributeBufferType.FFL_ATTRIBUTE_BUFFER_TYPE_NORMAL];
                    // ^^ "Unnecessary assignment of a value to 'offset'"
                }

                // Optional future attributes:
                /*
                // Tangent (optional, default to zero if missing)
                if (attrBuffers[(int)FFLAttributeBufferType.FFL_ATTRIBUTE_BUFFER_TYPE_TANGENT].ptr != null)
                {
                    CopyAttribute(attrBuffers[(int)FFLAttributeBufferType.FFL_ATTRIBUTE_BUFFER_TYPE_TANGENT], i, builder, offset);
                }
                else
                {
                    builder.WriteVertexElement(i, offset, (byte)0);
                }
                offset += VertexLayouts.AttributeToStrideMap[(int)FFLAttributeBufferType.FFL_ATTRIBUTE_BUFFER_TYPE_TANGENT];

                // Color (optional, default to zero if missing)
                if (attrBuffers[(int)FFLAttributeBufferType.FFL_ATTRIBUTE_BUFFER_TYPE_COLOR].ptr != null)
                {
                    CopyAttribute(attrBuffers[(int)FFLAttributeBufferType.FFL_ATTRIBUTE_BUFFER_TYPE_COLOR], i, builder, offset);
                }
                else
                {
                    builder.WriteVertexElement(i, offset, (byte)0);
                }
                */
            }

            builder.FreeGCHandle();
            return vertexData;
        }

        // Helper for above method: Copy a single attribute to the interleaved buffer.
        private static unsafe void CopyAttribute(FFLAttributeBuffer attrBuffer, uint vertexIndex, VertexDataBuilder builder, uint offset)
        {
            if (attrBuffer.ptr == null) return;

            IntPtr attributePtr = (IntPtr)((byte*)attrBuffer.ptr + (vertexIndex * attrBuffer.stride));
            builder.WriteVertexElement(vertexIndex, offset, (byte*)attributePtr, attrBuffer.stride);
        }

        /// <summary>
        /// Extension method on FFLAttributeBuffer to determine whether or not that attribute is usable.
        /// </summary>
        /// <param name="buffer">Attribute buffer</param>
        /// <returns>Whether or not you can use this attribute.</returns>
        public static unsafe bool IsUsable(this FFLAttributeBuffer buffer)
        {
            return buffer.ptr != null && buffer.size > 0
                && buffer.stride > 0; // NOTE: Some buffers (color) have a stride of 0 but size of 4
                                      // which I think means its value is consant (uniform?)
        }

        /// <summary>
        /// Note: taken from https://github.com/mellinoe/veldrid-samples/blob/master/src/AssetProcessor/AssimpProcessor.cs
        /// </summary>
        private unsafe readonly struct VertexDataBuilder
        {
            private readonly GCHandle _gch;
            private readonly byte* _dataPtr;
            private readonly uint _vertexSize;

            public VertexDataBuilder(byte[] data, uint vertexSize)
            {
                _gch = GCHandle.Alloc(data, GCHandleType.Pinned);
                _dataPtr = (byte*)_gch.AddrOfPinnedObject();
                _vertexSize = vertexSize;
            }

            public void WriteVertexElement<T>(uint vertex, uint elementOffset, ref T data) where T : unmanaged
            {
                byte* dst = _dataPtr + (_vertexSize * vertex) + elementOffset;
                Unsafe.Copy(dst, ref data);
            }

            // Overload to copy raw bytes
            public unsafe void WriteVertexElement(uint vertex, uint elementOffset, byte* sourcePtr, uint size)
            {
                byte* dst = _dataPtr + (vertex * _vertexSize) + elementOffset;
                Buffer.MemoryCopy(sourcePtr, dst, size, size);
            }

            public void WriteVertexElement<T>(uint vertex, uint elementOffset, T data) where T : unmanaged
            {
                byte* dst = _dataPtr + (_vertexSize * vertex) + elementOffset;
                Unsafe.Copy(dst, ref data);
            }

            public void FreeGCHandle()
            {
                _gch.Free();
            }
        }

    }
}
