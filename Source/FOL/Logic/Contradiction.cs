using System.Collections.Generic;

namespace TruthTree2.FOL.Logic
{
    class Contradiction : AtomicWFF
    {
        public override bool ContainsTerm(Term t)
        {
            return false;
        }

        public override bool Equals(object obj)
        {
            if (obj is Contradiction) { return true; }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override HashSet<Constant> GetConstants()
        {
            return new HashSet<Constant>();
        }

        public override HashSet<Variable> GetFreeVariables()
        {
            return new HashSet<Variable>();
        }

        public override HashSet<Term> GetTerms()
        {
            return new HashSet<Term>();
        }

        public override List<Term> GetTermsInOrder()
        {
            return new List<Term>();
        }

        public override WFF Rename(Term s, Term d)
        {
            return this;
        }

        public override string ToString()
        {
            return "⏊";
            //return "ϟ";
        }

        public override string ToInputString()
        {
            return "false";
        }
    }
}
