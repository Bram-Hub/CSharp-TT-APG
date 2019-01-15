using System.Collections.Generic;

namespace TruthTree2.FOL.Logic
{
    public class Negation : Connective
    {
        internal WFF inner;

        public Negation(WFF i)
        {
            if (i is Negation)
            {
                Negation n = (Negation)i;
                inner = n.inner;
            }
            else
            {
                inner = i;
            }
        }

        public override WFF GetNegation()
        {
            return inner;
        }

        public override HashSet<Term> GetTerms()
        {
            return inner.GetTerms();
        }

        public override HashSet<Constant> GetConstants()
        {
            return inner.GetConstants();
        }

        public override HashSet<Variable> GetFreeVariables()
        {
            return inner.GetFreeVariables();
        }

        public override WFF Rename(Term s, Term d)
        {
            return new Negation(inner.Rename(s, d));
        }

        public override bool Equals(object obj)
        {
            if (obj is Negation)
            {
                Negation n = (Negation)obj;
                return inner.Equals(n.inner);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("¬{0}", inner);
        }

        public override string ToInputString()
        {
            return string.Format("(not {0})", inner.ToInputString());
        }
    }
}
