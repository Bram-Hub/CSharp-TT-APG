using System.Collections.Generic;

namespace TruthTree2.FOL.Logic
{
    public class Variable : BasicTerm
    {
        public Variable(string n)
        {
            name = n;
        }

        public override HashSet<Constant> GetConstants()
        {
            return new HashSet<Constant>();
        }

        public override HashSet<Variable> GetFreeVariables()
        {
            HashSet<Variable> vars = new HashSet<Variable>();
            vars.Add(this);
            return vars;
        }

        public override string ToString()
        {
            return "_" + name;
        }

        public override string ToInputString()
        {
            return "_" + name;
        }
    }
}
