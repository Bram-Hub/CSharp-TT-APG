using System.Collections.Generic;

namespace TruthTree2.FOL.Logic
{
    public class Function : ComplexTerm
    {
        internal string name;
        internal Term[] arguments;
        internal int arity { get { return arguments.Length; } }

        public Function(string n, params Term[] a)
        {
            name = n;
            arguments = a;
        }

        public override HashSet<Term> GetTerms()
        {
            HashSet<Term> terms = new HashSet<Term>();
            terms.Add(this);
            foreach (Term a in arguments)
            {
                terms.UnionWith(a.GetTerms());
            }

            return terms;
        }

        public override HashSet<Constant> GetConstants()
        {
            HashSet<Constant> cons = new HashSet<Constant>();
            foreach (Term a in arguments)
            {
                cons.UnionWith(a.GetConstants());
            }

            return cons;
        }

        public override HashSet<Variable> GetFreeVariables()
        {
            HashSet<Variable> vars = new HashSet<Variable>();
            foreach (Term a in arguments)
            {
                vars.UnionWith(a.GetFreeVariables());
            }

            return vars;
        }

        public override bool ContainsTerm(Term t)
        {
            foreach (Term a in arguments)
            {
                if (a.Equals(t) || a.ContainsTerm(t)) { return true; }
            }

            return false;
        }

        public override Term Rename(Term s, Term d)
        {
            Term[] rargs = new Term[arity];
            for (int a = 0; a < arity; a++)
            {
                rargs[a] = (arguments[a].Equals(s) ? d : arguments[a].Rename(s, d));
            }

            return new Function(name, rargs);
        }

        public override bool Equals(object obj)
        {
            if (obj is Function)
            {
                Function f = (Function)obj;
                if (!name.Equals(f.name)) { return false; }
                if (arity != f.arity) { return false; }

                for (int a = 0; a < arity; a++)
                {
                    if (!arguments[a].Equals(f.arguments[a])) { return false; }
                }

                return true;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            if (arguments.Length == 0) { return name; }

            return string.Format("{0}({1})", name, string.Join<Term>(", ", arguments));
        }

        public override string ToInputString()
        {
            if (arguments.Length == 0) { return name; }

            return string.Format("({0} {1})", name, string.Join<Term>(" ", arguments));
        }
    }
}
