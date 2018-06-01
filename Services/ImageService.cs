using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;


//Example taken from https://github.com/Aux/Discord.Net-Example/blob/1.0/src/Services/LoggingService.cs
namespace LolResearchBot.Services
{
    public class ImageService
    {
        private const string TEMP_FILE = "tempFile.tmp";
        private readonly IConfigurationRoot _config;

        public ImageService(
            IConfigurationRoot config)
        {
            _config = config;
            if (Directory.Exists(imageFolder))
                ClearCache(imageFolder);
        }

        private string _imageFolder { get; set; }

        private string imageFolder
        {
            get
            {
                if (File.Exists("/.dockerenv")) // We check if we're running in a Docker container.
                {
                    _imageFolder = "/data/images/"; // Yes? Use /data/ mounted volume.
                    return _imageFolder;
                }

                _imageFolder = _config["systemOptions:imageFolder"];
                return _imageFolder;
            }
        }

        private int _cacheSize { get; set; }

        private int cacheSize
        {
            get
            {
                _cacheSize = Convert.ToInt32(_config["systemOptions:cacheSize"]);
                return _cacheSize;
            }
        }

        public string CreateTextImage(string input, string fileName)
        {
            var file = fileName + ".png";
            var imageLocation = Path.Combine(imageFolder, file);
            if (CheckDirectoryAccess(imageFolder))
            {
                if (!File.Exists(imageLocation))
                {
                    Font font;
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                        font = new Font("Overpass Mono", 18, FontStyle.Regular);
                    else
                        font = new Font("Consolas Mono", 18, FontStyle.Regular);

                    var img = DrawTextImage(input, font, Color.WhiteSmoke, Color.Black);
                    img.Save(imageLocation);
                    return imageLocation;
                }

                return imageLocation;
            }

            return null;
        }

        private Image DrawTextImage(string gameText, Font font, Color textColor, Color backColor)
        {
            return DrawTextImage(gameText, font, textColor, backColor, Size.Empty);
        }

        private Image DrawTextImage(string gameText, Font font, Color textColor, Color backColor, Size minSize)
        {
            //first, create a dummy bitmap just to get a graphics object
            SizeF textSize;
            using (Image img = new Bitmap(1, 1))
            {
                using (var drawing = Graphics.FromImage(img))
                {
                    //measure the string to see how big the image needs to be
                    textSize = drawing.MeasureString(gameText, font);
                    if (!minSize.IsEmpty)
                    {
                        textSize.Width = textSize.Width > minSize.Width ? textSize.Width : minSize.Width;
                        textSize.Height = textSize.Height > minSize.Height ? textSize.Height : minSize.Height;
                    }
                }
            }

            //create a new image of the right size
            Image retImg = new Bitmap((int) textSize.Width + 15, (int) textSize.Height);
            using (var drawing = Graphics.FromImage(retImg))
            {
                //paint the background
                drawing.Clear(backColor);

                //create a brush for the text
                using (Brush textBrush = new SolidBrush(textColor))
                {
                    drawing.SmoothingMode = SmoothingMode.AntiAlias;
                    drawing.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                    drawing.DrawString(gameText, font, textBrush, 0, 0);
                    drawing.Save();
                }
            }

            return retImg;
        }

        /// <summary>
        ///     Checks the ability to create and write to a file in the supplied directory.
        /// </summary>
        /// <param name="directory">String representing the directory path to check.</param>
        /// <returns>True if successful; otherwise false.</returns>
        private static bool CheckDirectoryAccess(string directory)
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
        private void ClearCache(string imageFolder)
        {
            var dirInfo = new DirectoryInfo(imageFolder);
            var dirSize = DirSize(dirInfo);
            if (Directory.Exists(imageFolder) && dirSize >= cacheSize)
            {
                var files = Directory.GetFiles(imageFolder);

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