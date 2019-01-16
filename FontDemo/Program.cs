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

            int height = 64;
            var scale = font.ScaleInPixels(height);

            var phrase = "Hellog yorld";
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

            int outWidth = 0;
            int realMaxHeight = 0;
            foreach (var ch in phrase)
            {
                var glyph = glyphs[ch];
                outWidth += glyph.xAdvance;

                int y0 = height - baseLine + glyph.yOfs;
                int y1 = y0 + glyph.Image.Height;
                realMaxHeight = Math.Max(realMaxHeight, y1);
            }

            baseLine += (height - realMaxHeight);
            int outHeight = realMaxHeight;

            var outBmp = new Bitmap(outWidth, outHeight);
            using (var g = Graphics.FromImage(outBmp))
            {
                // draw the baseline height in blue color
                var ly = -1 + outHeight - baseLine;
                g.DrawLine(new Pen(Color.Blue), 0, ly, outWidth - 1, ly);

                // now draw each character
                int x = 0;
                foreach (var ch in phrase)
                {
                    var glyph = glyphs[ch];
                    var bmp = bitmaps[ch];
                    g.DrawImage(bmp, x, height - baseLine + glyph.yOfs);

                    x += glyph.xAdvance;
                }
            }

            var outputFileName = "output.png";
            Console.WriteLine($"Outputting {outputFileName}...");
            outBmp.Save(outputFileName);
        }
    }
}
