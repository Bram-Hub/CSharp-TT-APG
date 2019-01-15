using System.Collections.Generic;

namespace TruthTree2.FOL.Logic
{
    public abstract class Term
    {
        public abstract HashSet<Term> GetTerms();
        public abstract HashSet<Constant> GetConstants();
        public abstract HashSet<Variable> GetFreeVariables();
        public abstract Term Rename(Term s, Term d);
        public abstract bool ContainsTerm(Term t);

        public abstract override bool Equals(object obj);
        public abstract override string ToString();
        public abstract string ToInputString();
        public override int GetHashCode() { return ToString().GetHashCode(); }
    }
}
