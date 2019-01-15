using System.Collections.Generic;

namespace TruthTree2.FOL.Logic
{
    public class Biconditional : Connective
    {
        internal WFF left;
        internal WFF right;

        public Biconditional(WFF l, WFF r)
        {
            left = l;
            right = r;
        }

        public override HashSet<Term> GetTerms()
        {
            HashSet<Term> terms = left.GetTerms();
            terms.UnionWith(right.GetTerms());
            return terms;
        }

        public override HashSet<Constant> GetConstants()
        {
            HashSet<Constant> cons = left.GetConstants();
            cons.UnionWith(right.GetConstants());
            return cons;
        }

        public override HashSet<Variable> GetFreeVariables()
        {
            HashSet<Variable> vars = left.GetFreeVariables();
            vars.UnionWith(right.GetFreeVariables());
            return vars;
        }

        public override WFF Rename(Term s, Term d)
        {
            return new Biconditional(
                left.Rename(s, d),
                right.Rename(s, d));
        }

        public override bool Equals(object obj)
        {
            if (obj is Biconditional)
            {
                Biconditional b = (Biconditional)obj;
                return (left.Equals(b.left) && right.Equals(b.right)) ||
                       (left.Equals(b.right) && right.Equals(b.left));
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("({0} ↔ {1})", left, right);
        }

        public override string ToInputString()
        {
            return string.Format("(iff {0} {1})", left.ToInputString(), right.ToInputString());
        }
    }
}
