using System.Collections.Generic;

namespace TruthTree2.FOL.Logic
{
    public class Conditional : Connective
    {
        internal WFF antecedent;
        internal WFF consequent;

        public Conditional(WFF a, WFF c)
        {
            antecedent = a;
            consequent = c;
        }

        public override HashSet<Term> GetTerms()
        {
            HashSet<Term> terms = antecedent.GetTerms();
            terms.UnionWith(consequent.GetTerms());
            return terms;
        }

        public override HashSet<Constant> GetConstants()
        {
            HashSet<Constant> cons = antecedent.GetConstants();
            cons.UnionWith(consequent.GetConstants());
            return cons;
        }

        public override HashSet<Variable> GetFreeVariables()
        {
            HashSet<Variable> vars = antecedent.GetFreeVariables();
            vars.UnionWith(consequent.GetFreeVariables());
            return vars;
        }

        public override WFF Rename(Term s, Term d)
        {
            return new Conditional(
                antecedent.Rename(s, d),
                consequent.Rename(s, d));
        }

        public override bool Equals(object obj)
        {
            if (obj is Conditional)
            {
                Conditional c = (Conditional)obj;
                return antecedent.Equals(c.antecedent) && consequent.Equals(c.consequent);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("({0} → {1})", antecedent, consequent);
        }

        public override string ToInputString()
        {
            return string.Format("(if {0} {1})", antecedent.ToInputString(), consequent.ToInputString());
        }
    }
}
