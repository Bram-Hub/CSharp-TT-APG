using System;
using System.Collections.Generic;

using TruthTree2.FOL.Logic;
using TruthTree2.FOL.Proofs;

namespace TruthTree2.FOL.ProofExtensions
{
    public static class ProofExtensions
    {
        #region SortOrder
        public static int SortOrder(this WFF f)
        {
            if (f is AtomicWFF) { return SortOrder((AtomicWFF)f); }
            else if (f is ComplexWFF) { return SortOrder((ComplexWFF)f); }

            throw new NotImplementedException(f.GetType().ToString());
        }

        public static int SortOrder(this AtomicWFF f)
        {
            if (f is Predicate) { return 0; }
            else if (f is Identity) { return int.MaxValue - 1; }
            else if (f is Contradiction) { return 0; }

            throw new NotImplementedException(f.GetType().ToString());
        }

        public static int SortOrder(this ComplexWFF f)
        {
            if (f is Connective) { return SortOrder((Connective)f); }
            else if (f is Quantifier) { return SortOrder((Quantifier)f); }

            throw new NotImplementedException(f.GetType().ToString());
        }

        public static int SortOrder(this Connective f)
        {
            if (f is Biconditional) { return 2; }
            else if (f is Conditional) { return 2; }
            else if (f is Conjunction) { return 1; }
            else if (f is Disjunction) { return ((Disjunction)f).arity; }
            else if (f is Negation) { return SortOrder((Negation)f); }
            throw new NotImplementedException(f.GetType().ToString());
        }

        public static int SortOrder(this Negation f)
        {
            WFF i = f.inner;
            if (i is AtomicWFF) { return 0; }
            else if (i is Biconditional) { return 2; }
            else if (i is Conditional) { return 1; }
            else if (i is Conjunction) { return ((Conjunction)i).arity; }
            else if (i is Disjunction) { return 1; }
            else if (i is Negation) { return 1; }
            else if (i is Quantifier) { return 1; }

            throw new NotImplementedException(f.GetType().ToString());
        }

        public static int SortOrder(this Quantifier f)
        {
            if (f is Existential) { return 1; }
            else if (f is Universal) { return int.MaxValue; }

            throw new NotImplementedException(f.GetType().ToString());
        }
        #endregion

        #region Decomposable
        public static bool Decomposable(this WFF f)
        {
            if (f is AtomicWFF) { return Decomposable((AtomicWFF)f); }
            else if (f is ComplexWFF) { return Decomposable((ComplexWFF)f); }

            throw new NotImplementedException(f.GetType().ToString());
        }

        public static bool Decomposable(this AtomicWFF f)
        {
            if (f is Predicate) { return false; }
            else if (f is Identity)
            {
                Identity i = (Identity)f;
                return !i.left.Equals(i.right);
            }
            else if (f is Contradiction) { return false; }

            throw new NotImplementedException(f.GetType().ToString());
        }

        public static bool Decomposable(this ComplexWFF f)
        {
            if (f is Connective) { return Decomposable((Connective)f); }
            else if (f is Quantifier) { return Decomposable((Quantifier)f); }

            throw new NotImplementedException(f.GetType().ToString());
        }

        public static bool Decomposable(this Connective f)
        {
            if (f is Biconditional) { return true; }
            else if (f is Conditional) { return true; }
            else if (f is Conjunction) { return true; }
            else if (f is Disjunction) { return true; }
            else if (f is Negation) { return Decomposable((Negation)f); }
            throw new NotImplementedException(f.GetType().ToString());
        }

        public static bool Decomposable(this Negation f)
        {
            WFF i = f.inner;
            if (i is AtomicWFF) { return false; }
            else if (i is Biconditional) { return true; }
            else if (i is Conditional) { return true; }
            else if (i is Conjunction) { return true; }
            else if (i is Disjunction) { return true; }
            else if (i is Negation) { return true; }
            else if (i is Quantifier) { return true; }

            throw new NotImplementedException(f.GetType().ToString());
        }

        public static bool Decomposable(this Quantifier f)
        {
            if (f is Existential) { return true; }
            else if (f is Universal) { return true; }

            throw new NotImplementedException(f.GetType().ToString());
        }
        #endregion

        #region TreeToProof
        internal static void AddTree(this Subproof p, Tree t)
        {
            if (t == null) { throw new ArgumentNullException(); }
            if (t.State != TreeState.Closed) { throw new InvalidOperationException(); }

            if (t.decomposed == null)
            {
                // Something got a contradiction here, find out what
                List<WFF> all = t.getAllSentences();
                WFF c1 = null;
                WFF c2 = null;
                for (int a = 0; a < all.Count; a++)
                {
                    if (all[a] is Negation)
                    {
                        Negation n = (Negation)all[a];
                        if (n.inner is Identity)
                        {
                            Identity i = (Identity)n.inner;
                            if (i.left.Equals(i.right))
                            {
                                c1 = all[a];
                                break;
                            }
                        }
                    }
                    else if (all[a] is Contradiction)
                    {
                        c1 = all[a];
                        break;
                    }

                    bool found = false;
                    for (int b = a + 1; b < all.Count; b++)
                    {
                        if (all[a].Equals(all[b].GetNegation()) || all[b].Equals(all[a].GetNegation()))
                        {
                            c1 = all[a];
                            c2 = all[b];
                            found = true;
                            break;
                        }
                    }

                    if (found) { break; }
                }

                Proof p1 = p.Find(c1);
                if (p1 == null) { throw new InvalidOperationException("p1"); }

                if (c2 == null)
                {
                    p.steps.Add(new ProofLine(new Contradiction(), Rule.ContradictionIntro, p1));
                }
                else
                {
                    Proof p2 = p.Find(c2);
                    if (p2 == null) { throw new InvalidOperationException("p2"); }

                    p.steps.Add(new ProofLine(new Contradiction(), Rule.ContradictionIntro, p1, p2));
                }
                
                return;
            }

            Proof d = p.Find(t.decomposed);
            if (t.decomposed is Biconditional)
            {
                // Add the steps for Equiv
                Biconditional b = (Biconditional)t.decomposed;
                Disjunction equ = new Disjunction(
                    new Conjunction(b.left, b.right),
                    new Conjunction(b.left.GetNegation(), b.right.GetNegation()));
                ProofLine equp = new ProofLine(equ, Rule.Equivalence, d);
                p.steps.Add(equp);
                
                ProofLine plleft = new ProofLine(equ.disjuncts[0], Rule.Assumption);
                Subproof left = new Subproof();
                left.parent = p;
                left.assumptions.Add(plleft);
                left.steps.Add(new ProofLine(b.left, Rule.AndElim, plleft));
                left.steps.Add(new ProofLine(b.right, Rule.AndElim, plleft));
                AddTree(left, t.children[0]);
                p.steps.Add(left);

                ProofLine plright = new ProofLine(equ.disjuncts[1], Rule.Assumption);
                Subproof right = new Subproof();
                right.parent = p;
                right.assumptions.Add(plright);
                right.steps.Add(new ProofLine(b.left.GetNegation(), Rule.AndElim, plright));
                right.steps.Add(new ProofLine(b.right.GetNegation(), Rule.AndElim, plright));
                AddTree(right, t.children[1]);
                p.steps.Add(right);

                p.steps.Add(new ProofLine(new Contradiction(), Rule.OrElim, equp, left, right));
            }
            else if (t.decomposed is Conditional)
            {
                // Add the steps for Impl
                Conditional c = (Conditional)t.decomposed;
                Disjunction imp = new Disjunction(c.antecedent.GetNegation(), c.consequent);
                ProofLine impp = new ProofLine(imp, Rule.Implication, d);
                p.steps.Add(impp);

                Proof[] steps = new Proof[t.children.Length + 1];
                steps[0] = impp;
                for (int a = 0; a < t.children.Length; a++)
                {
                    steps[a + 1] = new Subproof();
                    Subproof s = (Subproof)steps[a + 1];
                    s.parent = p;
                    s.assumptions.Add(new ProofLine(t.children[a].sentences[0], Rule.Assumption));
                    AddTree(s, t.children[a]);
                    p.steps.Add(s);
                }
                p.steps.Add(new ProofLine(new Contradiction(), Rule.OrElim, steps));
            }
            else if (t.decomposed is Conjunction)
            {
                foreach (WFF f in t.children[0].sentences)
                {
                    p.steps.Add(new ProofLine(f, Rule.AndElim, d));
                }
                AddTree(p, t.children[0]);
            }
            else if (t.decomposed is Disjunction)
            {
                Proof[] steps = new Proof[t.children.Length + 1];
                steps[0] = d;
                for (int a = 0; a < t.children.Length; a++)
                {
                    steps[a + 1] = new Subproof();
                    Subproof s = (Subproof)steps[a + 1];
                    s.parent = p;
                    s.assumptions.Add(new ProofLine(t.children[a].sentences[0], Rule.Assumption));
                    AddTree(s, t.children[a]);
                    p.steps.Add(s);
                }
                p.steps.Add(new ProofLine(new Contradiction(), Rule.OrElim, steps));
            }
            else if (t.decomposed is Existential)
            {
                Subproof sub = new Subproof();
                sub.parent = p;
                sub.assumptions.Add(new ProofLine(t.children[0].sentences[0], Rule.Assumption));
                AddTree(sub, t.children[0]);
                p.steps.Add(sub);
                p.steps.Add(new ProofLine(new Contradiction(), Rule.ExistentialElim, d, sub));
            }
            else if (t.decomposed is Identity)
            {
                // Need to add some way to find the other source
                foreach (WFF f in t.children[0].sentences)
                {
                    p.steps.Add(new ProofLine(f, Rule.EqualsElim, d));
                }
                AddTree(p, t.children[0]);
            }
            else if (t.decomposed is Negation)
            {
                Negation n = (Negation)t.decomposed;
                if (n.inner is Biconditional)
                {
                    // I really hope there is a better way for this one
                    // Add the steps for DeM and Equiv
                    Biconditional b = (Biconditional)n.inner;
                    ProofLine equ = new ProofLine(new Negation(new Conjunction(new Disjunction(b.left.GetNegation(), b.right), new Disjunction(b.left, b.right.GetNegation()))), Rule.Equivalence, d);
                    p.steps.Add(equ);

                    ProofLine dem = new ProofLine(new Disjunction(new Negation(new Disjunction(b.left.GetNegation(), b.right)), new Negation(new Disjunction(b.left, b.right.GetNegation()))), Rule.DeMorgans, equ);
                    p.steps.Add(dem);

                    Subproof left = new Subproof();
                    left.parent = p;
                    ProofLine plleft = new ProofLine(new Negation(new Disjunction(b.left.GetNegation(), b.right)), Rule.Assumption);
                    left.assumptions.Add(plleft);
                    ProofLine demleft = new ProofLine(new Conjunction(b.left, b.right.GetNegation()), Rule.DeMorgans, plleft);
                    left.steps.Add(demleft);
                    left.steps.Add(new ProofLine(b.left, Rule.AndElim, demleft));
                    left.steps.Add(new ProofLine(b.right.GetNegation(), Rule.AndElim, demleft));
                    AddTree(left, t.children[1]);
                    p.steps.Add(left);

                    Subproof right = new Subproof();
                    right.parent = p;
                    ProofLine plright = new ProofLine(new Negation(new Disjunction(b.left, b.right.GetNegation())), Rule.Assumption);
                    right.assumptions.Add(plright);
                    ProofLine demright = new ProofLine(new Conjunction(b.left.GetNegation(), b.right), Rule.DeMorgans, plright);
                    right.steps.Add(demright);
                    right.steps.Add(new ProofLine(b.left.GetNegation(), Rule.AndElim, demright));
                    right.steps.Add(new ProofLine(b.right, Rule.AndElim, demright));
                    AddTree(right, t.children[0]);
                    p.steps.Add(right);

                    p.steps.Add(new ProofLine(new Contradiction(), Rule.OrElim, dem, left, right));
                }
                else if (n.inner is Conditional)
                {
                    // Add the steps for DeM and Impl
                    Conditional c = (Conditional)n.inner;
                    ProofLine imp = new ProofLine(new Negation(new Disjunction(c.antecedent.GetNegation(), c.consequent)), Rule.Implication, d);
                    p.steps.Add(imp);

                    ProofLine dem = new ProofLine(new Conjunction(c.antecedent, c.consequent.GetNegation()), Rule.DeMorgans, imp);
                    p.steps.Add(dem);

                    p.steps.Add(new ProofLine(c.antecedent, Rule.AndElim, dem));
                    p.steps.Add(new ProofLine(c.consequent.GetNegation(), Rule.AndElim, dem));

                    AddTree(p, t.children[0]);
                }
                else if (n.inner is Conjunction)
                {
                    // Add the steps for DeM
                    Conjunction i = (Conjunction)n.inner;
                    List<WFF> neg = new List<WFF>();
                    foreach (WFF f in i.conjuncts) { neg.Add(f.GetNegation()); }
                    Disjunction dem = new Disjunction(neg.ToArray());
                    ProofLine demp = new ProofLine(dem, Rule.DeMorgans, d);
                    p.steps.Add(demp);

                    Proof[] steps = new Proof[t.children.Length + 1];
                    steps[0] = demp;
                    for (int a = 0; a < t.children.Length; a++)
                    {
                        steps[a + 1] = new Subproof();
                        Subproof s = (Subproof)steps[a + 1];
                        s.parent = p;
                        s.assumptions.Add(new ProofLine(t.children[a].sentences[0], Rule.Assumption));
                        AddTree(s, t.children[a]);
                        p.steps.Add(s);
                    }
                    p.steps.Add(new ProofLine(new Contradiction(), Rule.OrElim, steps));
                }
                else if (n.inner is Disjunction)
                {
                    // Add the steps for DeM
                    Disjunction i = (Disjunction)n.inner;
                    List<WFF> neg = new List<WFF>();
                    foreach (WFF f in i.disjuncts) { neg.Add(f.GetNegation()); }
                    Conjunction dem = new Conjunction(neg.ToArray());

                    ProofLine demp = new ProofLine(dem, Rule.DeMorgans, d);
                    p.steps.Add(demp);

                    foreach (WFF f in t.children[0].sentences)
                    {
                        p.steps.Add(new ProofLine(f, Rule.AndElim, demp));
                    }
                    AddTree(p, t.children[0]);
                }
                else if (n.inner is Existential)
                {
                    // Add the steps for DeM
                    p.steps.Add(new ProofLine(t.children[0].sentences[0], Rule.DeMorgans, d));
                    AddTree(p, t.children[0]);
                }
                else if (n.inner is Negation)
                {
                    p.steps.Add(new ProofLine(t.children[0].sentences[0], Rule.NotElim, d));
                    AddTree(p, t.children[0]);
                }
                else if (n.inner is Universal)
                {
                    // Add the steps for DeM
                    p.steps.Add(new ProofLine(t.children[0].sentences[0], Rule.DeMorgans, d));
                    AddTree(p, t.children[0]);
                }
            }
            else if (t.decomposed is Universal)
            {
                foreach (WFF f in t.children[0].sentences)
                {
                    p.steps.Add(new ProofLine(f, Rule.UniversalElim, d));
                }
                AddTree(p, t.children[0]);
            }
        }
        #endregion
    }
}
