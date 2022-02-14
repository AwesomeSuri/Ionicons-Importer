using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.U2D;

namespace Editor
{
    /// <summary>
    /// Copied from Unity.VectorGraphic package
    /// </summary>
    public static class VectorUtils
    {
        private static Material s_ExpandEdgesMat;

        /// <summary>Renders a vector sprite to Texture2D.</summary>
        /// <param name="sprite">The sprite to render</param>
        /// <param name="width">The desired width of the resulting texture</param>
        /// <param name="height">The desired height of the resulting texture</param>
        /// <param name="mat">The material used to render the sprite</param>
        /// <param name="antiAliasing">The number of samples per pixel for anti-aliasing</param>
        /// <param name="expandEdges">When true, expand the edges to avoid a dark banding effect caused by filtering. This is slower to render and uses more graphics memory.</param>
        /// <returns>A Texture2D object containing the rendered vector sprite</returns>
        public static Texture2D RenderSpriteToTexture2D(Sprite sprite, int width, int height, Material mat,
            int antiAliasing = 1, bool expandEdges = false)
        {
            if (width <= 0 || height <= 0)
                return null;

            RenderTexture tex = null;
            var oldActive = RenderTexture.active;

            var desc = new RenderTextureDescriptor(width, height, RenderTextureFormat.ARGB32, 0)
            {
                msaaSamples = 1,
                sRGB = QualitySettings.activeColorSpace == ColorSpace.Linear
            };

            if (expandEdges)
            {
                // Draw the sprite normally to be used as a background, no-antialiasing
                var normalTex = RenderTexture.GetTemporary(desc);
                RenderTexture.active = normalTex;
                RenderSprite(sprite, mat);

                // Expand the edges and make completely transparent
                if (s_ExpandEdgesMat == null)
                {
                    var shader = Shader.Find("Hidden/VectorExpandEdges");
                    if (shader == null)
                    {
#if UNITY_EDITOR
                        // Workaround for case 1167309.
                        // Shader.Find() seems to fail on the package shader when doing a fresh import with a clean Library folder,
                        // but AssetDatabase.LoadAssetAtPath() works fine though.
                        shader = UnityEditor.AssetDatabase.LoadAssetAtPath<Shader>(
                            "Packages/com.unity.vectorgraphics/Runtime/Shaders/VectorExpandEdges.shader");
#else
                        return null;
#endif
                    }

                    s_ExpandEdgesMat = new Material(shader);
                }

                var expandTex = RenderTexture.GetTemporary(desc);
                RenderTexture.active = expandTex;
                GL.Clear(false, true, Color.clear);
                Graphics.Blit(normalTex, expandTex, s_ExpandEdgesMat, 0);
                RenderTexture.ReleaseTemporary(normalTex);

                // Draw the sprite again, but clear with the texture rendered in the previous step,
                // this will make the bilinear filter to interpolate the colors with values different
                // than "transparent black", which causes black-ish outlines around the shape.
                desc.msaaSamples = antiAliasing;
                tex = RenderTexture.GetTemporary(desc);
                RenderTexture.active = tex;
                Graphics.Blit(expandTex, tex);
                RenderTexture.ReleaseTemporary(expandTex); // Use the expanded texture to clear the buffer

                RenderTexture.active = tex;
                RenderSprite(sprite, mat, false);
            }
            else
            {
                desc.msaaSamples = antiAliasing;
                tex = RenderTexture.GetTemporary(desc);
                RenderTexture.active = tex;
                RenderSprite(sprite, mat);
            }

            Texture2D copy = new Texture2D(width, height, TextureFormat.RGBA32, false);
            copy.hideFlags = HideFlags.HideAndDontSave;
            copy.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            copy.Apply();

            RenderTexture.active = oldActive;
            RenderTexture.ReleaseTemporary(tex);

            return copy;
        }

        /// <summary>Draws a vector sprite using the provided material.</summary>
        /// <param name="sprite">The sprite to render</param>
        /// <param name="mat">The material used for rendering</param>
        /// <param name="clear">If true, clear the render target before rendering</param>
        public static void RenderSprite(Sprite sprite, Material mat, bool clear = true)
        {
            float spriteWidth = sprite.rect.width;
            float spriteHeight = sprite.rect.height;
            float pixelsToUnits = sprite.rect.width / sprite.bounds.size.x;

            var uvs = sprite.uv;
            var triangles = sprite.triangles;
            var pivot = sprite.pivot;

            var vertices = sprite.vertices.Select(v =>
                new Vector2((v.x * pixelsToUnits + pivot.x) / spriteWidth,
                    (v.y * pixelsToUnits + pivot.y) / spriteHeight)
            ).ToArray();

            Color[] colors = null;
            if (sprite.HasVertexAttribute(VertexAttribute.Color))
                colors = sprite.GetVertexAttribute<Color32>(VertexAttribute.Color).Select(c => (Color)c).ToArray();

            Vector2[] settings = null;
            if (sprite.HasVertexAttribute(VertexAttribute.TexCoord2))
                settings = sprite.GetVertexAttribute<Vector2>(VertexAttribute.TexCoord2).ToArray();

            RenderFromArrays(vertices, sprite.triangles, sprite.uv, colors, settings, sprite.texture, mat, clear);
        }

        internal static void RenderFromArrays(Vector2[] vertices, UInt16[] indices, Vector2[] uvs, Color[] colors,
            Vector2[] settings, Texture2D texture, Material mat, bool clear = true)
        {
            mat.SetTexture("_MainTex", texture);
            mat.SetPass(0);

            if (clear)
                GL.Clear(true, true, Color.clear);

            GL.PushMatrix();
            GL.LoadOrtho();
            GL.Color(new Color(1, 1, 1, 1));
            GL.Begin(GL.TRIANGLES);
            for (int i = 0; i < indices.Length; ++i)
            {
                ushort index = indices[i];
                Vector2 vertex = vertices[index];
                Vector2 uv = uvs[index];
                GL.TexCoord2(uv.x, uv.y);
                if (settings != null)
                {
                    var setting = settings[index];
                    GL.MultiTexCoord2(2, setting.x, setting.y);
                }

                if (colors != null)
                    GL.Color(colors[index]);
                GL.Vertex3(vertex.x, vertex.y, 0);
            }

            GL.End();
            GL.PopMatrix();

            mat.SetTexture("_MainTex", null);
        }
    }
}