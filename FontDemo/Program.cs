using LunarLabs.Fonts;
using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

namespace FontDemo
{
    class Program
    {
        static Bitmap ConvertToBitmap(GlyphBitmap image)
        {
            var bmp = new Bitmap(image.Width, image.Height);
            for (int j = 0; j < image.Height; j++)
            {
                for (int i = 0; i < image.Width; i++)
                {
                    byte alpha = image.Pixels[i + j * image.Width];
                    bmp.SetPixel(i, j, Color.FromArgb(alpha, alpha, alpha, alpha));
                }
            }

            return bmp;
        }

        [STAThread]
        static void Main(string[] args)
        {
            var dlg = new OpenFileDialog { Filter = "Font Files (*.ttf)|*.ttf", InitialDirectory = Directory.GetCurrentDirectory() };
            if (dlg.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            var fontFileName = dlg.FileName;

            Console.WriteLine($"Loading {fontFileName}...");

            var bytes = File.ReadAllBytes(fontFileName);

            var font = new LunarLabs.Fonts.Font(bytes);

            // make this a number larger than 1 to enable SDF output
            int SDF_scale = 1;

            // here is the desired height in pixels of the output
            // the real value might not be exactly this depending on which characters are part of the input text
            int height = 64 * SDF_scale;
            var scale = font.ScaleInPixels(height);

            var phrase = "Hello world";
            var glyphs = new Dictionary<char, FontGlyph>();
            var bitmaps = new Dictionary<char, Bitmap>();
            foreach (var ch in phrase)
            {
                if (glyphs.ContainsKey(ch))
                {
                    continue;
                }

                var glyph = font.RenderGlyph(ch, scale);
                glyphs[ch] = glyph;
                bitmaps[ch] = ConvertToBitmap(glyph.Image);
            }

            int ascent, descent, lineGap;
            font.GetFontVMetrics(out ascent, out descent, out lineGap);
            int baseLine = height - (int)(ascent * scale);


            int minX = int.MaxValue;
            int maxX = int.MinValue;
            int minY = int.MaxValue;
            int maxY = int.MinValue;

            var positions = new Point[phrase.Length];
            
            int x = 0;
            for (int i=0; i<phrase.Length; i++)
            {
                var ch = phrase[i];
                var glyph = glyphs[ch];

                var next = i < phrase.Length - 1 ? phrase[i + 1] : '\0';

                var kerning = font.GetKerning(ch, next, scale);

                int y0 = height - baseLine + glyph.yOfs;
                int y1 = y0 + glyph.Image.Height;

                int x0 = x + glyph.xOfs - kerning;
                int x1 = x0 + glyph.Image.Width;
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

            for (int i=0; i<phrase.Length; i++)
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
                for (int i=0; i<phrase.Length; i++)
                {
                    var ch = phrase[i];
                    var glyph = glyphs[ch];
                    var bmp = bitmaps[ch];
                    var pos = positions[i];
                    tempBmp.Draw(glyph.Image, pos.X, pos.Y);
                }
            }

            if (SDF_scale > 1)
            {
                tempBmp = DistanceFieldUtils.CreateDistanceField(tempBmp, SDF_scale, 32);
            }

            var outBmp = ConvertToBitmap(tempBmp);
            var outputFileName = "output.png";
            Console.WriteLine($"Outputting {outputFileName}...");
            outBmp.Save(outputFileName);
        }
    }
}
