using LunarLabs.Fonts;
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
        private string selectedPath;

        public MainForm()
        {
            InitializeComponent();

            LoadFontPath(@"C:\Windows\Fonts");
        }

        static Bitmap ConvertToBitmap(GlyphBitmap image)
        {
            var bmp = new Bitmap(image.Width, image.Height);
            for (int j = 0; j < image.Height; j++)
            {
                for (int i = 0; i < image.Width; i++)
                {
                    byte alpha = image.Pixels[i + j * image.Width];
                    bmp.SetPixel(i, j, Color.FromArgb(alpha, 0, 0, 0));
                }
            }

            return bmp;
        }

        static Bitmap GenerateOutput(string fontFileName, string phrase, int pixels, int SDF_scale)
        {
            var bytes = File.ReadAllBytes(fontFileName);

            var font = new LunarLabs.Fonts.Font(bytes);

            int height = pixels * SDF_scale;
            var scale = font.ScaleInPixels(height);

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
            for (int i = 0; i < phrase.Length; i++)
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
                    var glyph = glyphs[ch];
                    var bmp = bitmaps[ch];
                    var pos = positions[i];
                    tempBmp.Draw(glyph.Image, pos.X, pos.Y);
                }
            }

            if (SDF_scale > 1)
            {
                tempBmp = DistanceFieldUtils.CreateDistanceField(tempBmp, SDF_scale, 16 * SDF_scale);
            }

            return ConvertToBitmap(tempBmp);
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
            this.selectedPath = selectedPath;

            var files = Directory.GetFiles(selectedPath, "*.ttf");
            listBox1.Items.Clear();
            foreach (var file in files)
            {
                listBox1.Items.Add(Path.GetFileNameWithoutExtension(file));
            }

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

        private void RefreshOutput()
        {
            var fontFileName = selectedPath + @"\" + listBox1.SelectedItem + ".ttf";
            var sdfScale = sdfCheckbox.Checked ? (int)sdfScaleSelector.Value : 1;
            outputBox.Image = GenerateOutput(fontFileName, textInput.Text, (int)fontHeightSelector.Value, sdfScale);
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
