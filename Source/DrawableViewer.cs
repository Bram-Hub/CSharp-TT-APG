using System;
using System.Drawing;
using System.Windows.Forms;

namespace TruthTree2
{
    internal partial class DrawableViewer : Form
    {
        private Bitmap image;
        private SizeF size;

        public DrawableViewer(Drawable d, string title)
        {
            InitializeComponent();
            Icon = TruthTree2.Properties.Resources.Icon;

            image = new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            Graphics g = Graphics.FromImage(image);
            size = d.ImageSize(g);
            image = new Bitmap((int)Math.Ceiling(size.Width), (int)Math.Ceiling(size.Height), System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            g = Graphics.FromImage(image);
            g.Clear(Color.White);
            if (d.AntiAliasDrawing) { g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias; }
            if (d.AntiAliasText) { g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit; }
            d.Draw(g);

            pbImage.Image = image;
            pbImage.Size = image.Size;

            Text = title;

            Size diff = Size - ClientSize;
            MaximumSize = image.Size + diff;
        }
    }
}
