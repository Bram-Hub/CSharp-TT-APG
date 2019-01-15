using System;
using System.Collections.Generic;
using System.Drawing;
using TruthTree2.FOL.Logic;

namespace TruthTree2.FOL.Proofs
{
    public abstract class Proof : Drawable
    {
        internal Proof parent;

        public int FirstLineNumber { get; internal set; }
        public abstract int LastLineNumber { get; }

        public abstract Proof Find(WFF f);

        internal abstract HashSet<int> getUsedProofs();
        public abstract void Clean();

        #region Drawing
        protected static int fontsize = 16;
        protected static Font font = new Font("Cambria", fontsize);
        protected static Font fontsmall = new Font("Cambria", fontsize * 3 / 4);
        protected static Brush bBlack = new SolidBrush(Color.Black);
        protected static Pen pBlack = new Pen(Color.Black);
        protected static int gapsize = 15;

        protected bool _sized = false;
        protected SizeF _ovsize;
        protected abstract void updateSizes(Graphics g);
        public SizeF ImageSize(Graphics g = null)
        {
            if (!_sized) { updateSizes(g); }

            return _ovsize;
        }
        public void Draw(Graphics image)
        {
            draw(image, ImageSize(image).Width);
        }

        internal abstract void draw(Graphics image, float imgwidth, int sidecolwidth = -1);

        public void SaveImage(string file)
        {
            Bitmap bmpproof = new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            Graphics gproof = Graphics.FromImage(bmpproof);
            SizeF psize = ImageSize(gproof);
            bmpproof = new Bitmap((int)Math.Ceiling(psize.Width), (int)Math.Ceiling(psize.Height), System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            gproof = Graphics.FromImage(bmpproof);
            gproof.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            gproof.Clear(Color.White);
            Draw(gproof);
            bmpproof.Save(file);
        }

        public bool AntiAliasText { get { return true; } }
        public bool AntiAliasDrawing { get { return false; } }
        #endregion
    }
}
