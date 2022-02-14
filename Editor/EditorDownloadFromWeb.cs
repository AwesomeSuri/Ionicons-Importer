using UnityEditor;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Editor
{
    /// <summary>
    /// https://blog.cyberiansoftware.com.ar/post/149707644965/web-requests-from-unity-editor
    /// </summary>
    public static class EditorDownloadFromWeb
    {
        private static UnityWebRequest WWW { get; set; }
        private static UnityAction<bool> _callback;

        public static void DownloadFile(string url, string targetUrl, UnityAction<bool> callback = null)
        {
            WWW = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET);
            WWW.downloadHandler = new DownloadHandlerFile(targetUrl);
            WWW.SendWebRequest();

            _callback = callback;

            EditorApplication.update += Update;
        }

        private static void Update()
        {
            if (!WWW.isDone) return;

            EditorApplication.update -= Update;

            var success = WWW.result == UnityWebRequest.Result.Success;
            WWW.Dispose();

            _callback?.Invoke(success);
        }
    }
}