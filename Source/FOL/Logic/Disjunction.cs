using System.Collections.Generic;
using System.Linq;

namespace TruthTree2.FOL.Logic
{
    public class Disjunction : Connective
    {
        internal WFF[] disjuncts;
        internal int arity { get { return disjuncts.Length; } }

        public Disjunction(params WFF[] d)
        {
            List<WFF> dis = new List<WFF>();
            foreach (WFF a in d)
            {
                if (a is Disjunction)
                {
                    Disjunction b = (Disjunction)a;
                    dis.AddRange(b.disjuncts);
                }
                else { dis.Add(a); }
            }

            disjuncts = dis.ToArray();
        }

        public override HashSet<Term> GetTerms()
        {
            HashSet<Term> terms = new HashSet<Term>();
            foreach (WFF d in disjuncts)
            {
                terms.UnionWith(d.GetTerms());
            }

            return terms;
        }

        public override HashSet<Constant> GetConstants()
        {
            HashSet<Constant> cons = new HashSet<Constant>();
            foreach (WFF d in disjuncts)
            {
                cons.UnionWith(d.GetConstants());
            }

            return cons;
        }

        public override HashSet<Variable> GetFreeVariables()
        {
            HashSet<Variable> vars = new HashSet<Variable>();
            foreach (WFF d in disjuncts)
            {
                vars.UnionWith(d.GetFreeVariables());
            }

            return vars;
        }

        public override WFF Rename(Term s, Term d)
        {
            WFF[] rdis = new WFF[disjuncts.Length];
            for (int a = 0; a < disjuncts.Length; a++)
            {
                rdis[a] = disjuncts[a].Rename(s, d);
            }

            return new Disjunction(rdis);
        }

        private bool setEquals(WFF[] a, WFF[] b)
        {
            if (a.Length != b.Length) { return false; }
            Dictionary<WFF, int> dict = new Dictionary<WFF, int>();

            foreach (WFF w in a)
            {
                if (dict.ContainsKey(w)) { dict[w]++; }
                else { dict[w] = 1; }
            }

            foreach (WFF w in b)
            {
                if (dict.ContainsKey(w)) { dict[w]--; }
                else { return false; }
            }

            return dict.Values.All(v => { return v == 0; });
        }

        public override bool Equals(object obj)
        {
            if (obj is Disjunction)
            {
                Disjunction d = (Disjunction)obj;
                return setEquals(disjuncts, d.disjuncts);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("({0})", string.Join<WFF>(" ⋁ ", disjuncts));
        }

        public override string ToInputString()
        {
            return string.Format("(or {0})", string.Join<WFF>(" ", disjuncts));
        }
    }
}
