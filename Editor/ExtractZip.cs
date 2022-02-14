using System.IO;
using System.IO.Compression;
using MiscUtil.IO;

namespace Editor
{
    /// <summary>
    /// https://forum.unity.com/threads/extracting-zip-files.472537/
    /// </summary>
    public static class ExtractZip
    {
        public static void ExtractFiles(string zipPath, string outputDir, bool deleteZip)
        {
            if (!File.Exists(zipPath)) return;

            // create output dir
            if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);

            // read zip file
            var zipArchive = ZipFile.Open(zipPath, ZipArchiveMode.Read);

            foreach (var entry in zipArchive.Entries)
            {
                var buffer = new byte[4096];
                var zipStream = entry.Open();

                var path = Path.Combine(outputDir, entry.Name);

                using var streamWriter = File.Create(path);
                StreamUtil.Copy(zipStream, streamWriter, buffer);

                zipStream.Close();
                streamWriter.Close();
            }

            zipArchive.Dispose();

            if (deleteZip)
            {
                File.Delete(zipPath);
            }
        }
    }
}