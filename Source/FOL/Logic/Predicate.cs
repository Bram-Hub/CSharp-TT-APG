using System.Collections.Generic;
using System.Linq;

namespace TruthTree2.FOL.Logic
{
    public class Predicate : AtomicWFF
    {
        internal string name;
        internal Term[] arguments;
        internal int arity { get { return arguments.Length; } }

        public Predicate(string n, params Term[] a)
        {
            name = n;
            arguments = a;
        }

        public override List<Term> GetTermsInOrder()
        {
            return new List<Term>(arguments);
        }

        public override bool ContainsTerm(Term t)
        {
            return arguments.Any(a => { return a.Equals(t) || a.ContainsTerm(t); });
        }

        public override HashSet<Term> GetTerms()
        {
            HashSet<Term> terms = new HashSet<Term>();
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

        public override WFF Rename(Term s, Term d)
        {
            Term[] rargs = new Term[arity];
            for (int a = 0; a < arity; a++)
            {
                rargs[a] = (arguments[a].Equals(s) ? d : arguments[a].Rename(s, d));
            }

            return new Predicate(name, rargs);
        }

        public override bool Equals(object obj)
        {
            if (obj is Predicate)
            {
                Predicate p = (Predicate)obj;
                if (!name.Equals(p.name)) { return false; }
                if (arity != p.arity) { return false; }

                for (int a = 0; a < arity; a++)
                {
                    if (!arguments[a].Equals(p.arguments[a])) { return false; }
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
