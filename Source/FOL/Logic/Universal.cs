namespace TruthTree2.FOL.Logic
{
    public class Universal : Quantifier
    {
        public Universal(Variable v, WFF s)
        {
            variable = v;
            scope = s;
        }

        public override WFF Rename(Term s, Term d)
        {
            if (variable.Equals(s)) { return this; }

            return new Universal(
                variable,
                scope.Rename(s, d));
        }

        public override string ToString()
        {
            return string.Format("∀{0} [{1}]", variable, scope);
        }

        public override string ToInputString()
        {
            return string.Format("(forall {0} {1})", variable.ToInputString(), scope.ToInputString());
        }
    }
}
