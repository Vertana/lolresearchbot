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
        private readonly IConfigurationRoot _config;
        private readonly SystemFileService _fileService;
        private readonly LeagueFileCacheService _fileCache;

        public ImageService(
            IConfigurationRoot config,
            SystemFileService fileService,
            LeagueFileCacheService fileCache)
        {
            _config = config;
            _fileService = fileService;
            _fileCache = fileCache;

            if (Directory.Exists(imageFolder))
                _fileService.ClearCache(imageFolder, cacheSize);
            else
             Directory.CreateDirectory(imageFolder);
        }

        private string _imageFolder { get; set; }

        private string imageFolder
        {
            get
            {
                if (File.Exists("/.dockerenv")) // We check if we're running in a Docker container.
                {
                    _imageFolder = "/data/league/match_images"; // Yes? Use /data/ mounted volume.
                    return _imageFolder;
                }

                _imageFolder = Path.Combine(_fileCache.leagueCacheFolder, "match_images/");
                return _imageFolder;
            }
        }

        private int _cacheSize { get; set; }

        private int cacheSize
        {
            get
            {
                _cacheSize = Convert.ToInt32(_config["systemOptions:imageCacheSize"]);
                return _cacheSize;
            }
        }

        public string CheckForImageFile(string fileName)
        {
            var file = fileName + ".png";
            var imageLocation = Path.Combine(imageFolder, file);
            if ( File.Exists(imageLocation))
                return imageLocation;
            else
                return null;
        }

        public string CreateTextImage(string input, string fileName)
        {
            var file = fileName + ".png";
            var imageLocation = Path.Combine(imageFolder, file);
            if (_fileService.CheckDirectoryAccess(imageFolder))
            {
                if (!File.Exists(imageLocation))
                {
                    Font font;
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                        font = new Font("Overpass Mono", 18, FontStyle.Regular);
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        font = new Font("Menlo", 18, FontStyle.Regular);
                    }
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
    }
}