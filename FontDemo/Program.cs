using LunarLabs.Fonts;
using System;
using System.IO;
using System.DrawingCore;

namespace FontDemo
{
    class Program
    {
        static void ExportPNG(PixelTarget image, string outputName)
        {
            var bmp = new Bitmap(image.Width, image.Height);
            for (int j = 0; j < image.Height; j++)
            {
                for (int i = 0; i < image.Width; i++)
                {
                    byte alpha = image.Pixels[i + j * image.Width];
                    bmp.SetPixel(i, j, Color.FromArgb(alpha, 255, 255, 255));
                }
            }

            bmp.Save(outputName);
        }

        static void Main(string[] args)
        {
            var fontName = "steelfish.ttf";

            Console.WriteLine("Loading "+fontName+"...");

            var bytes = File.ReadAllBytes(fontName);

            var font = new LunarLabs.Fonts.Font(bytes);

            var result = font.RenderGlyph('A', 12);
            ExportPNG(result.Image, "output.png");
        }
    }
}
