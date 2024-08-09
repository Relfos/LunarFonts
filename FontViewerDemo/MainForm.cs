using LunarLabs.Fonts;
using SkiaSharp;
using Svg;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FontViewerDemo
{
    public partial class MainForm : Form
    {
        private string _selectedPath;
        private static int _fontSize;

        public MainForm()
        {
            InitializeComponent();

            LoadFontPath(@"C:\Windows\Fonts");
        }

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

        static async Task<Bitmap> GenerateOutput(string fontFileName, string phrase, int pixels, int SDF_scale)
        {
            var bytes = File.ReadAllBytes(fontFileName);

            var font = new LunarLabs.Fonts.Font(bytes);
            font.SvgRender += SvgRender;

            int height = pixels * SDF_scale;
            var scale = font.ScaleInPixels(height);

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
                tempBmp = DistanceFieldUtils.CreateDistanceField(tempBmp, SDF_scale, 16 * SDF_scale);
            }

            return ConvertToBitmap(tempBmp, font.HasSVG());
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

        private void button1_Click(object sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            DialogResult result = dialog.ShowDialog();
            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath)) // check result.
            {
                LoadFontPath(dialog.SelectedPath);
            }
        }

        private void LoadFontPath(string selectedPath)
        {
            this._selectedPath = selectedPath;

            List<string> files = new List<string>();

            files.AddRange(Directory.GetFiles(selectedPath, "*.ttf"));
            files.AddRange(Directory.GetFiles(selectedPath, "*.otf"));

            files.Sort();

            listBox1.Items.Clear();

            foreach (var file in files)
                listBox1.Items.Add(Path.GetFileName(file));

            listBox1.SelectedIndex = 0;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            sdfScaleSelector.Visible = sdfCheckbox.Checked;
            RefreshOutput();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty( textInput.Text))
            {
                return;
            }

            RefreshOutput();
        }

        private async Task RefreshOutput()
        {
            var fontFileName = _selectedPath + @"\" + listBox1.SelectedItem;
            var sdfScale = sdfCheckbox.Checked ? (int)sdfScaleSelector.Value : 1;
            _fontSize = (int)fontHeightSelector.Value;
            outputBox.Image = await GenerateOutput(fontFileName, textInput.Text, _fontSize, sdfScale);
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            RefreshOutput();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshOutput();
        }

        private void sdfScaleSelector_ValueChanged(object sender, EventArgs e)
        {
            RefreshOutput();
        }

        private void borderCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            outputBox.BorderStyle = borderCheckbox.Checked ? BorderStyle.FixedSingle : BorderStyle.None;
        }
    }
}
