using System;
using System.IO;

namespace LolResearchBot.Services
{
    public class SystemFileService
    {
        private const string TEMP_FILE = "tempFile.tmp";


        /// <summary>
        ///     Checks the ability to create and write to a file in the supplied directory.
        /// </summary>
        /// <param name="directory">String representing the directory path to check.</param>
        /// <returns>True if successful; otherwise false.</returns>
        public bool CheckDirectoryAccess(string directory)
        {
            var success = false;
            var fullPath = directory + TEMP_FILE;

            if (Directory.Exists(directory))
                try
                {
                    using (var fs = new FileStream(fullPath, FileMode.CreateNew,
                        FileAccess.Write))
                    {
                        fs.WriteByte(0xff);
                    }

                    if (File.Exists(fullPath))
                    {
                        File.Delete(fullPath);
                        success = true;
                        return success;
                    }

                    return success;
                }
                catch (Exception)
                {
                    success = false;
                    return success;
                }

            Directory.CreateDirectory(directory);
            CheckDirectoryAccess(directory);
            return success;
        }

        //If the cache directory is over the size set in config.json, delete all files that were last accessed more than 2 days ago.
        public void ClearCache(string cacheFolder, int cacheSize)
        {
            var dirInfo = new DirectoryInfo(cacheFolder);
            var dirSize = DirSize(dirInfo);
            if (Directory.Exists(cacheFolder) && dirSize >= cacheSize)
            {
                var files = Directory.GetFiles(cacheFolder);

                foreach (var file in files)
                {
                    var fi = new FileInfo(file);
                    if (fi.LastAccessTime < DateTime.Now.AddDays(-2))
                        fi.Delete();
                }
            }
        }

        private static long DirSize(DirectoryInfo d)
        {
            long Size = 0;
            // Add file sizes.
            var fis = d.GetFiles();
            foreach (var fi in fis) Size += fi.Length;
            // Add subdirectory sizes.
            var dis = d.GetDirectories();
            foreach (var di in dis) Size += DirSize(di);
            //1048576 is 2^20 - this will convert our bytes into MB
            return Size / 1048576;
        }
    }
}