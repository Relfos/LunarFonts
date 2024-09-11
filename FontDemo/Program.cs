using LunarLabs.Fonts;
using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using SkiaSharp;
using Svg;
using System.Text;
using Svg.Skia;

namespace FontDemo
{
    class Program
    {
        private static int _fontSize = 64;

        static Bitmap ConvertToBitmap(GlyphBitmap image)
        {
            var bmp = new Bitmap(image.Width, image.Height);

            using (Graphics g = Graphics.FromImage(bmp))
                g.Clear(Color.Transparent);

            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    int pixelIndex = (x + y * image.Width) * 4;

                    byte red = image.Pixels[pixelIndex];
                    byte green = image.Pixels[pixelIndex + 1];
                    byte blue = image.Pixels[pixelIndex + 2];
                    byte alpha = image.Pixels[pixelIndex + 3];

                    Color color = Color.FromArgb(alpha, red, green, blue);
                    bmp.SetPixel(x, y, color);
                }
            }

            return bmp;
        }

        [STAThread]
        static void Main(string[] args)
        {
            OpenFont().GetAwaiter().GetResult();
        }

        public static async Task OpenFont()
        {
            var dlg = new OpenFileDialog { Filter = "Font Files (*.ttf;*.otf)|*.ttf;*.otf", InitialDirectory = Directory.GetCurrentDirectory() };
            if (dlg.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            var fontFileName = dlg.FileName;

            Console.WriteLine($"Loading {fontFileName}...");

            var bytes = File.ReadAllBytes(fontFileName);

            var font = new LunarLabs.Fonts.Font(bytes, null);
            font.SvgRender += SvgRender;

            // make this a number larger than 1 to enable SDF output
            int SDF_scale = 1;

            // here is the desired height in pixels of the output
            // the real value might not be exactly this depending on which characters are part of the input text
            int height = _fontSize * SDF_scale;
            var scale = font.IsSVG() ? (float)_fontSize / font.UnitsPerEm : font.ScaleInPixels(height);

            var phrase = "Hello World!";
            var metrics = new Dictionary<char, GlyphMetrics>();
            var bitmaps = new Dictionary<char, GlyphBitmap>();

            foreach (var codePoint in phrase)
            {
                if (metrics.ContainsKey(codePoint))
                    continue;

                var glyphMetrics = await font.GetGlyphMetrics(codePoint, scale, scale, 0, 0);
                var glyphBitmap = await font.RenderGlyph(codePoint, scale, Color.White, Color.Empty);

                if (glyphBitmap == null)
                    continue;

                metrics[codePoint] = glyphMetrics;
                bitmaps[codePoint] = glyphBitmap;
            }

            int ascent, descent, lineGap;
            font.GetFontVMetrics(out ascent, out descent, out lineGap);

            ascent = (int)Math.Round(ascent * scale);
            descent = (int)Math.Round(descent * scale);
            int baseLine = height - (int)(ascent * scale);

            int minX = int.MaxValue;
            int maxX = int.MinValue;
            int minY = int.MaxValue;
            int maxY = int.MinValue;

            var positions = new Point[phrase.Length];

            int x = 0;
            for (int i = 0; i < phrase.Length; i++)
            {
                var codePoint = phrase[i];
                GlyphMetrics glyphMetrics;

                if (!metrics.TryGetValue(codePoint, out glyphMetrics))
                    continue;

                var next = i < phrase.Length - 1 ? phrase[i + 1] : '\0';
                var kerning = font.GetKerning(codePoint, next, scale);
                var bounds = glyphMetrics.Bounds;

                int y0 = height - baseLine + (int)bounds.Top;
                int y1 = y0 + (int)bounds.Height;

                int x0 = x + (int)bounds.Left - kerning;
                int x1 = x0 + (int)bounds.Width;

                x += (int)glyphMetrics.AdvanceWidth;

                positions[i] = new Point(x0, y0);

                x1 = Math.Max(x, x1);

                minX = Math.Min(minX, x0);
                maxX = Math.Max(maxX, x1);

                minY = Math.Min(minY, y0);
                maxY = Math.Max(maxY, y1);
            }

            int realWidth = (maxX - minX) + 1;
            int realHeight = (maxY - minY) + 1;

            for (int i = 0; i < phrase.Length; i++)
            {
                positions[i].X -= minX;
                positions[i].Y -= minY;
            }

            var tempBmp = new GlyphBitmap(realWidth, realHeight, false, Color.Empty);
            {
                // draw the baseline height in blue color
                var ly = height - (baseLine + minY);
                //g.DrawLine(new Pen(Color.Blue), 0, ly, realWidth - 1, ly);

                // now draw each character
                x = 0;
                for (int i = 0; i < phrase.Length; i++)
                {
                    var ch = phrase[i];
                    GlyphBitmap glyphBitmap;

                    if (!bitmaps.TryGetValue(ch, out glyphBitmap))
                        continue;

                    var bmp = bitmaps[ch];
                    var pos = positions[i];
                    tempBmp.Draw(glyphBitmap, pos.X, pos.Y, Color.Empty);
                }
            }

            if (SDF_scale > 1)
            {
                tempBmp = DistanceFieldUtils.CreateDistanceField(tempBmp, SDF_scale, 32);
            }

            var outBmp = ConvertToBitmap(tempBmp);
            var outputFileName = Path.Combine(AppContext.BaseDirectory, "out.png");
            Console.WriteLine($"Outputting {outputFileName}...");
            outBmp.Save(outputFileName);

            System.Diagnostics.Process.Start(outputFileName);
        }

        public static async Task<GlyphBitmap> SvgRender(LunarLabs.Fonts.Font font, string svgDoc, char codePoint, int glyph, object userData)
        {
            try
            {
                var svgDocument = SvgDocument.FromSvg<SvgDocument>(svgDoc);

                // Set the ViewBox to encompass the full glyph bounding box
                int unitsPerEm = (int)font.UnitsPerEm;
                svgDocument.ViewBox = new SvgViewBox(0, -unitsPerEm, unitsPerEm, unitsPerEm);
                svgDocument.Width = svgDocument.Height = _fontSize;

                var scale = (float)_fontSize / unitsPerEm;

                int ascent, descent, lineGap;
                font.GetFontVMetrics(out ascent, out descent, out lineGap);
                var baseLine = (int)((font.UnitsPerEm - ascent) * scale);

                var glyphMetrics = await font.GetGlyphMetrics(codePoint, scale, scale, 0, 0);
                var svgRect = new RectangleF(glyphMetrics.Bounds.Left, glyphMetrics.Bounds.Top, glyphMetrics.Bounds.Width, glyphMetrics.Bounds.Height);

                var svg = SKSvg.CreateFromSvgDocument(svgDocument);
                var bitmap = new SKBitmap((int)glyphMetrics.Bounds.Width, (int)glyphMetrics.Bounds.Height, SKColorType.Rgba8888, SKAlphaType.Unpremul);

                using (var canvas = new SKCanvas(bitmap))
                    canvas.DrawPicture(svg.Picture, 0, -baseLine);

                var glyphBitmap = new GlyphBitmap(bitmap.Width, bitmap.Height, bitmap.Bytes, true);

                return glyphBitmap;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
