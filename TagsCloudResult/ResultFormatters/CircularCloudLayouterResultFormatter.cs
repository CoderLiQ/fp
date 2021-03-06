﻿using System.Drawing;
using TagsCloudResult.DataProviders;
using TagsCloudResult.Loggers;
using TagsCloudResult.Settings;

namespace TagsCloudResult.ResultFormatters
{
    public class CircularCloudLayouterResultFormatter : IResultFormatter
    {
        private readonly IDataProvider dataProvider;
        private readonly ICloudSettings cloudSettings;
        private readonly IFontSettings fontSettings;
        private readonly ILogger logger;

        public CircularCloudLayouterResultFormatter(IDataProvider dataProvider, ICloudSettings cloudSettings,
            IFontSettings fontSettings, ILogger logger)
        {
            this.dataProvider = dataProvider;
            this.cloudSettings = cloudSettings;
            this.fontSettings = fontSettings;
            this.logger = logger;
        }
        public Result<None> GenerateResult(string outputFileName)
        {
            return Result.OfAction(() => PrepareResult(outputFileName))
                .OnFail(logger.Log);
        }

        private void PrepareResult(string outputFileName)
        {
            using (var bitmap = new Bitmap(cloudSettings.Size.Width, cloudSettings.Size.Height))
            {
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    foreach (var entry in dataProvider.GetData().Value)
                    {
                        var font = new Font(fontSettings.FontFamily, 10);
                        var generatedFont = GetFont(graphics, entry.Text, entry.Rectangle.Size, font);

                        graphics.DrawString(entry.Text, generatedFont, fontSettings.Brush, entry.Rectangle);

                    }
                    bitmap.Save(outputFileName);
                }
            }
        }

        private Font GetFont(Graphics graphics, string longString, Size room, Font preferredFont)
        {
            var realSize = graphics.MeasureString(longString, preferredFont);
            var heightScaleRatio = room.Height / realSize.Height;
            var widthScaleRatio = room.Width / realSize.Width;

            var scaleRatio = heightScaleRatio < widthScaleRatio ? heightScaleRatio : widthScaleRatio;

            var scaleFontSize = preferredFont.Size * scaleRatio;

            return new Font(preferredFont.FontFamily, scaleFontSize - 1 > 0 ? scaleFontSize - 1 : scaleFontSize);
        }
    }
}
