namespace TruthTree2.FOL.Logic
{
    public class Existential : Quantifier
    {
        public Existential(Variable v, WFF s)
        {
            variable = v;
            scope = s;
        }

        public override WFF Rename(Term s, Term d)
        {
            if (variable.Equals(s)) { return this; }

            return new Existential(
                variable,
                scope.Rename(s, d));
        }

        public override string ToString()
        {
            return string.Format("∃{0} [{1}]", variable, scope);
        }

        public override string ToInputString()
        {
            return string.Format("(exists {0} {1})", variable.ToInputString(), scope.ToInputString());
        }
    }
}
