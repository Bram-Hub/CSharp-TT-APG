using System.Collections.Generic;

namespace TruthTree2.FOL.Logic
{
    public class Constant : BasicTerm
    {
        public Constant(string n)
        {
            name = n;
        }

        public override HashSet<Constant> GetConstants()
        {
            HashSet<Constant> cons = new HashSet<Constant>();
            cons.Add(this);
            return cons;
        }

        public override HashSet<Variable> GetFreeVariables()
        {
            return new HashSet<Variable>();
        }

        public override string ToString()
        {
            return name;
        }

        public override string ToInputString()
        {
            return name;
        }
    }
}
