using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using TruthTree2.FOL.Input;
using TruthTree2.FOL.Logic;
using TruthTree2.FOL.Proofs;
using TruthTree2.FOL.ProofExtensions;

namespace TruthTree2
{
    public partial class MainForm : Form
    {
        private List<WFF> premises;
        private WFF conclusion;
        private Worker<Tree> worker;
        private Tree tree;
        private Proof proof;

        public MainForm()
        {
            InitializeComponent();

            Icon = TruthTree2.Properties.Resources.Icon;

            WFF inn = new Negation(new Predicate("A"));
            WFF tmp1 = new Negation(inn);
            WFF tmp2 = inn.GetNegation();
            Console.WriteLine("{0}\t{1}", tmp1, tmp2);
        }

        #region Thread callbacks
        private delegate void SetTextCallback(Control c, string s);
        private void setText(Control c, string s)
        {
            if (c.InvokeRequired)
            {
                SetTextCallback cb = new SetTextCallback(setText);
                Invoke(cb, c, s);
            }
            else
            {
                c.Text = s;
            }
        }

        private delegate void SetEnabledCallback(Control c, bool b);
        private void setEnabled(Control c, bool b)
        {
            if (c.InvokeRequired)
            {
                SetEnabledCallback cb = new SetEnabledCallback(setEnabled);
                Invoke(cb, c, b);
            }
            else
            {
                c.Enabled = b;
            }
        }

        private delegate void SetStatusCallback(string s);
        private void setStatus(string s)
        {
            if (InvokeRequired)
            {
                SetStatusCallback cb = new SetStatusCallback(setStatus);
                Invoke(cb, s);
            }
            else
            {
                Text = "Logic! [" + s + "]";
            }
        }

        private delegate void SetTabCallback(int i);
        private void setTab(int i)
        {
            if (tcTabs.InvokeRequired)
            {
                SetTabCallback cb = new SetTabCallback(setTab);
                Invoke(cb, i);
            }
            else
            {
                tcTabs.SelectedIndex = i;
            }
        }
        #endregion

        private void bParse_Click(object sender, EventArgs e)
        {
            premises = new List<WFF>();
            rtbPremisesOut.Text = "";
            foreach (string l in rtbPremisesIn.Text.Trim().Split('\n'))
            {
                if (l.Length < 1) { continue; }

                WFF f = Parser.parseString(l.Trim());
                if (f != null)
                {
                    premises.Add(f);
                    rtbPremisesOut.AppendText(f + "\n");
                }
                else
                {
                    MessageBox.Show(string.Format("Error parsing premise {{{0}}}", l.Trim()));
                    rtbPremisesOut.Text = "";
                    return;
                }
            }

            conclusion = Parser.parseString(rtbConclusionIn.Text);
            if (conclusion == null)
            {
                MessageBox.Show(string.Format("Error parsing conclusion {{{0}}}", rtbConclusionIn.Text));
                rtbPremisesOut.Text = "";
                return;
            }
            rtbConclusionOut.Text = String.Format("{0}", conclusion);

            List<WFF> sentences = new List<WFF>(premises);
            sentences.Add(conclusion.GetNegation());

            worker = new Worker<Tree>(
                () =>
                {
                    Tree t = new Tree(sentences);
                    t.Satisfy();

                    return t;
                });
            worker.WorkFinished += workComplete;

            tree = null;
            proof = null;

            setStatus("Initialized");
            
            setEnabled(bParse, true);
            setEnabled(bSatisfy, true);
            setEnabled(bStop, false);
            setEnabled(bShowTree, false);
            setEnabled(bShowProof, false);
            setEnabled(bSaveTree, false);
            setEnabled(bSaveProof, false);

            setTab(1);
        }

        private void workComplete(object sender, WorkFinishedEventArgs<Tree> args)
        {
            string status;
            if (args.Completed)
            {
                tree = args.Result;

                setEnabled(bShowTree, true);
                setEnabled(bSaveTree, true);

                if (tree.State == TreeState.Closed)
                {
                    proof = new Subproof(premises);
                    Subproof sproof = (Subproof)proof;
                    proof.FirstLineNumber = 1;
                    List<WFF> assumptions = new List<WFF>();
                    assumptions.Add(conclusion.GetNegation());
                    Subproof sub = new Subproof(assumptions);
                    sub.parent = proof;
                    sub.AddTree(tree);
                    sproof.steps.Add(sub);
                    sproof.steps.Add(new ProofLine(conclusion, FOL.Proofs.Rule.NotIntro, sub));
                    proof.Clean();

                    setEnabled(bShowProof, true);
                    setEnabled(bSaveProof, true);
                    setText(lConclusion, "Valid");
                }
                else if (tree.State == TreeState.Open)
                {
                    setText(lConclusion, "Invalid");
                }
                else
                {
                    setText(lConclusion, "Unknown");
                }
                
                setTab(2);
                status = "Done";
            }
            else if (args.Cancelled)
            {
                status = "Stopped";
            }
            else
            {
                status = "Error";
            }

            setStatus(status);
            setText(lTime, args.Time.ToString());

            setEnabled(bParse, true);
            setEnabled(bSatisfy, false);
            setEnabled(bStop, false);
        }

        private void bSatisfy_Click(object sender, EventArgs e)
        {
            if (worker == null) { return; }

            worker.Start();
            setStatus("Working");

            setEnabled(bParse, false);
            setEnabled(bSatisfy, false);
            setEnabled(bStop, true);
        }

        private void bStop_Click(object sender, EventArgs e)
        {
            if (worker == null) { return; }

            worker.Stop();
            setEnabled(bParse, true);
            setEnabled(bSatisfy, false);
            setEnabled(bStop, false);
        }

        private void bShowTree_Click(object sender, EventArgs e)
        {
            if (tree == null) { return; }

            DrawableViewer tViewer = new DrawableViewer(tree, "Tree");
            tViewer.Show();
        }

        private void bShowProof_Click(object sender, EventArgs e)
        {
            if (proof == null) { return; }

            DrawableViewer pViewer = new DrawableViewer(proof, "Proof");
            pViewer.Show();
        }

        private void bSaveTree_Click(object sender, EventArgs e)
        {
            if (tree == null) { return; }

            if (sfdSave.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                tree.SaveImage(sfdSave.FileName);
            }
        }

        private void bSaveProof_Click(object sender, EventArgs e)
        {
            if (proof == null) { return; }

            if (sfdSave.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                proof.SaveImage(sfdSave.FileName);
            }
        }
    }
}
