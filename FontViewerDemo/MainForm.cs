using HarfBuzzSharp;
using LunarLabs.Fonts;
using SkiaSharp;
using Svg;
using Svg.Skia;
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
        private static Color _foregroundColor;
        private static Color _backgroundColor;

        private static Random _random = new Random();

        public MainForm()
        {
            InitializeComponent();

            LoadFontPath(@"C:\Windows\Fonts");
        }

        static Bitmap ConvertToBitmap(GlyphBitmap image)
        {
            var bmp = new Bitmap(image.Width, image.Height);

            using (Graphics g = Graphics.FromImage(bmp))
                g.Clear(_backgroundColor);

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

        static async Task<Bitmap> GenerateOutput(string fontFileName, string phrase, int pixels, int SDF_scale)
        {
            var bytes = File.ReadAllBytes(fontFileName);

            var font = new LunarLabs.Fonts.Font(bytes, null);
            font.SvgRender += SvgRender;

            int height = pixels * SDF_scale;
            var svgScale = (float)height / font.UnitsPerEm;
            var scale = font.IsSVG() ? (float)_fontSize / font.UnitsPerEm : font.ScaleInPixels(height);

            var metrics = new Dictionary<char, GlyphMetrics>();
            var bitmaps = new Dictionary<char, GlyphBitmap>();

            foreach (var codePoint in phrase)
            {
                if (metrics.ContainsKey(codePoint))
                    continue;

                var glyphMetrics = await font.GetGlyphMetrics(codePoint, scale, scale, 0, 0);
                var glyphBitmap = await font.RenderGlyph(codePoint, scale, _foregroundColor, _backgroundColor);

                if (glyphBitmap == null)
                    continue;

                metrics[codePoint] = glyphMetrics;
                bitmaps[codePoint] = glyphBitmap;
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

            var tempBmp = new GlyphBitmap(realWidth, realHeight, false, _backgroundColor);
            {
                // draw the baseline height in blue color
                var ly = height - (baseLine + minY);

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
                    tempBmp.Draw(glyphBitmap, pos.X, pos.Y, _backgroundColor);
                }
            }

            if (SDF_scale > 1)
            {
                tempBmp = DistanceFieldUtils.CreateDistanceField(tempBmp, SDF_scale, 16 * SDF_scale);
            }

            return ConvertToBitmap(tempBmp);
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
            _foregroundColor = butForeground.BackColor;
            _backgroundColor = butBackground.BackColor;
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

        private void butForeground_Click(object sender, EventArgs e)
        {
            using (ColorDialog colorDialog = new ColorDialog())
            {
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    butForeground.BackColor = colorDialog.Color;

                    RefreshOutput();
                }
            }
        }

        private void butBackground_Click(object sender, EventArgs e)
        {
            using (ColorDialog colorDialog = new ColorDialog())
            {
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    butBackground.BackColor = colorDialog.Color;

                    RefreshOutput();
                }
            }
        }
    }
}
