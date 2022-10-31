using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Watermark.Api.Extensions;

public static class ImageExtensions
{
    const string text = "burakneis.com";
    const float WatermarkPadding = 18f;
    const string WatermarkFont = "Roboto";
    const float WatermarkFontSize = 64f;

    public static async Task<bool> AddWatermark(this string path, string outputPath)
    {
        try
        {
            using var image = await Image.LoadAsync(path);

            FontFamily fontFamily;

            if (!SystemFonts.TryGet(WatermarkFont, out fontFamily))
                throw new Exception($"Couldn't find font {WatermarkFont}");

            var font = fontFamily.CreateFont(WatermarkFontSize, FontStyle.Regular);

            var options = new TextOptions(font)
            {
                Dpi = 72,
                KerningMode = KerningMode.Standard
            };

            var rect = TextMeasurer.Measure(text, options);

            image.Mutate(x => x.DrawText(
                text,
                font,
                new Color(Rgba32.ParseHex("#FFFFFFEE")),
                new PointF(image.Width - rect.Width - WatermarkPadding,
                        image.Height - rect.Height - WatermarkPadding)));

            await image.SaveAsJpegAsync(outputPath);
            return true;
        }
        catch
        {
            return false;
        }
    }
}