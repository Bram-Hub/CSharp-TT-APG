using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TruthTree2.FOL.Logic;

namespace TruthTree2.FOL.Proofs
{
    internal class Subproof : Proof
    {
        internal List<ProofLine> assumptions;
        internal List<Proof> steps;

        private bool _llCalc = false;
        private int _lastLineNumber;
        public override int LastLineNumber
        {
            get
            {
                if (!_llCalc)
                {
                    _lastLineNumber = FirstLineNumber;
                    foreach (ProofLine pl in assumptions)
                    {
                        pl.FirstLineNumber = _lastLineNumber;
                        _lastLineNumber++;
                    }

                    foreach (Proof p in steps)
                    {
                        p.FirstLineNumber = _lastLineNumber;
                        _lastLineNumber = p.LastLineNumber;
                        if (p.FirstLineNumber == p.LastLineNumber) { _lastLineNumber++; }
                    }
                }

                return _lastLineNumber;
            }
        }

        internal Subproof()
        {
            assumptions = new List<ProofLine>();
            steps = new List<Proof>();
        }

        public Subproof(List<WFF> a, params Proof[] s)
        {
            assumptions = new List<ProofLine>();
            foreach (WFF f in a) { assumptions.Add(new ProofLine(f, Rule.Assumption)); }

            steps = new List<Proof>();
            foreach (Proof p in s) { p.parent = this; steps.Add(p); }
        }

        public override Proof Find(WFF f)
        {
            Proof a = assumptions.Find(pl => { return pl.line.Equals(f); });
            if (a != null) { return a; }

            foreach (Proof p in steps)
            {
                if (p is Subproof) { continue; }

                a = p.Find(f);
                if (a != null) { return a; }
            }

            if (parent == null) { return null; }

            return parent.Find(f);
        }

        internal override HashSet<int> getUsedProofs()
        {
            HashSet<int> used = new HashSet<int>();
            foreach (Proof p in steps) { used.UnionWith(p.getUsedProofs()); }

            return used;
        }

        public override void Clean()
        {
            foreach (Proof p in steps) { p.Clean(); }

            // Force the recalculation of line numbers
            int force = LastLineNumber;

            HashSet<int> used = getUsedProofs();
            List<Proof> tmp = new List<Proof>();
            foreach (Proof p in steps)
            {
                if (used.Contains(p.FirstLineNumber)) { tmp.Add(p); }
                else if (p is ProofLine)
                {
                    ProofLine pl = (ProofLine)p;
                    if (pl.line.Equals(new Contradiction())) { tmp.Add(p); }
                    else if (pl.rule == Rule.NotIntro) { tmp.Add(p); }
                }
            }

            steps = tmp;
        }

        #region Drawing
        protected override void updateSizes(Graphics g)
        {
            if (g == null) { return; }

            SizeF top = assumptions.Aggregate(new SizeF(),
                (ov, pl) =>
                {
                    SizeF ln = pl.ImageSize(g);
                    return new SizeF(
                        Math.Max(ov.Width, ln.Width),
                        ov.Height + ln.Height);
                });
            SizeF bot = steps.Aggregate(top,
                (ov, st) =>
                {
                    SizeF ln = st.ImageSize(g);
                    return new SizeF(
                        (st is Subproof ? gapsize : 0) + Math.Max(ov.Width, ln.Width),
                        ov.Height + ln.Height);
                });
            _ovsize = new SizeF(bot.Width + gapsize, bot.Height);
        }

        internal override void draw(Graphics image, float imgwidth, int sidecolwidth = -1)
        {
            if (sidecolwidth < 0)
            {
                sidecolwidth = (int)image.MeasureString(LastLineNumber + ".", font).Width;
            }

            var ostate = image.Save();
            foreach (ProofLine pl in assumptions)
            {
                pl.draw(image, imgwidth, sidecolwidth);
                image.TranslateTransform(0, pl.ImageSize().Height);
            }

            if (assumptions.Count > 0)
            {
                image.DrawLine(pBlack, sidecolwidth, 0, sidecolwidth + gapsize, 0);
            }

            foreach (Proof p in steps)
            {
                if (p is Subproof) { p.draw(image, imgwidth, sidecolwidth + gapsize); }
                else { p.draw(image, imgwidth, sidecolwidth); }
                image.TranslateTransform(0, p.ImageSize().Height);
            }

            image.Restore(ostate);
            image.DrawLine(pBlack, sidecolwidth, 10, sidecolwidth, ImageSize().Height);
        }
        #endregion
    }
}
