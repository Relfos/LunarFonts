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

namespace FontDemo
{
    class Program
    {
        private static int _fontSize = 64;

        static Bitmap ConvertToBitmap(GlyphBitmap image, bool fullColor)
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

                    Color color = (fullColor ? Color.FromArgb(alpha, red, green, blue) : Color.FromArgb(alpha, 0, 0, 0));
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

            var font = new LunarLabs.Fonts.Font(bytes);
            font.SvgRender += SvgRender;

            // make this a number larger than 1 to enable SDF output
            int SDF_scale = 1;

            // here is the desired height in pixels of the output
            // the real value might not be exactly this depending on which characters are part of the input text
            int height = _fontSize * SDF_scale;
            var scale = font.ScaleInPixels(height);

            var phrase = "Hello World!";
            var glyphs = new Dictionary<char, FontGlyph>();
            var bitmaps = new Dictionary<char, Bitmap>();
            foreach (var ch in phrase)
            {
                if (glyphs.ContainsKey(ch))
                    continue;

                var glyph = await font.RenderGlyph(ch, scale);
                var isSVG = font.IsSVG(ch);

                if (glyph == null)
                    continue;

                glyphs[ch] = glyph;
                bitmaps[ch] = ConvertToBitmap(glyph.Image, isSVG);
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
                var ch = phrase[i];
                FontGlyph glyph;

                if (!glyphs.TryGetValue(ch, out glyph))
                    continue;

                var next = i < phrase.Length - 1 ? phrase[i + 1] : '\0';

                var kerning = font.GetKerning(ch, next, scale);

                int y0 = height - baseLine + glyph.yOfs;
                int y1 = y0 + glyph.Image.Height;

                int x0 = x + glyph.xOfs - kerning;
                int x1 = x0 + glyph.Image.Width;

                font.GetCodepointHMetrics(ch, out int ax, out int lsb);

                font.GetGlyphBitmapBox(ch, scale, scale, 0, 0, out int c_x1, out int c_y1, out int c_x2, out int c_y2);

                int y = ascent + c_y1;

                x += glyph.xAdvance;

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

            var tempBmp = new GlyphBitmap(realWidth, realHeight);
            {
                // draw the baseline height in blue color
                var ly = height - (baseLine + minY);
                //g.DrawLine(new Pen(Color.Blue), 0, ly, realWidth - 1, ly);

                // now draw each character
                x = 0;
                for (int i = 0; i < phrase.Length; i++)
                {
                    var ch = phrase[i];
                    FontGlyph glyph;

                    if (!glyphs.TryGetValue(ch, out glyph))
                        continue;

                    var bmp = bitmaps[ch];
                    var pos = positions[i];
                    tempBmp.Draw(glyph.Image, pos.X, pos.Y);
                }
            }

            if (SDF_scale > 1)
            {
                tempBmp = DistanceFieldUtils.CreateDistanceField(tempBmp, SDF_scale, 32);
            }

            var outBmp = ConvertToBitmap(tempBmp, font.HasSVG());
            var outputFileName = Path.Combine(AppContext.BaseDirectory, "out.png");
            Console.WriteLine($"Outputting {outputFileName}...");
            outBmp.Save(outputFileName);

            System.Diagnostics.Process.Start(outputFileName);
        }

        public static async Task<GlyphBitmap> SvgRender(LunarLabs.Fonts.Font font, SvgDocument svgDoc, int glyph)
        {
            var svg = new Svg.Skia.SKSvg();

            // Set the ViewBox to encompass the full glyph bounding box
            int unitsPerEm = (int)font.UnitsPerEm;
            svgDoc.ViewBox = new SvgViewBox(0, -unitsPerEm, unitsPerEm, unitsPerEm);
            svgDoc.Width = svgDoc.Height = _fontSize;

            var scale = font.ScaleInPixels(svgDoc.Height.Value);

            int advanceWidth, leftSideBearing;
            font.GetGlyphHMetrics(glyph, out advanceWidth, out leftSideBearing);
            int xAdvance = (int)System.Math.Floor(advanceWidth * scale);

            int ascent, descent, lineGap;
            font.GetFontVMetrics(out ascent, out descent, out lineGap);
            int baseLine = (int)svgDoc.Height.Value - (int)(ascent * scale);

            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(svgDoc.GetXML())))
            {
                svg.Load(stream);

                var newHeight = _fontSize;
                var newWidth = xAdvance + 1;
                var bitmap = new SKBitmap((int)newWidth, (int)newHeight, SKColorType.Rgba8888, SKAlphaType.Unpremul);

                using (var canvas = new SKCanvas(bitmap))
                    canvas.DrawPicture(svg.Picture, 0, -baseLine);

                var glyphBitmap = new GlyphBitmap(bitmap.Width, bitmap.Height, bitmap.Bytes);

                return glyphBitmap;
            }
        }
    }
}
