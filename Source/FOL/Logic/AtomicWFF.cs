using System.Collections.Generic;

namespace TruthTree2.FOL.Logic
{
    public abstract class AtomicWFF : WFF
    {
        public abstract List<Term> GetTermsInOrder();
        public abstract bool ContainsTerm(Term t);
    }
}
