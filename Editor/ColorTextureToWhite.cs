using UnityEngine;

namespace Editor
{
    public static class ColorTextureToWhite
    {
        private const string ShaderName = "WhiteShader";

        private static ComputeShader _computeShader;
        private static int _kernel;

        public static Texture2D ColorTexture(Texture2D original)
        {
            // load compute shader
            if (_computeShader == null)
            {
                _computeShader = Resources.Load<ComputeShader>(ShaderName);
                _kernel = _computeShader.FindKernel("CSMain");
            }

            // prepare texture for coloring
            var result = new RenderTexture(original.width, original.height, 1)
            {
                enableRandomWrite = true
            };
            result.Create();

            // prepare shader
            _computeShader.SetTexture(_kernel, "Origin", original);
            _computeShader.SetTexture(_kernel, "Result", result);

            // run shader
            _computeShader.Dispatch(_kernel, Mathf.Max(1, original.width / 16), Mathf.Max(1, original.height / 16), 1);

            // read from result
            RenderTexture.active = result;
            original.ReadPixels(new Rect(0, 0, original.width, original.height), 0, 0);
            original.Apply();

            return original;
        }
    }
}