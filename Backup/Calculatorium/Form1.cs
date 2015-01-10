using System;
using Calculatorium.Properties;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using AdvancedMath;

namespace Calculatorium
{
    public partial class Form1 : Form
    {
        public Form1()
        { InitializeComponent(); }

        private void evalButton_Click(object sender, EventArgs e)
        {
            string exp = inBox.Text;
            if (exp.ToLower().Contains("constant")) { outBox.Text = Resources.Constants; return; }
            messageBox.Text = ""; 
            Evaluation Eval = new Evaluation();
            outBox.Text = Eval.eval(exp).ToString();
            if (outBox.Text != "NaN")
                messageBox.Text = "Calculated result successfully.";
            messageBox.Text += Environment.NewLine + "Method used: " + Environment.NewLine + getMethods();
        }

        string getMethods()
        {
            string res = ""; 
            foreach (string val in Evaluation.methods.Keys)
            {
                if (Evaluation.methods[val] == 0) continue;
                res += val + " (" + Evaluation.methods[val] + " times)" + Environment.NewLine;
            }
            return res;
        }
    }
}
