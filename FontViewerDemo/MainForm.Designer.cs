namespace FontViewerDemo
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.textInput = new System.Windows.Forms.TextBox();
            this.sdfCheckbox = new System.Windows.Forms.CheckBox();
            this.sdfScaleSelector = new System.Windows.Forms.NumericUpDown();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.fontHeightSelector = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.outputBox = new System.Windows.Forms.PictureBox();
            this.borderCheckbox = new System.Windows.Forms.CheckBox();
            this.butForeground = new System.Windows.Forms.Button();
            this.butBackground = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.sdfScaleSelector)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.fontHeightSelector)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.outputBox)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Input text";
            // 
            // textInput
            // 
            this.textInput.Location = new System.Drawing.Point(18, 24);
            this.textInput.Name = "textInput";
            this.textInput.Size = new System.Drawing.Size(200, 20);
            this.textInput.TabIndex = 1;
            this.textInput.Text = "Hello World!";
            this.textInput.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // sdfCheckbox
            // 
            this.sdfCheckbox.AutoSize = true;
            this.sdfCheckbox.Location = new System.Drawing.Point(327, 25);
            this.sdfCheckbox.Name = "sdfCheckbox";
            this.sdfCheckbox.Size = new System.Drawing.Size(82, 17);
            this.sdfCheckbox.TabIndex = 2;
            this.sdfCheckbox.Text = "SDF Output";
            this.sdfCheckbox.UseVisualStyleBackColor = true;
            this.sdfCheckbox.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // sdfScaleSelector
            // 
            this.sdfScaleSelector.Location = new System.Drawing.Point(415, 25);
            this.sdfScaleSelector.Maximum = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.sdfScaleSelector.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.sdfScaleSelector.Name = "sdfScaleSelector";
            this.sdfScaleSelector.Size = new System.Drawing.Size(79, 20);
            this.sdfScaleSelector.TabIndex = 3;
            this.sdfScaleSelector.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.sdfScaleSelector.Visible = false;
            this.sdfScaleSelector.ValueChanged += new System.EventHandler(this.sdfScaleSelector_ValueChanged);
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.Location = new System.Drawing.Point(18, 50);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(200, 355);
            this.listBox1.TabIndex = 4;
            this.listBox1.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            // 
            // fontHeightSelector
            // 
            this.fontHeightSelector.Location = new System.Drawing.Point(224, 24);
            this.fontHeightSelector.Maximum = new decimal(new int[] {
            128,
            0,
            0,
            0});
            this.fontHeightSelector.Minimum = new decimal(new int[] {
            16,
            0,
            0,
            0});
            this.fontHeightSelector.Name = "fontHeightSelector";
            this.fontHeightSelector.Size = new System.Drawing.Size(79, 20);
            this.fontHeightSelector.TabIndex = 5;
            this.fontHeightSelector.Value = new decimal(new int[] {
            64,
            0,
            0,
            0});
            this.fontHeightSelector.ValueChanged += new System.EventHandler(this.numericUpDown2_ValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(221, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Height";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(18, 411);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(200, 24);
            this.button1.TabIndex = 7;
            this.button1.Text = "Change Folder";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // outputBox
            // 
            this.outputBox.Location = new System.Drawing.Point(224, 50);
            this.outputBox.Name = "outputBox";
            this.outputBox.Size = new System.Drawing.Size(352, 355);
            this.outputBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.outputBox.TabIndex = 8;
            this.outputBox.TabStop = false;
            // 
            // borderCheckbox
            // 
            this.borderCheckbox.AutoSize = true;
            this.borderCheckbox.Location = new System.Drawing.Point(224, 411);
            this.borderCheckbox.Name = "borderCheckbox";
            this.borderCheckbox.Size = new System.Drawing.Size(57, 17);
            this.borderCheckbox.TabIndex = 9;
            this.borderCheckbox.Text = "Border";
            this.borderCheckbox.UseVisualStyleBackColor = true;
            this.borderCheckbox.CheckedChanged += new System.EventHandler(this.borderCheckbox_CheckedChanged);
            // 
            // butForeground
            // 
            this.butForeground.BackColor = System.Drawing.Color.Black;
            this.butForeground.Location = new System.Drawing.Point(511, 22);
            this.butForeground.Name = "butForeground";
            this.butForeground.Size = new System.Drawing.Size(21, 23);
            this.butForeground.TabIndex = 10;
            this.butForeground.UseVisualStyleBackColor = false;
            this.butForeground.Click += new System.EventHandler(this.butForeground_Click);
            // 
            // butBackground
            // 
            this.butBackground.BackColor = System.Drawing.Color.Transparent;
            this.butBackground.Location = new System.Drawing.Point(538, 22);
            this.butBackground.Name = "butBackground";
            this.butBackground.Size = new System.Drawing.Size(21, 23);
            this.butBackground.TabIndex = 11;
            this.butBackground.UseVisualStyleBackColor = false;
            this.butBackground.Click += new System.EventHandler(this.butBackground_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(655, 440);
            this.Controls.Add(this.butBackground);
            this.Controls.Add(this.butForeground);
            this.Controls.Add(this.borderCheckbox);
            this.Controls.Add(this.outputBox);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.fontHeightSelector);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.sdfScaleSelector);
            this.Controls.Add(this.sdfCheckbox);
            this.Controls.Add(this.textInput);
            this.Controls.Add(this.label1);
            this.Name = "MainForm";
            this.Text = "Font Viewer";
            ((System.ComponentModel.ISupportInitialize)(this.sdfScaleSelector)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.fontHeightSelector)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.outputBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textInput;
        private System.Windows.Forms.CheckBox sdfCheckbox;
        private System.Windows.Forms.NumericUpDown sdfScaleSelector;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.NumericUpDown fontHeightSelector;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.PictureBox outputBox;
        private System.Windows.Forms.CheckBox borderCheckbox;
        private System.Windows.Forms.Button butForeground;
        private System.Windows.Forms.Button butBackground;
    }
}

