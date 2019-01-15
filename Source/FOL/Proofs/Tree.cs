using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TruthTree2.FOL.Logic;
using TruthTree2.FOL.ProofExtensions;

namespace TruthTree2.FOL.Proofs
{
    public enum TreeState
    {
        Incomplete,
        Closed,
        Open
    }

    public class Tree : Drawable
    {
        internal List<WFF> sentences;
        internal WFF decomposed;
        internal Tree parent;
        internal Tree[] children;
        internal HashSet<Term> terms;

        private TreeState _state;
        public TreeState State
        {
            get { return _state; }
            private set
            {
                _state = value;
                if (_state != TreeState.Incomplete && parent != null) { parent.checkForCompletion(); }
            }
        }
        public int Height
        {
            get
            {
                if (children == null) { return 1; }
                return 1 + children.Aggregate(0,
                    (max, t) =>
                    {
                        return Math.Max(max, t.Height);
                    });
            }
        }
        public int Width
        {
            get
            {
                if (children == null) { return 1; }
                return children.Aggregate(0,
                    (sum, t) =>
                    {
                        return sum + t.Width;
                    });
            }
        }

        public Tree(List<WFF> s, Tree p = null, WFF d = null, bool rdup = true)
        {
            sentences = new List<WFF>();
            parent = p;
            decomposed = d;
            children = null;

            if (parent == null)
            {
                sentences.AddRange(s);
                terms = new HashSet<Term>();
                foreach (WFF f in sentences)
                {
                    if (f == null) { continue; }

                    terms.UnionWith(f.GetTerms());
                }
            }
            else
            {
                List<WFF> all = getAllSentences();
                foreach (WFF f in s)
                {
                    if (f == null) { continue; }

                    if (rdup)
                    {
                        if (!all.Contains(f))
                        {
                            sentences.Add(f);
                            all.Add(f);
                        }
                    }
                    else { sentences.Add(f); }
                }

                terms = new HashSet<Term>(parent.terms);
            }
        }

        internal List<WFF> getAllSentences()
        {
            List<WFF> all = new List<WFF>(sentences);
            if (parent != null) { all.AddRange(parent.getAllSentences()); }

            return all;
        }

        private List<WFF> getAllUnsatisfied()
        {
            List<WFF> uns = new List<WFF>(sentences.FindAll(f => { return f.Decomposable(); }));
            if (parent != null) { uns.AddRange(parent.getAllUnsatisfied()); }

            if (!(decomposed is Universal) && !(decomposed is Identity))
            {
                uns.Remove(decomposed);
            }

            return uns;
        }

        private HashSet<WFF> getUsedSentences()
        {
            HashSet<WFF> set = new HashSet<WFF>();
            if (children == null)
            {
                List<WFF> all = getAllSentences();
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
                                set.Add(all[a]);
                                break;
                            }
                        }
                    }
                    else if (all[a] is Contradiction)
                    {
                        set.Add(all[a]);
                        break;
                    }

                    bool found = false;
                    for (int b = a + 1; b < all.Count; b++)
                    {
                        if (all[a].Equals(all[b].GetNegation()) || all[b].Equals(all[a].GetNegation()))
                        {
                            set.Add(all[a]);
                            set.Add(all[b]);
                            found = true;
                            break;
                        }
                    }

                    if(found) { break; }
                }

                //if (set.Count == 0) { throw new Exception("How did this even happen?"); }
                return set;
            }

            set.Add(decomposed);
            foreach (Tree c in children) { set.UnionWith(c.getUsedSentences()); }

            return set;
        }

        private void checkForCompletion()
        {
            if (State != TreeState.Incomplete) { return; }

            if (children != null)
            {
                if (children.Any(t => { return t.State == TreeState.Open; }))
                {
                    State = TreeState.Open;
                }
                else if (children.All(t => { return t.State == TreeState.Closed; }))
                {
                    State = TreeState.Closed;
                }
                else
                {
                    State = TreeState.Incomplete;
                }

                return;
            }

            List<WFF> all = getAllSentences();
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
                            State = TreeState.Closed;
                            return;
                        }
                    }
                }
                else if (all[a] is Contradiction)
                {
                    State = TreeState.Closed;
                    return;
                }

                for (int b = a + 1; b < all.Count; b++)
                {
                    if (all[a].Equals(all[b].GetNegation()) || all[b].Equals(all[a].GetNegation()))
                    {
                        State = TreeState.Closed;
                        return;
                    }
                }
            }

            List<WFF> uns = getAllUnsatisfied();
            State = (uns.Count == 0 ? TreeState.Open : TreeState.Incomplete);
        }

        public bool Decompose()
        {
            checkForCompletion();
            if (State != TreeState.Incomplete) { return false; }

            List<WFF> unsatisfied = getAllUnsatisfied();
            unsatisfied.Sort((a, b) => { return a.SortOrder().CompareTo(b.SortOrder()); });
            if (unsatisfied.Count < 1) { throw new Exception("How did this even happen?"); }

            for (int a = 0; a < unsatisfied.Count; a++)
            {
                Decomposition dec = DecomposeWFF(unsatisfied[a]);
                if (dec == null) { continue; }

                children = dec.getTrees(this);
                if (children == null) { continue; }

                decomposed = dec.decomposed;
                return true;
            }
            
            return false;
        }

        public void Satisfy(int maxheight = int.MaxValue)
        {
            // Breadth first decomposition
            // This potentially avoids an infinite branch if there is also an open branch
            Queue<Tree> work = new Queue<Tree>();
            work.Enqueue(this);
            do
            {
                Tree cur = work.Dequeue();
                if (cur.Decompose())
                {
                    foreach (Tree c in cur.children) { work.Enqueue(c); }
                }
                else if (cur.State == TreeState.Incomplete)
                {
                    // This is a fix for the fact that a node is only marked open if it has no sentences left to decompose
                    // but it may be the case that a universal or identity can still be decomposed but doing so results in
                    // no new sentences
                    cur.State = TreeState.Open;
                    if (cur.parent != null) { cur.parent.checkForCompletion(); }
                }
            } while (work.Count > 0 && State == TreeState.Incomplete && Height < maxheight);
        }

        public void Clean()
        {
            if (State != TreeState.Closed) { return; }

            if (children != null)
            {
                foreach (Tree c in children) { c.Clean(); }
            }

            HashSet<WFF> used = getUsedSentences();
            List<WFF> tmp = new List<WFF>();
            foreach (WFF f in sentences)
            {
                if (used.Contains(f)) { tmp.Add(f); }
            }
            sentences = tmp;
        }

        #region DecomposeWFF
        private Decomposition DecomposeWFF(WFF f)
        {
            if (f is AtomicWFF) { return DecomposeWFF((AtomicWFF)f); }
            else if (f is ComplexWFF) { return DecomposeWFF((ComplexWFF)f); }

            throw new NotImplementedException(f.GetType().ToString());
        }

        private Decomposition DecomposeWFF(AtomicWFF f)
        {
            if (f is Predicate) { return null; }
            else if (f is Identity) { return DecomposeWFF((Identity)f); }
            else if (f is Contradiction) { return null; }

            throw new NotImplementedException(f.GetType().ToString());
        }

        private Decomposition DecomposeWFF(Identity f)
        {
            List<WFF> all = getAllSentences();
            List<WFF> dec = new List<WFF>();
            foreach (WFF a in all)
            {
                dec.AddRange(IdentityDecompose(f, a));
            }

            return new Decomposition(f, dec);
        }

        private Decomposition DecomposeWFF(ComplexWFF f)
        {
            if (f is Connective) { return DecomposeWFF((Connective)f); }
            else if (f is Quantifier) { return DecomposeWFF((Quantifier)f); }

            throw new NotImplementedException(f.GetType().ToString());
        }

        private Decomposition DecomposeWFF(Connective f)
        {
            if (f is Biconditional) { return DecomposeWFF((Biconditional)f); }
            else if (f is Conditional) { return DecomposeWFF((Conditional)f); }
            else if (f is Conjunction) { return DecomposeWFF((Conjunction)f); }
            else if (f is Disjunction) { return DecomposeWFF((Disjunction)f); }
            else if (f is Negation) { return DecomposeWFF((Negation)f); }

            throw new NotImplementedException(f.GetType().ToString());
        }

        private Decomposition DecomposeWFF(Biconditional f)
        {
            List<WFF> left = new List<WFF>();
            List<WFF> right = new List<WFF>();
            left.Add(f.left);
            left.Add(f.right);
            right.Add(f.left.GetNegation());
            right.Add(f.right.GetNegation());
            return new Decomposition(f, left, right);
        }

        private Decomposition DecomposeWFF(Conditional f)
        {
            List<WFF> left = new List<WFF>();
            List<WFF> right = new List<WFF>();
            left.Add(f.antecedent.GetNegation());
            right.Add(f.consequent);
            return new Decomposition(f, left, right);
        }

        private Decomposition DecomposeWFF(Conjunction f)
        {
            List<WFF> conjuncts = new List<WFF>(f.conjuncts);
            return new Decomposition(f, conjuncts);
        }

        private Decomposition DecomposeWFF(Disjunction f)
        {
            List<WFF>[] disjuncts = new List<WFF>[f.arity];
            for (int a = 0; a < f.arity; a++)
            {
                disjuncts[a] = new List<WFF>();
                disjuncts[a].Add(f.disjuncts[a]);
            }

            return new Decomposition(f, disjuncts.ToArray());
        }

        private Decomposition DecomposeWFF(Negation f)
        {
            if (f.inner is AtomicWFF) { return null; }
            else if (f.inner is Biconditional)
            {
                Biconditional inner = (Biconditional)f.inner;
                List<WFF> left = new List<WFF>();
                List<WFF> right = new List<WFF>();
                left.Add(inner.left.GetNegation());
                left.Add(inner.right);
                right.Add(inner.left);
                right.Add(inner.right.GetNegation());
                return new Decomposition(f, left, right);
            }
            else if (f.inner is Conditional)
            {
                Conditional inner = (Conditional)f.inner;
                List<WFF> left = new List<WFF>();
                left.Add(inner.antecedent);
                left.Add(inner.consequent.GetNegation());
                return new Decomposition(f, left);
            }
            else if (f.inner is Conjunction)
            {
                Conjunction inner = (Conjunction)f.inner;
                List<WFF>[] conjuncts = new List<WFF>[inner.arity];
                for (int a = 0; a < inner.arity; a++)
                {
                    conjuncts[a] = new List<WFF>();
                    conjuncts[a].Add(inner.conjuncts[a].GetNegation());
                }

                return new Decomposition(f, conjuncts.ToArray());
            }
            else if (f.inner is Disjunction)
            {
                Disjunction inner = (Disjunction)f.inner;
                List<WFF> disjuncts = new List<WFF>();
                foreach (WFF d in inner.disjuncts) { disjuncts.Add(d.GetNegation()); }
                return new Decomposition(f, disjuncts);
            }
            else if (f.inner is Existential)
            {
                Existential inner = (Existential)f.inner;
                List<WFF> dec = new List<WFF>();
                dec.Add(new Universal(inner.variable, inner.scope.GetNegation()));
                return new Decomposition(f, dec);
            }
            else if (f.inner is Negation)
            {
                Negation inner = (Negation)f.inner;
                List<WFF> dec = new List<WFF>();
                dec.Add(inner.inner);
                return new Decomposition(f, dec);
            }
            else if (f.inner is Universal)
            {
                Universal inner = (Universal)f.inner;
                List<WFF> dec = new List<WFF>();
                dec.Add(new Existential(inner.variable, inner.scope.GetNegation()));
                return new Decomposition(f, dec);
            }

            throw new NotImplementedException(f.inner.GetType().ToString());
        }

        private Decomposition DecomposeWFF(Quantifier f)
        {
            if (f is Existential) { return DecomposeWFF((Existential)f); }
            else if (f is Universal) { return DecomposeWFF((Universal)f); }

            throw new NotImplementedException(f.GetType().ToString());
        }

        private Decomposition DecomposeWFF(Existential f)
        {
            List<WFF> dec = new List<WFF>();
            FreeConstant fc = new FreeConstant();
            terms.Add(fc);
            dec.Add(f.scope.Rename(f.variable, fc));
            return new Decomposition(f, dec);
        }

        private Decomposition DecomposeWFF(Universal f)
        {
            List<WFF> dec = new List<WFF>();
            if (terms.Count == 0) { terms.Add(new FreeConstant()); }
            foreach (Term t in terms) { dec.Add(f.scope.Rename(f.variable, t)); }

            return new Decomposition(f, dec);
        }
        #endregion

        #region IdentityDecompose
        private List<WFF> IdentityDecompose(Identity i, WFF f)
        {
            if (f is AtomicWFF) { return IdentityDecompose(i, (AtomicWFF)f); }
            else if (f is ComplexWFF) { return IdentityDecompose(i, (ComplexWFF)f); }

            throw new NotImplementedException(f.GetType().ToString());
        }

        private List<WFF> IdentityDecompose(Identity i, AtomicWFF f)
        {
            if (f is Predicate) { return IdentityDecompose(i, (Predicate)f); }
            else if (f is Identity) { return IdentityDecompose(i, (Identity)f); }

            throw new NotImplementedException(f.GetType().ToString());
        }

        private List<WFF> IdentityDecompose(Identity i, Predicate f)
        {
            List<Term>[] stuff = new List<Term>[f.arity];
            for (int a = 0; a < f.arity; a++) { stuff[a] = IdentityDecomposeTerm(i, f.arguments[a]); }

            List<WFF> result = new List<WFF>();
            List<List<Term>> cp = crossproduct(stuff);
            foreach (List<Term> arg in cp)
            {
                result.Add(new Predicate(f.name, arg.ToArray()));
            }

            return result;
        }

        private List<Term> IdentityDecomposeTerm(Identity i, Term t)
        {
            if (t is BasicTerm) { return IdentityDecomposeTerm(i, (BasicTerm)t); }
            else if (t is ComplexTerm) { return IdentityDecomposeTerm(i, (ComplexTerm)t); }

            throw new NotImplementedException(t.GetType().ToString());
        }

        private List<Term> IdentityDecomposeTerm(Identity i, BasicTerm t)
        {
            List<Term> tmp = new List<Term>();
            tmp.Add(t);
            if (t.Equals(i.left)) { tmp.Add(i.right); }
            if (t.Equals(i.right)) { tmp.Add(i.left); }
            List<Term> result = new List<Term>();
            foreach (Term d in tmp.Distinct()) { result.Add(d); }
            return result;
        }

        private List<Term> IdentityDecomposeTerm(Identity i, ComplexTerm t)
        {
            if (t is Function) { return IdentityDecomposeTerm(i, (Function)t); }

            throw new NotImplementedException(t.GetType().ToString());
        }

        private List<Term> IdentityDecomposeTerm(Identity i, Function t)
        {
            List<Term>[] stuff = new List<Term>[t.arity];
            for (int a = 0; a < t.arity; a++) { stuff[a] = IdentityDecomposeTerm(i, t.arguments[a]); }

            List<Term> result = new List<Term>();
            if (i.left.Equals(t)) { result.Add(i.right); }
            if (i.right.Equals(t)) { result.Add(i.left); }
            List<List<Term>> cp = crossproduct(stuff);
            foreach (List<Term> arg in cp)
            {
                result.Add(new Function(t.name, arg.ToArray()));
            }

            return result;
        }

        private List<List<T>> crossproduct<T>(List<T>[] lists, int depth = 0)
        {
            if (depth >= lists.Length) { return new List<List<T>>(); }

            if (depth == lists.Length - 1)
            {
                List<List<T>> res = new List<List<T>>();
                foreach (T a in lists[depth])
                {
                    List<T> tmp = new List<T>();
                    tmp.Add(a);
                    res.Add(tmp);
                }

                return res;
            }

            List<List<T>> sub = crossproduct(lists, depth + 1);
            List<List<T>> result = new List<List<T>>();
            foreach (T a in lists[depth])
            {
                foreach (List<T> b in sub)
                {
                    List<T> tmp = new List<T>();
                    tmp.Add(a);
                    tmp.AddRange(b);
                    result.Add(tmp);
                }
            }

            return result;
        }

        private List<WFF> IdentityDecompose(Identity i, Identity f)
        {
            List<Term> left = IdentityDecomposeTerm(i, f.left);
            List<Term> right = IdentityDecomposeTerm(i, f.right);
            List<WFF> result = new List<WFF>();
            foreach (Term a in left)
            {
                foreach (Term b in right)
                {
                    result.Add(new Identity(a, b));
                }
            }

            return result;
        }

        private List<WFF> IdentityDecompose(Identity i, ComplexWFF f)
        {
            if (f is Connective) { return IdentityDecompose(i, (Connective)f); }
            else if (f is Quantifier) { return IdentityDecompose(i, (Quantifier)f); }

            throw new NotImplementedException(f.GetType().ToString());
        }

        private List<WFF> IdentityDecompose(Identity i, Connective f)
        {
            if (f is Biconditional) { return IdentityDecompose(i, (Biconditional)f); }
            else if (f is Conditional) { return IdentityDecompose(i, (Conditional)f); }
            else if (f is Conjunction) { return IdentityDecompose(i, (Conjunction)f); }
            else if (f is Disjunction) { return IdentityDecompose(i, (Disjunction)f); }
            else if (f is Negation) { return IdentityDecompose(i, (Negation)f); }

            throw new NotImplementedException(f.GetType().ToString());
        }

        private List<WFF> IdentityDecompose(Identity i, Biconditional f)
        {
            List<WFF> left = IdentityDecompose(i, f.left);
            List<WFF> right = IdentityDecompose(i, f.right);
            List<WFF> result = new List<WFF>();
            foreach (WFF a in left)
            {
                foreach (WFF b in right)
                {
                    result.Add(new Biconditional(a, b));
                }
            }

            return result;
        }

        private List<WFF> IdentityDecompose(Identity i, Conditional f)
        {
            List<WFF> ant = IdentityDecompose(i, f.antecedent);
            List<WFF> con = IdentityDecompose(i, f.consequent);
            List<WFF> result = new List<WFF>();
            foreach (WFF a in ant)
            {
                foreach (WFF b in con)
                {
                    result.Add(new Conditional(a, b));
                }
            }

            return result;
        }
        
        private List<WFF> IdentityDecompose(Identity i, Conjunction f)
        {
            List<WFF>[] stuff = new List<WFF>[f.arity];
            for (int a = 0; a < f.arity; a++) { stuff[a] = IdentityDecompose(i, f.conjuncts[a]); }

            List<WFF> result = new List<WFF>();
            List<List<WFF>> cp = crossproduct(stuff);
            foreach (List<WFF> con in cp)
            {
                result.Add(new Conjunction(con.ToArray()));
            }

            return result;
        }

        private List<WFF> IdentityDecompose(Identity i, Disjunction f)
        {
            List<WFF>[] stuff = new List<WFF>[f.arity];
            for (int a = 0; a < f.arity; a++) { stuff[a] = IdentityDecompose(i, f.disjuncts[a]); }

            List<WFF> result = new List<WFF>();
            List<List<WFF>> cp = crossproduct(stuff);
            foreach (List<WFF> con in cp)
            {
                result.Add(new Conjunction(con.ToArray()));
            }

            return result;
        }

        private List<WFF> IdentityDecompose(Identity i, Negation f)
        {
            List<WFF> stuff = IdentityDecompose(i, f.inner);
            List<WFF> result = new List<WFF>();
            foreach (WFF a in stuff) { result.Add(new Negation(a)); }

            return result;
        }

        private List<WFF> IdentityDecompose(Identity i, Quantifier f)
        {
            if (i.left.Equals(f.variable) || i.right.Equals(f.variable)) { return new List<WFF>(); }

            if (f is Existential) { return IdentityDecompose(i, (Existential)f); }
            else if (f is Universal) { return IdentityDecompose(i, (Universal)f); }

            throw new NotImplementedException(f.GetType().ToString());
        }

        private List<WFF> IdentityDecompose(Identity i, Existential f)
        {
            List<WFF> stuff = IdentityDecompose(i, f.scope);
            List<WFF> result = new List<WFF>();
            foreach (WFF a in stuff) { result.Add(new Existential(f.variable, a)); }

            return result;
        }

        private List<WFF> IdentityDecompose(Identity i, Universal f)
        {
            List<WFF> stuff = IdentityDecompose(i, f.scope);
            List<WFF> result = new List<WFF>();
            foreach (WFF a in stuff) { result.Add(new Universal(f.variable, a)); }

            return result;
        }
        #endregion

        #region Drawing
        private static int fontsize = 16;
        private static Font font = new Font("Cambria", fontsize);
        private static Font fontbold = new Font("Cambria", fontsize, FontStyle.Bold);
        private static Brush bBlack = new SolidBrush(Color.Black);
        private static Brush bGreen = new SolidBrush(Color.Green);
        private static Brush bRed = new SolidBrush(Color.Red);
        private static Pen pBlack = new Pen(Color.Black);
        private static int gapsize = 15;

        private bool _sized = false;
        private SizeF _mysize;
        private SizeF _chsize;
        private SizeF _ovsize;

        private void updateSizes(Graphics g)
        {
            if (g == null) { return; }

            _mysize = sentences.Aggregate(new SizeF(),
                (box, f) =>
                {
                    SizeF measured = g.MeasureString(f.ToString(), font);
                    return new SizeF(
                        Math.Max(box.Width, measured.Width),
                        box.Height + measured.Height);
                });

            if (children == null)
            {
                _mysize.Width = Math.Max(_mysize.Width, g.MeasureString("?", fontbold).Width);
                _mysize.Height += g.MeasureString("?", fontbold).Height;
                _chsize = new SizeF();
            }
            else
            {
                _chsize = new SizeF();
                foreach (Tree c in children)
                {
                    SizeF cb = c.ImageSize(g);
                    _chsize.Width += cb.Width;
                    _chsize.Height = Math.Max(_chsize.Height, cb.Height);
                }
                _chsize.Width += (children.Length - 1) * gapsize;
                _chsize.Height += (children == null ? 0 : gapsize);
            }

            _ovsize = new SizeF(
                Math.Max(_mysize.Width, _chsize.Width) + 2,
                _mysize.Height + _chsize.Height + 2);

            _sized = true;
        }

        public SizeF ImageSize(Graphics g = null)
        {
            if (!_sized) { updateSizes(g); }
            return _ovsize;
        }

        internal SizeF MyAreaSize(Graphics g = null)
        {
            if (!_sized) { updateSizes(g); }
            return _mysize;
        }

        internal SizeF ChildAreaSize(Graphics g = null)
        {
            if (!_sized) { updateSizes(g); }
            return _chsize;
        }

        private void drawCenteredXString(string s, Graphics i, Font f, Brush b, float arealeft, float arearight, float y)
        {
            SizeF measured = i.MeasureString(s, font);
            i.DrawString(
                s,
                f,
                b,
                arealeft + ((arearight - arealeft - measured.Width) / 2),
                y);
        }

        public void Draw(Graphics image)
        {
            float myleft = (ImageSize().Width - MyAreaSize().Width) / 2;
            image.DrawRectangle(pBlack, myleft, 1, MyAreaSize().Width, MyAreaSize().Height);
            float stop = 1;
            for (int a = 0; a < sentences.Count; a++)
            {
                drawCenteredXString(
                    sentences[a].ToString(),
                    image,
                    font,
                    bBlack,
                    myleft + 1,
                    myleft + MyAreaSize().Width,
                    stop);
                stop += image.MeasureString(sentences[a].ToString(), font).Height;
            }

            if (children == null)
            {
                string str;
                Brush col;
                switch (State)
                {
                    case TreeState.Closed: str = "X"; col = bGreen; break;
                    case TreeState.Open: str = "O"; col = bGreen; break;
                    default: str = "?"; col = bRed; break;
                }

                drawCenteredXString(
                    str,
                    image,
                    fontbold,
                    col,
                    myleft + 1,
                    myleft + MyAreaSize().Width,
                    stop);
                return;
            }

            PointF mybotcenter = new PointF(
                myleft + (MyAreaSize().Width / 2),
                MyAreaSize().Height + 1);
            float left = Math.Max(1, ImageSize().Width - ChildAreaSize().Width) / 2;
            foreach (Tree c in children)
            {
                PointF chtopcenter = new PointF(
                    left + (c.ImageSize().Width / 2),
                    mybotcenter.Y + gapsize);
                image.DrawLine(pBlack, mybotcenter, chtopcenter);

                var pstate = image.Save();
                image.TranslateTransform(left, MyAreaSize().Height + gapsize);
                c.Draw(image);
                image.Restore(pstate);
                left += gapsize + c.ImageSize().Width;
            }
        }

        public void SaveImage(string file)
        {
            Bitmap bmptree = new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            Graphics gtree = Graphics.FromImage(bmptree);
            SizeF tsize = ImageSize(gtree);
            bmptree = new Bitmap((int)Math.Ceiling(tsize.Width), (int)Math.Ceiling(tsize.Height), System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            gtree = Graphics.FromImage(bmptree);
            gtree.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            gtree.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            gtree.Clear(Color.White);
            Draw(gtree);
            bmptree.Save(file);
        }

        public bool AntiAliasText { get { return true; } }
        public bool AntiAliasDrawing { get { return true; } }
        #endregion
    }
}
