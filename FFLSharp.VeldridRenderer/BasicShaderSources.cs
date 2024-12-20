using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFLSharp.VeldridRenderer
{
    /// <summary>
    /// Contains sources for a basic shader pulled in from CharModelResource.
    /// </summary>
    public static class BasicShaderSources
    {

        // Defining shader code into strings for now.
        // Vertex shader corresponding to PipelineDefault3DShape
        public const string VertexShaderDefault3DCode = @"#version 310 es
            precision highp float;

            layout(set = 0, binding = 0) uniform VertexUniforms
            {
                mat4 u_mv;
                mat4 u_proj;
            } vertexUniforms;
            // Binding 0 = VertexUniforms / ResourceKind.UniformBuffer

            layout(location = 0) in vec4 a_position;  // FFL_ATTRIBUTE_BUFFER_TYPE_POSITION
            layout(location = 1) in vec2 a_texCoord;  // FFL_ATTRIBUTE_BUFFER_TYPE_TEXCOORD

            layout(location = 2) in vec3 a_normal;    // FFL_ATTRIBUTE_BUFFER_TYPE_NORMAL
//          layout(location = 3) in vec3 a_tangent;   // FFL_ATTRIBUTE_BUFFER_TYPE_TANGENT
//          layout(location = 4) in vec4 a_color;     // FFL_ATTRIBUTE_BUFFER_TYPE_COLOR


            layout(location = 0) out vec2 v_texCoord;
            layout(location = 1) out vec4 v_position;

            layout(location = 2) out vec3 v_normal;
            layout(location = 3) out vec3 v_tangent;
            layout(location = 4) out vec4 v_color;


            void main()
            {
                vec4 position = vec4(a_position.xyz, 1.0);
                vec4 transformed = vertexUniforms.u_mv * position;
                gl_Position = vertexUniforms.u_proj * transformed;

                v_position = transformed;
                v_normal = a_normal;
                //v_tangent = v_tangent;
                v_texCoord = a_texCoord;
                //v_color = a_color;

                //v_normal = vec3(0.0, 0.0, 0.0);
                v_tangent = vec3(0.0, 0.0, 0.0);
                v_color = vec4(0.0, 0.0, 0.0, 0.0);
            }
            ";

        // This shader has no uniforms, lighting attributes
        // and is meant for the 2D planes.
        public const string VertexShader2DPlaneCode = @"#version 310 es
            precision highp float;

            layout(set = 0, binding = 0) uniform VertexUniforms
            {
                mat4 u_mv;
                mat4 u_proj;
            } vertexUniforms;
            // Binding 0 = VertexUniforms / ResourceKind.UniformBuffer

            layout(location = 0) in vec4 a_position;  // FFL_ATTRIBUTE_BUFFER_TYPE_POSITION
            layout(location = 1) in vec2 a_texCoord;  // FFL_ATTRIBUTE_BUFFER_TYPE_TEXCOORD

            layout(location = 0) out vec2 v_texCoord;
            layout(location = 1) out vec4 v_position;

            layout(location = 2) out vec3 v_normal;
            layout(location = 3) out vec3 v_tangent;
            layout(location = 4) out vec4 v_color;

            void main()
            {
                gl_Position = vec4(a_position.xyz, 1.0);
                v_texCoord = a_texCoord;

                v_position = vertexUniforms.u_proj * a_position;
                v_normal = vec3(0.0, 0.0, 0.0);
                v_tangent = vec3(0.0, 0.0, 0.0);
                v_color = vec4(0.0, 0.0, 0.0, 0.0);
            }
            ";

        // This fragment shader should be being used for everything.
        public const string FragmentShaderCode = @"#version 310 es
            precision mediump float;

            layout(set = 0, binding = 1) uniform FragmentUniforms
            {
                int u_mode;
                vec4 u_const1;
                vec4 u_const2;
                vec4 u_const3;
            } fragmentUniforms;
            // Binding 1 = FragmentUniforms / ResourceKind.UniformBuffer

            // TODO: May not be one-to-one compatible with other environments (need if VELDRID preprocessor?)
            // See: https://github.com/SupinePandora43/UltralightNet/blob/95a060fc226024a81cd9ba058d691628e1055489/gpu/shaders/shader_fill.frag#L188

            layout(set = 0, binding = 2) uniform mediump texture2D Texture;
            // Binding 2 = ResourceKind.TextureReadOnly
            layout(set = 0, binding = 3) uniform mediump sampler Sampler;
            // Binding 3 = ResourceKind.Sampler

            layout(location = 0) in vec2 v_texCoord;
            layout(location = 1) in vec4 v_position;

            layout(location = 2) in vec3 v_normal;
            layout(location = 3) in vec3 v_tangent;
            layout(location = 4) in vec4 v_color;

            layout(location = 0) out vec4 FragColor;

            void main()
            {
            //FragColor = vec4(v_texCoord.xy, 0.0, 1.0); return;
                vec4 texColor = texture(sampler2D(Texture, Sampler), v_texCoord); // Sample from the texture

                if (fragmentUniforms.u_mode == 0)
                    FragColor = fragmentUniforms.u_const1;
                else if (fragmentUniforms.u_mode == 1)
                    FragColor = texColor;
                else if (fragmentUniforms.u_mode == 2)
                    FragColor = vec4(
                        fragmentUniforms.u_const1.rgb * texColor.r +
                        fragmentUniforms.u_const2.rgb * texColor.g +
                        fragmentUniforms.u_const3.rgb * texColor.b,
                        texColor.a
                    );
                else if (fragmentUniforms.u_mode == 3)
                    FragColor = vec4(
                        fragmentUniforms.u_const1.rgb * texColor.r,
                        texColor.r
                    );
                else if (fragmentUniforms.u_mode == 4)
                    FragColor = vec4(
                        fragmentUniforms.u_const1.rgb * texColor.g,
                        texColor.r
                    );
                else if (fragmentUniforms.u_mode == 5)
                    FragColor = vec4(
                        fragmentUniforms.u_const1.rgb * texColor.r,
                        1.0
                    );

                // avoids little outline around mask elements
                if (FragColor.a == 0.0f)
                    discard;
            }
            ";
    }
}
