using System.Collections.Generic;

namespace TruthTree2.FOL.Logic
{
    public abstract class BasicTerm : Term
    {
        internal string name;

        public override HashSet<Term> GetTerms()
        {
            HashSet<Term> terms = new HashSet<Term>();
            terms.Add(this);
            return terms;
        }

        public override Term Rename(Term s, Term d)
        {
            return this;
        }

        public override bool ContainsTerm(Term t)
        {
            return false;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) { return false; }

            if (GetType() != obj.GetType()) { return false; }

            BasicTerm t = (BasicTerm)obj;
            return name.Equals(t.name);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
