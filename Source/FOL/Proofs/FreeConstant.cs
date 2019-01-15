using TruthTree2.FOL.Logic;

namespace TruthTree2.FOL.Proofs
{
    class FreeConstant : Constant
    {
        private static int number;

        public FreeConstant()
            : base("κ" + number)
        {
            number++;
        }
    }
}
