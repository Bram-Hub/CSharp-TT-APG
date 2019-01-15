using System.Collections.Generic;

namespace TruthTree2.FOL.Logic
{
    public class Identity : AtomicWFF
    {
        internal Term left;
        internal Term right;

        public Identity(Term l, Term r)
        {
            left = l;
            right = r;
        }

        public override List<Term> GetTermsInOrder()
        {
            List<Term> terms = new List<Term>();
            terms.Add(left);
            terms.Add(right);
            return terms;
        }

        public override bool ContainsTerm(Term t)
        {
            return left.Equals(t) || right.Equals(t) || left.ContainsTerm(t) || right.ContainsTerm(t);
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
            return new Identity(
                (left.Equals(s) ? d : left.Rename(s, d)),
                (right.Equals(s) ? d : right.Rename(s, d)));
        }

        public override bool Equals(object obj)
        {
            if (obj is Identity)
            {
                Identity id = (Identity)obj;

                return (left.Equals(id.left) && right.Equals(id.right)) ||
                       (left.Equals(id.right) && right.Equals(id.left));
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("({0} = {1})", left, right);
        }

        public override string ToInputString()
        {
            return string.Format("(= {0} {1})", left.ToInputString(), right.ToInputString());
        }
    }
}
