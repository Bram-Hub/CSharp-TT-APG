using System;
using System.Collections.Generic;
using System.Drawing;
using TruthTree2.FOL.Logic;

namespace TruthTree2.FOL.Proofs
{
    internal enum Rule
    {
        AndElim,
        OrElim,
        IfElim,
        IffElim,
        NotElim,
        NotIntro,
        ContradictionIntro,
        ExistentialElim,
        UniversalElim,
        EqualsElim,
        Assumption,
        DeMorgans,
        Implication,
        Equivalence,
        Magic
    }

    internal class ProofLine : Proof
    {
        internal WFF line;
        internal Rule rule;
        internal List<Proof> reasons;

        public override int LastLineNumber { get { return FirstLineNumber; } }

        public ProofLine(WFF l, Rule r, params Proof[] s)
        {
            line = l;
            rule = r;
            reasons = new List<Proof>(s);
        }

        public override Proof Find(WFF f)
        {
            if (line.Equals(f)) { return this; }

            return null;
        }

        internal override HashSet<int> getUsedProofs()
        {
            HashSet<int> used = new HashSet<int>();
            foreach(Proof p in reasons) { used.Add(p.FirstLineNumber); }
            
            return used;
        }

        public override void Clean()
        {
            // Nothing to do here
        }

        #region Drawing
        private string ruleToString(Rule r)
        {
            switch (r)
            {
                case Rule.AndElim: return "⋀ Elim";
                case Rule.Assumption: return "Assume";
                case Rule.ContradictionIntro: return "Contradiction";
                case Rule.DeMorgans: return "DeMorgan's";
                case Rule.EqualsElim: return "= Elim";
                case Rule.Equivalence: return "Equivalence";
                case Rule.ExistentialElim: return "∃ Elim";
                case Rule.IfElim: return "→ Elim";
                case Rule.IffElim: return "↔ Elim";
                case Rule.Implication: return "Implication";
                case Rule.Magic: return "MAGIC";
                case Rule.NotElim: return "¬ Elim";
                case Rule.NotIntro: return "¬ Intro";
                case Rule.OrElim: return "⋁ Elim";
                case Rule.UniversalElim: return "∀ Elim";
            }

            throw new InvalidOperationException(r.ToString());
        }

        private string getReasonLineNumbers()
        {
            List<string> lines = new List<string>();
            foreach (Proof p in reasons)
            {
                int f = p.FirstLineNumber;
                int l = p.LastLineNumber;

                if (f == l) { lines.Add(f.ToString()); }
                else { lines.Add(f + "-" + (l - 1)); }
            }

            return String.Join(", ", lines.ToArray());
        }

        private SizeF _lnsize;
        private string _reason;
        protected override void updateSizes(Graphics g)
        {
            if (g == null) { return; }

            _lnsize = g.MeasureString(line.ToString(), font);
            _reason = getReasonLineNumbers() + "  " + ruleToString(rule);
            SizeF so = g.MeasureString(_reason, fontsmall);
            _ovsize = new SizeF(
                1 + Math.Max(_lnsize.Width, so.Width),
                _lnsize.Height + so.Height);
        }

        internal override void draw(Graphics image, float imgwidth, int sidecolwidth = -1)
        {
            if (sidecolwidth < 0)
            {
                sidecolwidth = (int)image.MeasureString(FirstLineNumber + ".", font).Width;
            }

            float numbertop = (ImageSize(image).Height - image.MeasureString(FirstLineNumber + ".", font).Height) / 2;

            // Alternating highlighted lines in the proof
            if (FirstLineNumber % 2 == 1)
            {
                SolidBrush tmp = new SolidBrush(Color.FromArgb(25, 0, 0, 255));
                image.FillRectangle(tmp, 0, 0, imgwidth, ImageSize().Height);
                tmp.Dispose();
            }

            image.DrawString(FirstLineNumber + ".", font, bBlack, 0, numbertop);
            image.DrawString(line.ToString(), font, bBlack, 1 + sidecolwidth, 5);
            image.DrawString(_reason, fontsmall, bBlack, 1 + sidecolwidth, _lnsize.Height);
        }
        #endregion
    }
}
