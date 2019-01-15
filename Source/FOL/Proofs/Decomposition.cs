using System.Collections.Generic;
using TruthTree2.FOL.Logic;

namespace TruthTree2.FOL.Proofs
{
    internal class Decomposition
    {
        internal WFF decomposed;
        internal List<WFF>[] decomposition;

        internal Decomposition(WFF d, params List<WFF>[] c)
        {
            decomposed = d;
            decomposition = c;
        }

        internal Tree[] getTrees(Tree parent)
        {
            List<Tree> trees = new List<Tree>();
            for (int a = 0; a < decomposition.Length; a++)
            {
                Tree t = new Tree(decomposition[a], parent, null, decomposition.Length == 1);
                if (t.sentences.Count > 0) { trees.Add(t); }
            }

            return trees.Count > 0 ? trees.ToArray() : null;
        }
    }
}
