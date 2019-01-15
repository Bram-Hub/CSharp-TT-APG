using System.Collections.Generic;

namespace TruthTree2.FOL.Logic
{
    public abstract class Quantifier : ComplexWFF
    {
        internal Variable variable;
        internal WFF scope;

        public override HashSet<Term> GetTerms()
        {
            HashSet<Term> terms = scope.GetTerms();
            terms.RemoveWhere(t => { return t.ContainsTerm(variable) || t.Equals(variable); });
            return terms;
        }

        public override HashSet<Constant> GetConstants()
        {
            return scope.GetConstants();
        }

        public override HashSet<Variable> GetFreeVariables()
        {
            HashSet<Variable> vars = scope.GetFreeVariables();
            vars.Remove(variable);
            return vars;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) { return false; }

            if (GetType() != obj.GetType()) { return false; }

            Quantifier q = (Quantifier)obj;
            if (variable.Equals(q.variable)) { return scope.Equals(q.scope); }

            WFF renamed = q.scope.Rename(q.variable, variable);
            return scope.Equals(renamed);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
