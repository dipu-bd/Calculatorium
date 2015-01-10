using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Calculatorium.Properties;

namespace AdvancedMath
{   
    public class Evaluation
    {        
        static public Dictionary<string, double> methods;
        string ein = ErrMsg.unknown_error; string error = ErrMsg.unknown_error;
        public double eval(string exp)
        {            
            loadMethods();
            if (constants == null) loadConstants();
            exp = getExp(exp.Replace(" ", "").Replace('\n', ';').ToLower());
            if (exp == "NaN") { setError(error, ein); return double.NaN; }
            double res = Bracket(exp);
            if (res.ToString() == "NaN") setError(error, ein);
            return res;
        }

        public Dictionary<string, double> variables;
        public Dictionary<string, double> constants;
        public string getExp(string exp)
        {
            if (exp == "") { ein = "Expression"; error = ErrMsg.empty_exp ; return "NaN"; }
            if (variables != null) variables.Clear();
            else variables = new Dictionary<string, double>();
            string[] ea = exp.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            if (ea.Length == 1) return ea[0];
            //get variables
            for (int i = 1; i < ea.Length; i++)
            {
                try
                {
                    if (!ea[i].Contains("=")) throw new Exception();
                    string[] a = ea[i].Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                    double e = Bracket(a[1]);
                    if (e.ToString() == "NaN")
                    { ein = "At '" + ea[i] + "'"; error = ErrMsg.invalid_variable; setError(error, ein); return "NaN"; }
                    variables.Add(a[0], e);
                }
                catch
                { error = ErrMsg.invalid_variable; ein = "At '" + ea[i] + "'"; return "NaN"; }
            }
            return ea[0];
        }

        public double Bracket(string exp)
        {
            if (isNumeric(exp)) return double.Parse(exp);
            int p = exp.LastIndexOf('(');
            if (p < 0) return Absolute(exp);
            do
            {
                methods["Bracket"]++;
                int q = exp.IndexOf(')', p);
                if (q == -1) q = exp.Length;
                string left = "", middle = "", right = "";
                left = exp.Substring(0, p);
                if (left.Length > 0) { if (isNumeric(left[left.Length - 1].ToString())) left += "#"; }
                middle = exp.Substring(p + 1, q - p - 1);
                if (q < exp.Length - 1) right = exp.Substring(q + 1, exp.Length - q - 1);
                if (right.Length > 0) { if (isNumeric(right[0].ToString())) right = "#" + right; }
                exp = left + Absolute(middle).ToString() + right;
                p = exp.LastIndexOf('(');
            } while (p > -1);
            return Absolute(exp);
        }

        public double Absolute(string exp)
        {
            double res = 0; if (double.TryParse(exp, out res)) return res;
            int p = exp.IndexOf("|");
            if (p < 0) return PlusMinus(exp);
            do
            {
                methods["Absolute"]++;
                int q = exp.IndexOf("|", p + 1);
                if (q == -1) q = exp.Length;
                string left, middle, right;
                left = exp.Substring(0, p);
                if (left.Length > 0) { if (isNumeric(left[left.Length - 1].ToString())) left += "#"; }
                middle = exp.Substring(p + 1, q - p - 1);
                right = exp.Substring(q + 1, exp.Length - q - 1);
                if (right.Length > 0) { if (isNumeric(right[0].ToString())) right = "#" + right; }
                exp = left + Math.Abs(PlusMinus(middle)) + right;
                p = exp.LastIndexOf('(');
            } while (p > -1);
            return PlusMinus(exp);
        }

        public double PlusMinus(string exp)
        {
            double rs = 0; if (double.TryParse(exp, out rs)) return rs;
            try
            {
                exp = exp.Replace("+-", "-").Replace("--", "+").Replace("-", "+-");                
                if (!exp.Contains("+")) return Cross(exp);
                methods["PlusMinus"]++; double res = 0;
                foreach (string p in exp.Split(new string[] { "+" }, StringSplitOptions.RemoveEmptyEntries))
                { if (getNumeric(exp, out rs)) res += rs; else res += Cross(p); }
                return res;
            }
            catch { error = ErrMsg.addition_error; ein = exp; return double.NaN; }
        }
        public double Cross(string exp)
        {
            if (isNumeric(exp)) return double.Parse(exp);
            if (!exp.Contains("*")) return Division(exp);
            try
            {
                methods["Multiply"]++; double res = 1;          
                foreach (string p in exp.Split(new string[] { "*" }, StringSplitOptions.RemoveEmptyEntries))
                { double rs; if (getNumeric(exp, out rs)) res *= rs; else res *= Division(p); }
                return res;
            }
            catch { error = ErrMsg.multiply_error; ein = exp; return double.NaN; }
        }

        public double Division(string exp)
        {
            if (isNumeric(exp)) return double.Parse(exp);
            if (!exp.Contains("/")) return BCross(exp);
            try
            {
                methods["Division"]++; double res=1;
                string[] exps = exp.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = exps.Length - 1; i >= 0; i--)
                {
                    string p = exps[i].ToString(); double rs;
                    if (getNumeric(exp, out rs)) res = rs / res; else res = BCross(p) / res;
                }
                return res;
            }
            catch { error = ErrMsg.division_error; ein = exp; return double.NaN; }
        }
        public double BCross(string exp)
        {
            double res = 0; if (getNumeric(exp, out res)) return res;
            if (!exp.Contains("#")) return Power(exp);
            methods["Bracket Cross"]++; methods["Multiply"]--;
            return Cross(exp.Replace("#", "*"));
        }
        /* need more review to unlease
        public double PandC(string exp)
        {            
            if (isNumeric(exp)) return double.Parse(exp); string sp = "";
            if (!(exp.Contains("p") || exp.Contains("c"))) return Power(exp);
            try
            {
                methods["Permutation and Combination"]++;
                sp = (exp.Contains("p")) ? "p" : "c";
                string[] num = exp.Split(new string[] { sp }, StringSplitOptions.None);
                double tn, tr; int n = 0, r = 0; double res = 1;
                if (!getNumeric(num[0], out tn) || !getNumeric(num[1], out tr)) throw new Exception();
                n = (int)tn; r = (int)tr;
                if (n < 0) { res = -1; n *= -1; } if ((r > n) || (r < 0) || (n == 0))
                { error = (sp == "p") ? ErrMsg.permu_error_t1 : ErrMsg.combi_error_t1; ein = exp; return double.NaN; }
                decimal up = 1, down = 1;
                for (int i = 0; i < r; i++) { up *= n - i; down *= r - i; }
                res *= (sp == "c") ? (double)(up / down) : (double)up; return res;
            }
            catch { error = ErrMsg.pc_error_t0; ein = exp; return double.NaN; }
        }
         */

        public double Power(string exp)
        {
            if (isNumeric(exp)) return double.Parse(exp);
            if (!exp.Contains("^")) Percent(exp);
            try
            {
                double res = 1;
                string[] exps = exp.Split(new string[] { "^" }, StringSplitOptions.RemoveEmptyEntries);                
                for (int i = exps.Length - 1; i >= 0; i--)
                {
                    string p = exps[i].ToString(); double rs;
                    if (getNumeric(p, out rs)) res = Math.Pow(rs, res);
                    else res = Math.Pow(Percent(p), res);
                } 
                methods["Power"]++;
                return res;                
            }
            catch { error = ErrMsg.power_error; ein = exp; return double.NaN; }
        }
        public double Percent(string exp)
        {
            if (isNumeric(exp)) return double.Parse(exp);
            if (!exp.Contains("%")) return Factorial(exp);
            try
            {
                methods["Percentage"]++;
                exp = exp.Replace("%", ""); double res = 0;
                if (!isNumeric(exp)) exp = Factorial(exp).ToString();
                res = double.Parse(exp) / 100; return res;
            }
            catch { error = ErrMsg.percentage_error; ein = exp; return double.NaN; }
        }
        public double Factorial(string exp)
        {
            if (isNumeric(exp)) return double.Parse(exp);
            if (!exp.Contains("!")) return Functions(exp);
            try
            {
                methods["Factorial"]++; exp = exp.Replace("!", "");
                if (!isNumeric(exp)) exp = Functions(exp).ToString();
                double res = 1; int n = (int)(double.Parse(exp));
                if (n == 0) return 1;
                for (int i = 1; i <= n; i++) res *= i;
                return res;
            }
            catch { error = ErrMsg.factorial_error ; ein = exp; return double.NaN; }
        }
        public double Functions(string exp)
        {
            try
            {
                if (isNumeric(exp)) return double.Parse(exp);
                if (exp.Contains("arc")) return invtrigFunc(exp);
                else if (exp.Contains("h")) return hyperbolicFunc(exp);
                else if (exp.Contains("log") || exp.Contains("ln")) return logarithms(exp);
                else return trigonometric(exp);
            }
            catch { error = ErrMsg.function_error; ein = exp; return double.NaN; }
        }

        string[] tratio = { "sin", "cos", "tan", "cot", "cosec", "sec" };
        public double hyperbolicFunc(string exp)
        {
            try
            {
                methods["Hyperbolic functions"]++;
                exp = exp.Replace("h", ""); int pos = -1;
                for (int i = 0; i < 6; i++)
                { if (exp.Contains(tratio[i])) { pos = i; break; } }
                if (pos == -1) return double.NaN;
                string[] nums = exp.Split(new string[] { tratio[pos] }, StringSplitOptions.None);
                if (nums[0] == "") nums[0] = "1"; double num1 = double.Parse(nums[0]), num2;
                if (!getNumeric(nums[1], out num2)) throw new Exception();
                if (pos == 0) { return num1 * Math.Sinh(num2); }
                else if (pos == 1) { return num1 * Math.Cosh(num2); }
                else if (pos == 2) { return num1 * Math.Tanh(num2); }
                else if (pos == 3) { return num1 / Math.Tanh(num2); }
                else if (pos == 4) { return num1 / Math.Sinh(num2); }
                else if (pos == 5) { return num1 / Math.Cosh(num2); }
                else return double.NaN;
            }
            catch { error = ErrMsg.hyperbolic_error; ein = exp; return double.NaN; }
        }
        public double trigonometric(string exp)
        {
            try
            {
                methods["Trigonometry"]++;
                int pos = -1; for (int i = 0; i < 6; i++)
                { if (exp.Contains(tratio[i])) { pos = i; break; } }
                if (pos == -1) return double.NaN;
                string[] nums = exp.Split(new string[] { tratio[pos] }, StringSplitOptions.None);
                if (nums[0] == "") nums[0] = "1"; double num1=double.Parse(nums[0]), num2;
                if (!getNumeric(nums[1], out num2)) throw new Exception();
                if (pos == 0) { return num1 * Math.Sin(num2); }
                else if (pos == 1) { return num1 * Math.Cos(num2); }
                else if (pos == 2) { return num1 * Math.Tan(num2); }
                else if (pos == 3) { return num1 / Math.Tan(num2); }
                else if (pos == 4) { return num1 / Math.Sin(num2); }
                else if (pos == 5) { return num1 / Math.Cos(num2); }
                else return double.NaN;
            }
            catch
            { error = ErrMsg.trigonometric_error; ein = exp; return double.NaN; }
        }
        public double invtrigFunc(string exp)
        {
            try
            {
                methods["Inverse Trigonometric"]++;
                exp = exp.Replace("arc", ""); int pos = -1;
                for (int i = 0; i < 6; i++)
                { if (exp.Contains(tratio[i])) { pos = i; break; } }
                if (pos == -1) return double.NaN;
                string[] nums = exp.Split(new string[] { tratio[pos] }, StringSplitOptions.None);
                if (nums[0] == "") nums[0] = "1"; double num1 = double.Parse(nums[0]), num2;
                if (!getNumeric(nums[1], out num2)) throw new Exception();
                if (pos == 0) { return num1 * Math.Asin(num2); }
                else if (pos == 1) { return num1 * Math.Acos(num2); }
                else if (pos == 2) { return num1 * Math.Atan(num2); }
                else if (pos == 3) { return num1 * Math.Atan(1 / num2); }
                else if (pos == 4) { return num1 * Math.Asin(1 / num2); }
                else if (pos == 5) { return num1 * Math.Acos(1 / num2); }
                else return double.NaN;
            }
            catch  { error = ErrMsg.invTrig_error ; ein = exp; return double.NaN; }
        }
        public double logarithms(string exp)
        {
            try
            {
                methods["Logarithm"]++;
                string chr = (exp.Contains("ln")) ? "ln" : "log";
                string[] nums = exp.Split(new string[] { chr }, StringSplitOptions.None);
                if (nums[0] == "") nums[0] = "10";  if (chr == "ln") nums[0] = Math.E.ToString();
                double num1, num2;  if (!getNumeric(nums[0], out num1) ||
                    !getNumeric(nums[1], out num2)) throw new Exception();
                double res = Math.Log(num2, num1); return res;
            }
            catch { error = ErrMsg.log_error; ein = exp; return double.NaN; }
        }
        

        /*need review before remove
         public string varsAndCons(string exp)
        {
            try
            {                
                string fexp = "";
                for (int i = 0; i < exp.Length; i++)
                {
                    string chr = exp[i].ToString();
                    if (isNumeric(chr)) { fexp += chr; continue; }
                    double val = 0; bool changed = false;
                    if (variables != null) { changed = variables.ContainsKey(chr); val = changed ? variables[chr] : 0; }
                    if ((!changed) && (constants != null)) { changed = constants.ContainsKey(chr); val = changed ? constants[chr] : 0; }
                    if (changed)
                    {
                        if (fexp.Length > 0) fexp += (isNumeric(fexp[fexp.Length - 1].ToString())) ? "*" : "";
                        fexp += val;
                        if (i < exp.Length - 1) fexp += (isNumeric(exp[i + 1].ToString())) ? "*" : "";
                    }
                    else { fexp += chr; }
                }
                return fexp;
            }
            catch (Exception ex)
            { error = ErrMsg.var_not_found; ein = ex.Message; return "NaN"; }
        }
        */
        public bool getNumeric(string exp, out double res)
        {
            if (double.TryParse(exp, out res)) return true; 
            int indx1 = 0, indx2 = exp.Length - 1, begin = 1;
            if (exp.StartsWith("-")) { begin = -1; exp = exp.Remove(0, 1); }
            //get number from left
            for (int i = 0; i < exp.Length; i++)
            { int c = char.ConvertToUtf32(exp, i); if (c < 46 && c > 57 && c != 47) { indx1 = i; break; } }
            //get numbers
            try
            {
                double num1 = 1; string Var;
                if (indx1 == 0) { Var = exp; }
                else
                {
                    num1 = double.Parse(exp.Substring(0, indx1));
                    Var = exp.Substring(indx1, exp.Length - indx1 - 1);
                }
                bool vc = variables.ContainsKey(Var);
                bool cc = constants.ContainsKey(Var);
                if (!(vc || cc)) throw new Exception((vc) ? "v" : "c");
                double val = (vc) ? variables[Var] : constants[Var];
                res = begin * num1 * val; return true;
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("v")) error = ErrMsg.var_not_found; else error = ErrMsg.cons_not_found;
                ein = "At " + exp + ", "; return false;
            }
        }
        ///////////////////Other Functions/////////////////////
        public void setError(string message, string index)
        {
            TextBox msgbox = (TextBox)Application.OpenForms[0].Controls[0].Controls["groupBox3"].Controls["messageBox"];
            msgbox.Text = "Calculation Failed" + Environment.NewLine +
                "Error: " + message + Environment.NewLine + "Position: " + index;
        }
        public bool isNumeric(string num)
        { double res; return double.TryParse(num, out res); }
        public void loadConstants()
        {
            constants = new Dictionary<string,double>();
            foreach (string citm in Resources.Constants.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
            {                
                string[] itm = citm.Split(new char[] { '=' });
                constants.Add(itm[0].ToLower(), double.Parse(itm[1]));
            }
        }
        public void loadMethods()
        {
            methods = new Dictionary<string, double>();
            methods.Add("Bracket", 0);
            methods.Add("Absolute", 0);
            methods.Add("PlusMinus", 0);
            methods.Add("Multiply", 0);
            methods.Add("Division", 0);
            methods.Add("Bracket Cross", 0);
            methods.Add("Power", 0);
            methods.Add("Percentage", 0);
            methods.Add("Factorial", 0);
            methods.Add("Permutation and Combination", 0);
            methods.Add("Logarithm", 0);
            methods.Add("Trigonometry", 0);
            methods.Add("Inverse Trigonometric", 0);
            methods.Add("Hyperbolic functions", 0);
        }
    }
}

/*
 * load constants
 * fix varsAndCons method
 */