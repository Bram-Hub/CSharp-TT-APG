using System.Collections.Generic;

namespace TruthTree2.FOL.Logic
{
    public abstract class WFF
    {
        public abstract HashSet<Term> GetTerms();
        public abstract HashSet<Constant> GetConstants();
        public abstract HashSet<Variable> GetFreeVariables();
        public virtual WFF GetNegation() { return new Negation(this); }
        public abstract WFF Rename(Term s, Term d);

        public abstract override bool Equals(object obj);
        public abstract override string ToString();
        public abstract string ToInputString();
        public override int GetHashCode() { return ToString().GetHashCode(); }
    }
}
