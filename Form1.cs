using System;
using System;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using static System.Math;

namespace OrbitalApp
{

    public partial class Form1 : Form
    {
        /* Uses three dictionaries for each variable:
         * d_textbox maps the variable to its input/output textbox on the app.
         * d maps the variable to its value.
         * d_status stores whether each variable is currently known or not 
         *      (to decide when formulae can be applied)
         * */

        static Dictionary<string, TextBox> d_textbox;
        static Dictionary<string, double> d;
        static Dictionary<string, bool> d_status;
        bool consistent_system;

        public Form1()
        {
            InitializeComponent();
        }

        private void CreateDictionaries()
        {
            //initialises the dictionaries with all the textbox names,
            //with values of 0 and unknown status for all variables.


            d = new Dictionary<string, double>();
            d_textbox = new Dictionary<string, TextBox>();
            d_status = new Dictionary<string, bool>();

            {
                d_textbox.Add("a", textbox_a);
                d_textbox.Add("b", textbox_b);
                d_textbox.Add("e", textbox_e);
                d_textbox.Add("p", textbox_p);
                d_textbox.Add("r_a", textbox_r_a);
                d_textbox.Add("r_p", textbox_r_p);
                d_textbox.Add("r", textbox_r);
                d_textbox.Add("theta", textbox_theta);
                d_textbox.Add("v", textbox_v);
                d_textbox.Add("h", textbox_h);
                d_textbox.Add("mu", textbox_mu);
                d_textbox.Add("eps", textbox_eps);
                d_textbox.Add("T", textbox_T);
                d_textbox.Add("M_e", textbox_M_e);
                d_textbox.Add("E", textbox_E2);
            }

            foreach(var item in d_textbox)
            {
                d.Add(item.Key, 0);
                d_status.Add(item.Key, false);
            }
            
        }

        private void UpdateDictionaries()
        {
            //Called after the user presses the "Go" button, and
            //updates the dictionaries to reflect the current user input.
            foreach(var item in d_textbox)
            {
                if(item.Value.Text == "")
                {
                    d_status[item.Key] = false;
                }
                else
                {
                    d_status[item.Key] = true;
                    d[item.Key] = Convert.ToDouble(item.Value.Text);
                }
            }
        }

        private void ChangeValue(string parameter, double value)
        {
            //Changes the dictionaries to reflect a formula application. If
            // this doesn't match the user input, a consistency error is raised
            if (d_textbox[parameter].Text == "")
            {
                d_status[parameter] = true;
                d[parameter] = value;
            }
            else if (d_textbox[parameter].Text != Convert.ToString(value))
            {
                consistent_system = false;
            }
        }

        private void UpdateScreen()
        {
            //Updates the screen to match a change in the dictionaries.
            foreach(var item in d_textbox)
            {
                if (d_status[item.Key])
                {
                    item.Value.Text = Convert.ToString(d[item.Key]);
                }
            }
        }

        private void Evaluate()
        {
            //For each parameter, iterates through the formulae it to the rest,
            //  and updates it if there's enough info.

            //a
            {
                if (d_status["r_a"] & d_status["r_p"])
                    ChangeValue("a", (d["r_a"] + d["r_p"]) / 2);
                else if (d_status["r_a"] & d_status["e"])
                    ChangeValue("a", d["r_a"] / (1 + d["e"]));
                else if (d_status["r_p"] & d_status["e"])
                    ChangeValue("a", d["r_p"] / (1 - d["e"]));
                else if (d_status["p"] & d_status["e"])
                    ChangeValue("a", d["p"] / (1 - Pow(d["e"], 2)));
                else if (d_status["b"] & d_status["e"])
                    ChangeValue("a", d["b"] / Sqrt(1 - Pow(d["e"], 2)));
                else if (d_status["b"] & d_status["p"])
                    ChangeValue("a", Pow(d["b"], 2) / d["p"]);
                else if (d_status["mu"] & d_status["eps"])
                    ChangeValue("a", d["mu"] / (-2 * d["eps"]));
                else if (d_status["mu"] & d_status["T"])
                    ChangeValue("a", Pow(Pow(d["T"] / (2 * PI), 2) * d["mu"], 1 / 3));
            }

            //b
            {
                if (d_status["a"] & d_status["p"])
                    ChangeValue("b", Sqrt(d["a"] * d["p"]));
                else if (d_status["a"] & d_status["p"])
                    ChangeValue("b", d["a"] * Sqrt(1 - Pow(d["e"], 2)));
            }

            //e
            {
                if (d_status["r_a"] & d_status["a"])
                    ChangeValue("e", d["r_a"] / d["a"] - 1);
                else if (d_status["r_p"] & d_status["a"])
                    ChangeValue("e", 1 - (d["r_p"] / d["a"]));
                else if (d_status["p"] & d_status["a"])
                    ChangeValue("e", Sqrt(1 - (d["p"] / d["a"])));
                else if (d_status["b"] & d_status["a"])
                    ChangeValue("e", Sqrt(1 - Pow(d["b"] / d["a"], 2)));
                else if (d_status["theta"] & d_status["r"] & d_status["p"])
                    ChangeValue("e", (d["p"] / d["r"] - 1) / Cos(d["theta"]));
                else if (d_status["E"] & d_status["theta"]) //this is particularly nasty
                    ChangeValue("e", (1 - Pow((Tan(d["E"] / 2) / Tan(d["theta"] / 2)), 2)) / (1 + Pow((Tan(d["E"] / 2) / Tan(d["theta"] / 2)), 2)));
                else if (d_status["M_e"] & d_status["E"])
                    ChangeValue("e", (d["E"] - d["M_e"]) / Sin(d["E"]));
            }

            //p
            {
                if (d_status["a"] & d_status["e"])
                    ChangeValue("p", d["a"] * (1 - Pow(d["e"], 2)));
                else if (d_status["a"] & d_status["b"])
                    ChangeValue("p", d["a"] / Pow(d["b"], 2));
                else if (d_status["r"] & d_status["e"] & d_status["theta"])
                    ChangeValue("p", d["r"] * (1 + d["e"] * Cos(d["theta"])));
                else if (d_status["h"] & d_status["mu"])
                    ChangeValue("p", Pow(d["h"], 2) / d["mu"]);
            }

            //r_a
            {
                if (d_status["a"] & d_status["e"])
                    ChangeValue("r_a", d["a"] * (1 + d["e"]));
                else if (d_status["a"] & d_status["r_p"])
                    ChangeValue("r_a", 2 * d["a"] - d["r_p"]);
            }

            //r_p
            {
                if (d_status["a"] & d_status["r_a"])
                    ChangeValue("r_p", 2 * d["a"] - d["r_a"]);
                else if (d_status["a"] & d_status["e"])
                    ChangeValue("r_a", d["a"] * (1 - d["e"]));
            }

            //r
            {
                if (d_status["p"] & d_status["e"] & d_status["theta"])
                    ChangeValue("r", d["p"] / (1 + d["e"] * Cos(d["theta"])));
                if (d_status["h"] & d_status["v"])
                    ChangeValue("r", d["h"] / d["v"]);
            }

            //theta
            {
                if (d_status["p"] & d_status["r"] & d_status["e"])
                    ChangeValue("theta", Math.Acos((d["p"] / d["r"] - 1) / d["e"]));
                if (d_status["E"] & d_status["e"])
                    ChangeValue("theta", 2 * Atan(Sqrt((1 + d["e"]) / (1 - d["e"])) * Tan(d["E"] / 2)));

            }

            //h
            {
                if (d_status["r"] & d_status["v"])
                    ChangeValue("h", d["r"] * d["v"]);
                if (d_status["p"] & d_status["mu"])
                    ChangeValue("h", Sqrt(d["p"]*d["mu"]));
            }

            //v
            {
                if (d_status["h"] & d_status["r"])
                    ChangeValue("v", d["h"] / d["r"]);
            }

            //mu
            {
                if (d_status["p"] & d_status["h"])
                    ChangeValue("mu", Pow(d["h"], 2) / d["p"]);
                if (d_status["eps"] & d_status["a"])
                    ChangeValue("mu", -2 * d["eps"] * d["a"]);
                if (d_status["a"] & d_status["T"])
                    ChangeValue("mu", Pow(d["a"], 3) * Pow((2*PI)/d["T"], 2));
            }

            //eps
            {
                if (d_status["mu"] & d_status["a"])
                    ChangeValue("eps", d["mu"] / (-2 * d["a"]));
            }

            //T
            {
                if (d_status["a"] & d_status["mu"])
                    ChangeValue("T", 2 * PI * Sqrt(Pow(d["a"], 3) / d["mu"]));
            }

            //E
            {
                if (d_status["theta"] & d_status["e"])
                    ChangeValue("E", 2 * Atan(Sqrt((1-d["e"]) / (1 + d["e"]))*Tan(d["theta"] / 2)));
            }

            //M_e
            if (d_status["e"] & d_status["E"])
                ChangeValue("M_e", d["E"] - d["e"] * Sin(d["E"]));

        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            //Clears the input
            foreach(var item in d_textbox)
            {
                item.Value.Text = "";
            }
            inconsistent_label.Visible = false;
        }

        private void GoButton_Click(object sender, EventArgs e)
        {
            //Iterates through the evaluation procedure a couple of times, to make
            //  sure all parameters which can be found have been. If no consistency 
            //  errors have been raised, the screen is updated with results.

            consistent_system = true;
            inconsistent_label.Visible = false;

            UpdateDictionaries();
            for (var i = 1; i <= 2; i++)
            {
                Evaluate();
            }
            if (consistent_system)
            {
                UpdateScreen();
            }
            else
            {
                inconsistent_label.Visible = true;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CreateDictionaries();
        }

        private void textbox_h_TextChanged(object sender, EventArgs e)
        {

        }

        private void label18_Click(object sender, EventArgs e)
        {

        }
    }
}
