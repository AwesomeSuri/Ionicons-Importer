using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Editor
{
    public static class SvgConverter
    {
        public static void ConvertToPNG(Sprite svgSprite,
            int resolution, int antiAliasing, string outputDir,
            Func<Texture2D, Texture2D> imageProcessing = null)
        {
            var mat = CreateSvgSpriteMaterial();

            var size = svgSprite.bounds.size;
            size /= Mathf.Max(size.x, size.y);
            size *= Mathf.Max(16, resolution);
            var width = Mathf.FloorToInt(size.x);
            var height = Mathf.FloorToInt(size.y);

            // get texture
            var texture = VectorUtils.RenderSpriteToTexture2D(svgSprite, width, height, mat, antiAliasing);
            Object.DestroyImmediate(mat);

            if (imageProcessing != null)
            {
                texture = imageProcessing(texture);
            }

            // create sprite
            var bytes = texture.EncodeToPNG();
            var file = File.Open(Path.Combine(outputDir, $"{svgSprite.name}.png"), FileMode.Create);
            var binary = new BinaryWriter(file);
            binary.Write(bytes);
            file.Close();
        }

        private static Material _vectorMat;

        private static Material CreateSvgSpriteMaterial()
        {
            if (_vectorMat == null)
            {
                const string vectorMatPath = "Packages/com.unity.vectorgraphics/Runtime/Materials/Unlit_Vector.mat";
                _vectorMat = AssetDatabase.LoadMainAssetAtPath(vectorMatPath) as Material;
            }

            return new Material(_vectorMat);
        }
    }
}