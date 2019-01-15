using System.Collections.Generic;
using System.Linq;

namespace TruthTree2.FOL.Logic
{
    public class Conjunction : Connective
    {
        internal WFF[] conjuncts;
        internal int arity { get { return conjuncts.Length; } }

        public Conjunction(params WFF[] c)
        {
            List<WFF> con = new List<WFF>();
            foreach (WFF a in c)
            {
                if (a is Conjunction)
                {
                    Conjunction b = (Conjunction)a;
                    con.AddRange(b.conjuncts);
                }
                else { con.Add(a); }
            }

            conjuncts = con.ToArray();
        }

        public override HashSet<Term> GetTerms()
        {
            HashSet<Term> terms = new HashSet<Term>();
            foreach (WFF c in conjuncts)
            {
                terms.UnionWith(c.GetTerms());
            }

            return terms;
        }

        public override HashSet<Constant> GetConstants()
        {
            HashSet<Constant> cons = new HashSet<Constant>();
            foreach (WFF c in conjuncts)
            {
                cons.UnionWith(c.GetConstants());
            }

            return cons;
        }

        public override HashSet<Variable> GetFreeVariables()
        {
            HashSet<Variable> vars = new HashSet<Variable>();
            foreach (WFF c in conjuncts)
            {
                vars.UnionWith(c.GetFreeVariables());
            }

            return vars;
        }

        public override WFF Rename(Term s, Term d)
        {
            WFF[] rcon = new WFF[conjuncts.Length];
            for (int a = 0; a < conjuncts.Length; a++)
            {
                rcon[a] = conjuncts[a].Rename(s, d);
            }

            return new Conjunction(rcon);
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
            if (obj is Conjunction)
            {
                Conjunction c = (Conjunction)obj;
                return setEquals(conjuncts, c.conjuncts);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("({0})", string.Join<WFF>(" ⋀ ", conjuncts));
        }

        public override string ToInputString()
        {
            return string.Format("(and {0})", string.Join<WFF>(" ", conjuncts));
        }
    }
}
