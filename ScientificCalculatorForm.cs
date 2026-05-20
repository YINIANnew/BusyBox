using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Numerics;
using System.Security.Cryptography;
using System.IO;
using System.Text.RegularExpressions;

namespace BusyBox
{
    public class ScientificCalculatorForm : Form
    {
        private TextBox _display;
        private TextBox _expressionDisplay;
        private List<string> _history = new List<string>();
        private bool _isDegreeMode = true;
        private ComboBox _baseComboBox;
        private Label _modeLabel;
        private string _currentBase = "10";
        private ListBox _historyListBox;
        private double _memoryValue = 0;
        private double _lastAnswer = 0;

        public ScientificCalculatorForm()
        {
            Text = "科学计算器";
            Size = new Size(700, 720);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(30, 30, 30);
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            MinimizeBox = true;
            KeyPreview = true;
            KeyDown += ScientificCalculatorForm_KeyDown;
            MinimumSize = new Size(700, 720);
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            _expressionDisplay = new TextBox
            {
                Location = new Point(20, 20),
                Size = new Size(660, 35),
                Font = new Font("Consolas", 12),
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.Gray,
                ReadOnly = true,
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = HorizontalAlignment.Right
            };
            Controls.Add(_expressionDisplay);

            _display = new TextBox
            {
                Location = new Point(20, 60),
                Size = new Size(660, 55),
                Font = new Font("Consolas", 26, FontStyle.Bold),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                ReadOnly = false,
                BorderStyle = BorderStyle.FixedSingle,
                TextAlign = HorizontalAlignment.Right,
                Text = "0",
                Cursor = Cursors.IBeam,
                Enabled = true
            };
            _display.MouseClick += Display_MouseClick;
            _display.KeyDown += Display_KeyDown;
            Controls.Add(_display);

            var topPanel = new Panel
            {
                Location = new Point(20, 120),
                Size = new Size(660, 35),
                BackColor = Color.FromArgb(30, 30, 30)
            };
            Controls.Add(topPanel);

            var historyButton = new Button
            {
                Text = "历史",
                Location = new Point(5, 5),
                Size = new Size(90, 25),
                BackColor = Color.FromArgb(70, 70, 70),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9)
            };
            historyButton.Click += HistoryButton_Click;
            topPanel.Controls.Add(historyButton);

            _modeLabel = new Label
            {
                Text = "DEG",
                Location = new Point(105, 7),
                Size = new Size(45, 20),
                ForeColor = Color.FromArgb(155, 89, 182),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            topPanel.Controls.Add(_modeLabel);

            var modeButton = new Button
            {
                Text = "切换角度/弧度",
                Location = new Point(155, 5),
                Size = new Size(110, 25),
                BackColor = Color.FromArgb(155, 89, 182),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8)
            };
            modeButton.Click += ModeButton_Click;
            topPanel.Controls.Add(modeButton);

            var baseLabel = new Label
            {
                Text = "进制:",
                Location = new Point(500, 7),
                Size = new Size(40, 20),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9)
            };
            topPanel.Controls.Add(baseLabel);

            _baseComboBox = new ComboBox
            {
                Location = new Point(545, 5),
                Size = new Size(55, 22),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _baseComboBox.Items.AddRange(new[] { "2", "8", "10", "16" });
            _baseComboBox.SelectedIndex = 2;
            _baseComboBox.SelectedIndexChanged += BaseComboBox_SelectedIndexChanged;
            topPanel.Controls.Add(_baseComboBox);

            var sysCalcButton = new Button
            {
                Text = "系统计算器",
                Location = new Point(605, 5),
                Size = new Size(95, 25),
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8)
            };
            sysCalcButton.Click += (s, e) => {
                try { System.Diagnostics.Process.Start("calc.exe"); }
                catch { }
            };
            topPanel.Controls.Add(sysCalcButton);

            _historyListBox = new ListBox
            {
                Location = new Point(520, 165),
                Size = new Size(160, 480),
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.White,
                Font = new Font("Consolas", 9),
                Visible = false,
                BorderStyle = BorderStyle.FixedSingle
            };
            _historyListBox.DoubleClick += HistoryListBox_DoubleClick;
            Controls.Add(_historyListBox);

            int startY = 165;
            int buttonHeight = 40;
            int buttonWidth = 60;
            int gap = 6;
            AddBasicButtons(startY, buttonHeight, buttonWidth, gap);
        }

        private void Display_MouseClick(object sender, MouseEventArgs e)
        {
            if (_display.Text == "0" || _display.Text == "Error" || 
                _display.Text == "无效表达式" || _display.Text == "无效操作" || 
                _display.Text == "超出范围" || _display.Text == "计算错误")
            {
                _display.Text = "";
            }
        }

        private void Display_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return || e.KeyCode == Keys.Enter)
            {
                Calculate();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                ClearAll();
                e.SuppressKeyPress = true;
            }
        }

        private void AddBasicButtons(int startY, int buttonHeight, int buttonWidth, int gap)
        {
            string[][] buttonRows = {
                new[] { "(", ")", "mc", "mr", "m+", "m-", "ms" },
                new[] { "x²", "√x", "!", "%", "π", "e", "C" },
                new[] { "sin", "cos", "tan", "ln", "log", "EE", "CE" },
                new[] { "asin", "acos", "atan", "eˣ", "10ˣ", "1/x", "⌫" },
                new[] { "7", "8", "9", "÷", "xʸ", "x³", "±" },
                new[] { "4", "5", "6", "×", "n!", "mod", "(" },
                new[] { "1", "2", "3", "-", "deg", "rad", ")" },
                new[] { "0", ".", "=", "+", "ans", "EXP", "n!" }
            };

            for (int rowIndex = 0; rowIndex < buttonRows.Length; rowIndex++)
            {
                AddButtonRow(startY + rowIndex * (buttonHeight + gap), buttonRows[rowIndex], buttonWidth, buttonHeight, gap);
            }

            var specialPanel = new Panel
            {
                Location = new Point(20, startY + 8 * (buttonHeight + gap) + 10),
                Size = new Size(480, buttonHeight),
                BackColor = Color.FromArgb(30, 30, 30)
            };
            Controls.Add(specialPanel);

            string[] specialButtons = { "进制转换", "编码转换", "哈希计算", "CRC校验", "微积分" };
            int specialWidth = 94;
            for (int i = 0; i < specialButtons.Length; i++)
            {
                var btn = new Button
                {
                    Text = specialButtons[i],
                    Location = new Point(i * (specialWidth + 3), 2),
                    Size = new Size(specialWidth, buttonHeight - 4),
                    BackColor = Color.FromArgb(52, 152, 219),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 7)
                };
                int index = i;
                btn.Click += (s, e) => OpenSpecialTool(index);
                specialPanel.Controls.Add(btn);
            }
        }

        private void AddButtonRow(int y, string[] buttons, int width, int height, int gap)
        {
            int totalWidth = buttons.Length * width + (buttons.Length - 1) * gap;
            int startX = (480 - totalWidth) / 2 + 20;

            for (int i = 0; i < buttons.Length; i++)
            {
                var btn = new Button
                {
                    Text = buttons[i],
                    Location = new Point(startX + i * (width + gap), y),
                    Size = new Size(width, height),
                    BackColor = Color.FromArgb(70, 70, 70),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", IsDigit(buttons[i]) ? 14 : 8.5f, IsDigit(buttons[i]) ? FontStyle.Bold : FontStyle.Regular),
                    Margin = new Padding(2)
                };

                if (IsDigit(buttons[i]) || buttons[i] == "." || buttons[i] == "π" || buttons[i] == "e")
                    btn.BackColor = Color.FromArgb(80, 80, 80);
                else if (buttons[i] == "=")
                {
                    btn.BackColor = Color.FromArgb(39, 174, 96);
                    btn.Font = new Font("Segoe UI", 14, FontStyle.Bold);
                }
                else if (buttons[i] == "C" || buttons[i] == "CE" || buttons[i] == "⌫")
                    btn.BackColor = Color.FromArgb(192, 57, 43);
                else if (buttons[i] == "sin" || buttons[i] == "cos" || buttons[i] == "tan" ||
                         buttons[i] == "asin" || buttons[i] == "acos" || buttons[i] == "atan")
                    btn.BackColor = Color.FromArgb(155, 89, 182);

                btn.Click += Button_Click;
                Controls.Add(btn);
            }
        }

        private bool IsDigit(string text)
        {
            return text.All(c => char.IsDigit(c));
        }

        private void Button_Click(object sender, EventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;

            string text = btn.Text;

            try
            {
                switch (text)
                {
                    case "C": ClearAll(); break;
                    case "CE": ClearEntry(); break;
                    case "⌫": Backspace(); break;
                    case "+": AppendInput("+"); break;
                    case "-": AppendInput("-"); break;
                    case "×": AppendInput("*"); break;
                    case "÷": AppendInput("/"); break;
                    case "(": AppendInput("("); break;
                    case ")": AppendInput(")"); break;
                    case ".": AppendInput("."); break;
                    case "π": AppendInput("pi"); break;
                    case "e": AppendInput("e"); break;
                    case "mc": MemoryClear(); break;
                    case "mr": MemoryRecall(); break;
                    case "m+": MemoryAdd(); break;
                    case "m-": MemorySubtract(); break;
                    case "ms": MemoryStore(); break;
                    case "ans": AppendInput("ans"); break;
                    case "deg": _isDegreeMode = true; _modeLabel.Text = "DEG"; break;
                    case "rad": _isDegreeMode = false; _modeLabel.Text = "RAD"; break;
                    case "EE":
                    case "EXP": AppendInput("E"); break;
                    case "mod": AppendInput("%"); break;
                    case "xʸ": AppendInput("^"); break;
                    case "=": Calculate(); break;
                    case "x²": AppendFunction("sqr"); break;
                    case "x³": AppendFunction("cube"); break;
                    case "√x": AppendFunction("sqrt"); break;
                    case "sin": AppendFunction("sin"); break;
                    case "cos": AppendFunction("cos"); break;
                    case "tan": AppendFunction("tan"); break;
                    case "asin": AppendFunction("asin"); break;
                    case "acos": AppendFunction("acos"); break;
                    case "atan": AppendFunction("atan"); break;
                    case "ln": AppendFunction("ln"); break;
                    case "log": AppendFunction("log"); break;
                    case "eˣ": AppendFunction("exp"); break;
                    case "10ˣ": AppendFunction("pow10"); break;
                    case "1/x": AppendFunction("reciprocal"); break;
                    case "%": AppendFunction("percent"); break;
                    case "!":
                    case "n!": AppendFunction("factorial"); break;
                    case "±": ToggleSign(); break;
                    default:
                        if (IsDigit(text))
                            AppendInput(text);
                        break;
                }
            }
            catch (Exception ex)
            {
                _display.Text = "Error";
                _expressionDisplay.Text = ex.Message;
            }
        }

        private void AppendInput(string input)
        {
            string current = _display.Text;
            
            if (current == "0" || current == "Error" || current == "无效表达式" ||
                current == "无效操作" || current == "超出范围" || current == "计算错误")
            {
                _display.Text = input;
                _display.SelectionStart = input.Length;
            }
            else
            {
                int cursorPos = _display.SelectionStart;
                _display.Text = _display.Text.Insert(cursorPos, input);
                _display.SelectionStart = cursorPos + input.Length;
            }
            _display.Focus();
        }

        private void AppendFunction(string func)
        {
            string current = _display.Text;
            
            if (current == "0" || current == "Error" || current == "无效表达式" || 
                current == "无效操作" || current == "超出范围" || current == "计算错误")
            {
                current = "";
            }

            try
            {
                double num;
                if (!string.IsNullOrEmpty(current) && double.TryParse(current, out num))
                {
                    double result = CalculateFunction(func, num);
                    if (double.IsNaN(result) || double.IsInfinity(result))
                    {
                        _display.Text = "超出范围";
                    }
                    else
                    {
                        _display.Text = FormatResult(result);
                        _lastAnswer = result;
                    }
                }
                else
                {
                    int cursorPos = _display.SelectionStart;
                    string funcText = func + "(" + current + ")";
                    _display.Text = funcText;
                    _display.SelectionStart = funcText.Length;
                }
            }
            catch
            {
                int cursorPos = _display.SelectionStart;
                string funcText = func + "(" + current + ")";
                _display.Text = funcText;
                _display.SelectionStart = funcText.Length;
            }
            _display.Focus();
        }

        private double CalculateFunction(string func, double num)
        {
            switch (func)
            {
                case "sqr": return num * num;
                case "cube": return num * num * num;
                case "sqrt": return Math.Sqrt(num);
                case "sin": return Math.Sin(_isDegreeMode ? num * Math.PI / 180 : num);
                case "cos": return Math.Cos(_isDegreeMode ? num * Math.PI / 180 : num);
                case "tan": return Math.Tan(_isDegreeMode ? num * Math.PI / 180 : num);
                case "asin": return _isDegreeMode ? Math.Asin(num) * 180 / Math.PI : Math.Asin(num);
                case "acos": return _isDegreeMode ? Math.Acos(num) * 180 / Math.PI : Math.Acos(num);
                case "atan": return _isDegreeMode ? Math.Atan(num) * 180 / Math.PI : Math.Atan(num);
                case "ln": return Math.Log(num);
                case "log": return Math.Log10(num);
                case "exp": return Math.Exp(num);
                case "pow10": return Math.Pow(10, num);
                case "reciprocal": return 1.0 / num;
                case "percent": return num / 100;
                case "factorial": return Factorial((int)num);
                default: return num;
            }
        }

        private long Factorial(int n)
        {
            if (n < 0) return 0;
            if (n <= 1) return 1;
            long result = 1;
            for (int i = 2; i <= n; i++)
                result *= i;
            return result;
        }

        private void MemoryClear()
        {
            _memoryValue = 0;
        }

        private void MemoryRecall()
        {
            _display.Text = _memoryValue.ToString("G10");
            _display.SelectionStart = _display.Text.Length;
            _display.Focus();
        }

        private void MemoryAdd()
        {
            if (double.TryParse(_display.Text, out double value))
                _memoryValue += value;
        }

        private void MemorySubtract()
        {
            if (double.TryParse(_display.Text, out double value))
                _memoryValue -= value;
        }

        private void MemoryStore()
        {
            if (double.TryParse(_display.Text, out double value))
                _memoryValue = value;
        }

        private void ClearAll()
        {
            _display.Text = "0";
            _expressionDisplay.Text = "";
        }

        private void ClearEntry()
        {
            _display.Text = "0";
        }

        private void Backspace()
        {
            if (_display.Text.Length > 1)
            {
                int cursorPos = _display.SelectionStart;
                if (cursorPos > 0)
                {
                    _display.Text = _display.Text.Remove(cursorPos - 1, 1);
                    _display.SelectionStart = cursorPos - 1;
                }
            }
            else
            {
                _display.Text = "0";
            }
            _display.Focus();
        }

        private void ToggleSign()
        {
            if (_display.Text.StartsWith("-"))
            {
                _display.Text = _display.Text.Substring(1);
            }
            else if (_display.Text != "0" && !string.IsNullOrEmpty(_display.Text))
            {
                _display.Text = "-" + _display.Text;
            }
            _display.SelectionStart = _display.Text.Length;
            _display.Focus();
        }

        private void Calculate()
        {
            try
            {
                string expr = _display.Text;
                if (string.IsNullOrWhiteSpace(expr))
                {
                    _display.Text = "无效表达式";
                    return;
                }

                string originalExpr = expr;
                expr = PreprocessExpression(expr);

                double result = Evaluate(expr);

                if (double.IsNaN(result) || double.IsInfinity(result))
                {
                    _display.Text = "超出范围";
                    return;
                }

                string resultStr = FormatResult(result);
                _expressionDisplay.Text = originalExpr + " =";
                _display.Text = resultStr;
                _display.SelectionStart = resultStr.Length;
                _lastAnswer = result;
                _history.Add(_expressionDisplay.Text + " " + resultStr);

                if (_historyListBox.Visible && _historyListBox.Items.Count < _history.Count)
                    _historyListBox.Items.Add(_history.Last());
            }
            catch (Exception ex)
            {
                _display.Text = "计算错误";
                _expressionDisplay.Text = ex.Message;
            }
            _display.Focus();
        }

        private string PreprocessExpression(string expr)
        {
            expr = expr.Replace("×", "*");
            expr = expr.Replace("÷", "/");
            expr = expr.Replace("pi", Math.PI.ToString());
            expr = expr.Replace("e", Math.E.ToString());
            expr = expr.Replace("ans", _lastAnswer.ToString());

            expr = Regex.Replace(expr, @"(\d)E(\d)", "$1e$2");
            expr = Regex.Replace(expr, @"(\d)e(\d)", "($1)*10^$2");

            return expr;
        }

        private string FormatResult(double num)
        {
            if (Math.Abs(num) > 1e15 || (Math.Abs(num) < 1e-10 && num != 0))
                return num.ToString("E6");
            return num.ToString("G10");
        }

        private double Evaluate(string expr)
        {
            expr = expr.Replace(" ", "");
            var tokens = Tokenize(expr);
            return ParseExpression(tokens);
        }

        private List<string> Tokenize(string expr)
        {
            var tokens = new List<string>();
            int i = 0;

            while (i < expr.Length)
            {
                if (char.IsDigit(expr[i]) || expr[i] == '.')
                {
                    int start = i;
                    while (i < expr.Length && (char.IsDigit(expr[i]) || expr[i] == '.'))
                        i++;
                    tokens.Add(expr.Substring(start, i - start));
                }
                else if (char.IsLetter(expr[i]))
                {
                    int start = i;
                    while (i < expr.Length && char.IsLetter(expr[i]))
                        i++;
                    tokens.Add(expr.Substring(start, i - start));
                }
                else
                {
                    tokens.Add(expr[i].ToString());
                    i++;
                }
            }

            return tokens;
        }

        private int _pos = 0;
        private List<string> _tokens;

        private double ParseExpression(List<string> tokens)
        {
            _tokens = tokens;
            _pos = 0;
            double result = ParseAdditive();
            if (_pos < _tokens.Count)
                throw new InvalidOperationException("表达式格式错误");
            return result;
        }

        private double ParseAdditive()
        {
            double result = ParseMultiplicative();

            while (_pos < _tokens.Count)
            {
                string op = _tokens[_pos];
                if (op == "+")
                {
                    _pos++;
                    result += ParseMultiplicative();
                }
                else if (op == "-")
                {
                    _pos++;
                    result -= ParseMultiplicative();
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        private double ParseMultiplicative()
        {
            double result = ParseExponent();

            while (_pos < _tokens.Count)
            {
                string op = _tokens[_pos];
                if (op == "*")
                {
                    _pos++;
                    result *= ParseExponent();
                }
                else if (op == "/")
                {
                    _pos++;
                    double divisor = ParseExponent();
                    if (divisor == 0)
                        throw new InvalidOperationException("除数不能为零");
                    result /= divisor;
                }
                else if (op == "%")
                {
                    _pos++;
                    result %= ParseExponent();
                }
                else
                {
                    break;
                }
            }

            return result;
        }

        private double ParseExponent()
        {
            double result = ParseUnary();

            while (_pos < _tokens.Count && _tokens[_pos] == "^")
            {
                _pos++;
                double exponent = ParseUnary();
                result = Math.Pow(result, exponent);
            }

            return result;
        }

        private double ParseUnary()
        {
            if (_pos < _tokens.Count && _tokens[_pos] == "-")
            {
                _pos++;
                return -ParsePrimary();
            }
            if (_pos < _tokens.Count && _tokens[_pos] == "+")
            {
                _pos++;
                return ParsePrimary();
            }
            return ParsePrimary();
        }

        private double ParsePrimary()
        {
            if (_pos >= _tokens.Count)
                throw new InvalidOperationException("表达式不完整");

            string token = _tokens[_pos];

            if (token == "(")
            {
                _pos++;
                double result = ParseAdditive();
                if (_pos >= _tokens.Count || _tokens[_pos] != ")")
                    throw new InvalidOperationException("缺少右括号");
                _pos++;
                return result;
            }

            if (double.TryParse(token, out double number))
            {
                _pos++;
                return number;
            }

            if (char.IsLetter(token[0]))
            {
                string func = token;
                _pos++;

                if (_pos >= _tokens.Count || _tokens[_pos] != "(")
                    throw new InvalidOperationException("函数缺少参数");
                _pos++;

                double arg = ParseAdditive();

                if (_pos >= _tokens.Count || _tokens[_pos] != ")")
                    throw new InvalidOperationException("函数缺少右括号");
                _pos++;

                return EvaluateFunction(func, arg);
            }

            throw new InvalidOperationException("未知操作数: " + token);
        }

        private double EvaluateFunction(string func, double arg)
        {
            switch (func.ToLower())
            {
                case "sin": return Math.Sin(_isDegreeMode ? arg * Math.PI / 180 : arg);
                case "cos": return Math.Cos(_isDegreeMode ? arg * Math.PI / 180 : arg);
                case "tan": return Math.Tan(_isDegreeMode ? arg * Math.PI / 180 : arg);
                case "asin": return _isDegreeMode ? Math.Asin(arg) * 180 / Math.PI : Math.Asin(arg);
                case "acos": return _isDegreeMode ? Math.Acos(arg) * 180 / Math.PI : Math.Acos(arg);
                case "atan": return _isDegreeMode ? Math.Atan(arg) * 180 / Math.PI : Math.Atan(arg);
                case "sinh": return Math.Sinh(arg);
                case "cosh": return Math.Cosh(arg);
                case "tanh": return Math.Tanh(arg);
                case "ln": return Math.Log(arg);
                case "log": return Math.Log10(arg);
                case "exp": return Math.Exp(arg);
                case "sqrt": return Math.Sqrt(arg);
                case "abs": return Math.Abs(arg);
                case "floor": return Math.Floor(arg);
                case "ceil": return Math.Ceiling(arg);
                case "round": return Math.Round(arg);
                default: throw new InvalidOperationException("未知函数: " + func);
            }
        }

        private void ModeButton_Click(object sender, EventArgs e)
        {
            _isDegreeMode = !_isDegreeMode;
            _modeLabel.Text = _isDegreeMode ? "DEG" : "RAD";
        }

        private void HistoryButton_Click(object sender, EventArgs e)
        {
            try
            {
                _historyListBox.Visible = !_historyListBox.Visible;
                if (_historyListBox.Visible && _historyListBox.Items.Count == 0)
                {
                    foreach (var item in _history)
                        _historyListBox.Items.Add(item);
                }
            }
            catch
            {
            }
        }

        private void HistoryListBox_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                if (_historyListBox.SelectedItem != null)
                {
                    string selected = _historyListBox.SelectedItem.ToString();
                    int eqIndex = selected.LastIndexOf('=');
                    if (eqIndex >= 0)
                        _display.Text = selected.Substring(eqIndex + 1).Trim();
                }
            }
            catch
            {
            }
        }

        private void BaseComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                _currentBase = _baseComboBox.SelectedItem.ToString();
            }
            catch
            {
            }
        }

        private void OpenSpecialTool(int index)
        {
            try
            {
                switch (index)
                {
                    case 0: ShowBaseConverter(); break;
                    case 1: ShowEncodingConverter(); break;
                    case 2: ShowHashCalculator(); break;
                    case 3: ShowCRCCalculator(); break;
                    case 4: ShowCalculusTool(); break;
                }
            }
            catch
            {
                MessageBox.Show("工具打开失败", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowBaseConverter()
        {
            using (var form = new Form())
            {
                form.Text = "进制转换器";
                form.Size = new Size(420, 340);
                form.StartPosition = FormStartPosition.CenterParent;
                form.BackColor = Color.FromArgb(40, 40, 40);
                form.MinimumSize = new Size(420, 340);

                var inputLabel = new Label { Text = "输入数值:", Location = new Point(20, 20), ForeColor = Color.White, Font = new Font("Segoe UI", 10) };
                var inputBox = new TextBox { Location = new Point(20, 45), Width = 370, BackColor = Color.FromArgb(60, 60, 60), ForeColor = Color.White, Font = new Font("Consolas", 12) };

                var fromLabel = new Label { Text = "源进制:", Location = new Point(20, 85), ForeColor = Color.White, Font = new Font("Segoe UI", 10) };
                var fromCombo = new ComboBox { Location = new Point(95, 83), Width = 100, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(60, 60, 60), ForeColor = Color.White, Font = new Font("Segoe UI", 9) };
                fromCombo.Items.AddRange(new[] { "二进制", "八进制", "十进制", "十六进制" });
                fromCombo.SelectedIndex = 2;

                var toLabel = new Label { Text = "目标进制:", Location = new Point(225, 85), ForeColor = Color.White, Font = new Font("Segoe UI", 10) };
                var toCombo = new ComboBox { Location = new Point(310, 83), Width = 100, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(60, 60, 60), ForeColor = Color.White, Font = new Font("Segoe UI", 9) };
                toCombo.Items.AddRange(new[] { "二进制", "八进制", "十进制", "十六进制" });
                toCombo.SelectedIndex = 0;

                var resultLabel = new Label { Text = "转换结果:", Location = new Point(20, 130), ForeColor = Color.White, Font = new Font("Segoe UI", 10) };
                var resultBox = new TextBox { Location = new Point(20, 155), Width = 370, Height = 45, Multiline = true, ReadOnly = true, BackColor = Color.FromArgb(50, 50, 50), ForeColor = Color.LightGreen, Font = new Font("Consolas", 12), ScrollBars = ScrollBars.Vertical };

                var convertBtn = new Button { Text = "转换", Location = new Point(170, 215), Width = 85, Height = 32, BackColor = Color.FromArgb(39, 174, 96), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 11) };
                convertBtn.Click += (s, e) => {
                    try
                    {
                        int fromBase = fromCombo.SelectedIndex switch { 0 => 2, 1 => 8, 2 => 10, _ => 16 };
                        int toBase = toCombo.SelectedIndex switch { 0 => 2, 1 => 8, 2 => 10, _ => 16 };
                        
                        long value;
                        try
                        {
                            value = Convert.ToInt64(inputBox.Text, fromBase);
                        }
                        catch
                        {
                            MessageBox.Show("输入无效，请检查输入格式", "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                        
                        string result = Convert.ToString(value, toBase).ToUpper();
                        resultBox.Text = result;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("转换失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                };

                var clearBtn = new Button { Text = "清空", Location = new Point(265, 215), Width = 75, Height = 32, BackColor = Color.FromArgb(70, 70, 70), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10) };
                clearBtn.Click += (s, e) => {
                    inputBox.Text = "";
                    resultBox.Text = "";
                };

                var quickPanel = new Panel { Location = new Point(20, 265), Size = new Size(370, 50), BackColor = Color.FromArgb(50, 50, 50), BorderStyle = BorderStyle.FixedSingle };
                form.Controls.Add(quickPanel);

                var quickLabel = new Label { Text = "快捷转换:", Location = new Point(10, 5), ForeColor = Color.LightGray, Font = new Font("Segoe UI", 8) };
                quickPanel.Controls.Add(quickLabel);

                string[] quickButtons = { "10→2", "10→8", "10→16", "16→10" };
                int[] quickTargets = { 0, 1, 3, 2 };
                for (int i = 0; i < 4; i++)
                {
                    var btn = new Button { Text = quickButtons[i], Location = new Point(75 + i * 85, 20), Size = new Size(65, 22), BackColor = Color.FromArgb(60, 60, 60), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 8) };
                    int target = quickTargets[i];
                    btn.Click += (s, e) => {
                        fromCombo.SelectedIndex = 2;
                        toCombo.SelectedIndex = target;
                        convertBtn.PerformClick();
                    };
                    quickPanel.Controls.Add(btn);
                }

                form.Controls.AddRange(new Control[] { inputLabel, inputBox, fromLabel, fromCombo, toLabel, toCombo, resultLabel, resultBox, convertBtn, clearBtn });
                form.ShowDialog();
            }
        }

        private void ShowEncodingConverter()
        {
            using (var form = new Form())
            {
                form.Text = "编码转换器";
                form.Size = new Size(500, 350);
                form.StartPosition = FormStartPosition.CenterParent;
                form.BackColor = Color.FromArgb(40, 40, 40);
                form.MinimumSize = new Size(500, 350);

                var tabControl = new TabControl { Location = new Point(10, 10), Size = new Size(470, 300), BackColor = Color.FromArgb(40, 40, 40) };
                tabControl.TabPages.Add(CreateEncodingTab("ASCII/Unicode"));
                tabControl.TabPages.Add(CreateEncodingTab("URL编码"));
                tabControl.TabPages.Add(CreateEncodingTab("Base64"));
                form.Controls.Add(tabControl);
                form.ShowDialog();
            }
        }

        private TabPage CreateEncodingTab(string title)
        {
            var tab = new TabPage { Text = title, BackColor = Color.FromArgb(50, 50, 50), ForeColor = Color.White };

            var inputLabel = new Label { Text = "输入:", Location = new Point(10, 15), ForeColor = Color.White, Font = new Font("Segoe UI", 9) };
            var inputBox = new TextBox { Location = new Point(10, 40), Size = new Size(430, 70), Multiline = true, BackColor = Color.FromArgb(60, 60, 60), ForeColor = Color.White, ScrollBars = ScrollBars.Vertical, Font = new Font("Consolas", 10) };

            var outputLabel = new Label { Text = "输出:", Location = new Point(10, 130), ForeColor = Color.White, Font = new Font("Segoe UI", 9) };
            var outputBox = new TextBox { Location = new Point(10, 155), Size = new Size(430, 70), Multiline = true, ReadOnly = true, BackColor = Color.FromArgb(50, 50, 50), ForeColor = Color.LightGreen, Font = new Font("Consolas", 9), ScrollBars = ScrollBars.Vertical };

            var encodeBtn = new Button { Text = "编码 →", Location = new Point(140, 240), Size = new Size(80, 28), BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9) };
            var decodeBtn = new Button { Text = "← 解码", Location = new Point(230, 240), Size = new Size(80, 28), BackColor = Color.FromArgb(155, 89, 182), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9) };
            var copyBtn = new Button { Text = "复制结果", Location = new Point(320, 240), Size = new Size(80, 28), BackColor = Color.FromArgb(70, 70, 70), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9) };

            encodeBtn.Click += (s, e) => {
                try
                {
                    if (title == "ASCII/Unicode")
                        outputBox.Text = string.Join(" ", Encoding.ASCII.GetBytes(inputBox.Text).Select(b => b.ToString("D3")));
                    else if (title == "URL编码")
                        outputBox.Text = Uri.EscapeDataString(inputBox.Text);
                    else if (title == "Base64")
                        outputBox.Text = Convert.ToBase64String(Encoding.UTF8.GetBytes(inputBox.Text));
                }
                catch { outputBox.Text = "编码错误"; }
            };

            decodeBtn.Click += (s, e) => {
                try
                {
                    if (title == "ASCII/Unicode")
                    {
                        var bytes = inputBox.Text.Split(' ')
                            .Where(s => !string.IsNullOrWhiteSpace(s) && byte.TryParse(s, out _))
                            .Select(s => byte.Parse(s)).ToArray();
                        outputBox.Text = Encoding.ASCII.GetString(bytes);
                    }
                    else if (title == "URL编码")
                        outputBox.Text = Uri.UnescapeDataString(inputBox.Text);
                    else if (title == "Base64")
                        outputBox.Text = Encoding.UTF8.GetString(Convert.FromBase64String(inputBox.Text));
                }
                catch { outputBox.Text = "解码错误"; }
            };

            copyBtn.Click += (s, e) => {
                if (!string.IsNullOrEmpty(outputBox.Text))
                {
                    Clipboard.SetText(outputBox.Text);
                }
            };

            tab.Controls.AddRange(new Control[] { inputLabel, inputBox, outputLabel, outputBox, encodeBtn, decodeBtn, copyBtn });
            return tab;
        }

        private void ShowHashCalculator()
        {
            using (var form = new Form())
            {
                form.Text = "哈希计算器";
                form.Size = new Size(500, 350);
                form.StartPosition = FormStartPosition.CenterParent;
                form.BackColor = Color.FromArgb(40, 40, 40);
                form.MinimumSize = new Size(500, 350);

                var inputLabel = new Label { Text = "输入文本:", Location = new Point(15, 20), ForeColor = Color.White, Font = new Font("Segoe UI", 10) };
                var inputBox = new TextBox { Location = new Point(15, 45), Size = new Size(460, 80), Multiline = true, BackColor = Color.FromArgb(60, 60, 60), ForeColor = Color.White, ScrollBars = ScrollBars.Vertical, Font = new Font("Consolas", 10) };

                var hashLabel = new Label { Text = "选择哈希算法:", Location = new Point(15, 140), ForeColor = Color.White, Font = new Font("Segoe UI", 10) };
                var hashCombo = new ComboBox { Location = new Point(120, 138), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList, BackColor = Color.FromArgb(60, 60, 60), ForeColor = Color.White, Font = new Font("Segoe UI", 9) };
                hashCombo.Items.AddRange(new[] { "MD5", "SHA-1", "SHA-256", "SHA-384", "SHA-512" });
                hashCombo.SelectedIndex = 2;

                var resultLabel = new Label { Text = "哈希值:", Location = new Point(15, 185), ForeColor = Color.White, Font = new Font("Segoe UI", 10) };
                var resultBox = new TextBox { Location = new Point(15, 210), Size = new Size(460, 55), Multiline = true, ReadOnly = true, BackColor = Color.FromArgb(50, 50, 50), ForeColor = Color.LightGreen, Font = new Font("Consolas", 10), ScrollBars = ScrollBars.Vertical };

                var calcBtn = new Button { Text = "计算哈希", Location = new Point(180, 280), Size = new Size(75, 30), BackColor = Color.FromArgb(39, 174, 96), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10) };
                var copyBtn = new Button { Text = "复制", Location = new Point(270, 280), Size = new Size(65, 30), BackColor = Color.FromArgb(70, 70, 70), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10) };
                var clearBtn = new Button { Text = "清空", Location = new Point(350, 280), Size = new Size(65, 30), BackColor = Color.FromArgb(70, 70, 70), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10) };

                calcBtn.Click += (s, e) => {
                    try
                    {
                        byte[] bytes = Encoding.UTF8.GetBytes(inputBox.Text);
                        HashAlgorithm hash = hashCombo.SelectedIndex switch {
                            0 => MD5.Create(),
                            1 => SHA1.Create(),
                            2 => SHA256.Create(),
                            3 => SHA384.Create(),
                            _ => SHA512.Create()
                        };
                        byte[] hashBytes = hash.ComputeHash(bytes);
                        resultBox.Text = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                    }
                    catch { resultBox.Text = "计算错误"; }
                };

                copyBtn.Click += (s, e) => {
                    if (!string.IsNullOrEmpty(resultBox.Text))
                    {
                        Clipboard.SetText(resultBox.Text);
                    }
                };

                clearBtn.Click += (s, e) => {
                    inputBox.Text = "";
                    resultBox.Text = "";
                };

                form.Controls.AddRange(new Control[] { inputLabel, inputBox, hashLabel, hashCombo, resultLabel, resultBox, calcBtn, copyBtn, clearBtn });
                form.ShowDialog();
            }
        }

        private void ShowCRCCalculator()
        {
            using (var form = new Form())
            {
                form.Text = "CRC32 校验计算器";
                form.Size = new Size(500, 320);
                form.StartPosition = FormStartPosition.CenterParent;
                form.BackColor = Color.FromArgb(40, 40, 40);
                form.MinimumSize = new Size(500, 320);

                var inputLabel = new Label { Text = "输入数据:", Location = new Point(15, 20), ForeColor = Color.White, Font = new Font("Segoe UI", 10) };
                var inputBox = new TextBox { Location = new Point(15, 45), Size = new Size(460, 70), Multiline = true, BackColor = Color.FromArgb(60, 60, 60), ForeColor = Color.White, Font = new Font("Consolas", 10) };

                var formatGroup = new GroupBox { Text = "输入格式:", Location = new Point(15, 130), Size = new Size(220, 55), BackColor = Color.FromArgb(50, 50, 50), ForeColor = Color.White, Font = new Font("Segoe UI", 9) };
                var textRadio = new RadioButton { Text = "文本", Location = new Point(15, 25), Checked = true, BackColor = Color.FromArgb(50, 50, 50), ForeColor = Color.White };
                var hexRadio = new RadioButton { Text = "十六进制", Location = new Point(115, 25), BackColor = Color.FromArgb(50, 50, 50), ForeColor = Color.White };
                formatGroup.Controls.AddRange(new Control[] { textRadio, hexRadio });

                var resultLabel = new Label { Text = "CRC32 校验值:", Location = new Point(15, 200), ForeColor = Color.White, Font = new Font("Segoe UI", 10) };
                var resultBox = new TextBox { Location = new Point(15, 225), Size = new Size(460, 35), ReadOnly = true, BackColor = Color.FromArgb(50, 50, 50), ForeColor = Color.LightGreen, Font = new Font("Consolas", 12) };

                var calcBtn = new Button { Text = "计算 CRC32", Location = new Point(180, 270), Size = new Size(85, 30), BackColor = Color.FromArgb(39, 174, 96), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10) };
                var copyBtn = new Button { Text = "复制", Location = new Point(280, 270), Size = new Size(65, 30), BackColor = Color.FromArgb(70, 70, 70), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10) };

                calcBtn.Click += (s, e) => {
                    try
                    {
                        byte[] data;
                        if (hexRadio.Checked)
                        {
                            string hex = inputBox.Text.Replace("0x", "").Replace(" ", "").Replace("-", "");
                            if (hex.Length % 2 != 0)
                            {
                                resultBox.Text = "错误: 十六进制输入必须是偶数位";
                                return;
                            }
                            data = Enumerable.Range(0, hex.Length / 2).Select(i => Convert.ToByte(hex.Substring(i * 2, 2), 16)).ToArray();
                        }
                        else
                        {
                            data = Encoding.UTF8.GetBytes(inputBox.Text);
                        }
                        uint crc = CalculateCRC32(data);
                        resultBox.Text = $"0x{crc.ToString("X8")}  ({crc.ToString("D10")})";
                    }
                    catch (Exception ex)
                    {
                        resultBox.Text = "计算错误: " + ex.Message;
                    }
                };

                copyBtn.Click += (s, e) => {
                    if (!string.IsNullOrEmpty(resultBox.Text))
                    {
                        int hexStart = resultBox.Text.IndexOf("0x");
                        if (hexStart >= 0)
                        {
                            Clipboard.SetText(resultBox.Text.Substring(hexStart, 10));
                        }
                    }
                };

                form.Controls.AddRange(new Control[] { inputLabel, inputBox, formatGroup, resultLabel, resultBox, calcBtn, copyBtn });
                form.ShowDialog();
            }
        }

        private uint CalculateCRC32(byte[] data)
        {
            uint[] table = new uint[256];
            const uint polynomial = 0xEDB88320;

            for (uint i = 0; i < 256; i++)
            {
                uint crc = i;
                for (int j = 0; j < 8; j++)
                {
                    crc = (crc & 1) == 1 ? (crc >> 1) ^ polynomial : crc >> 1;
                }
                table[i] = crc;
            }

            uint crc32 = 0xFFFFFFFF;
            foreach (byte b in data)
            {
                crc32 = table[(crc32 ^ b) & 0xFF] ^ (crc32 >> 8);
            }
            return crc32 ^ 0xFFFFFFFF;
        }

        private void ShowCalculusTool()
        {
            using (var form = new Form())
            {
                form.Text = "微积分计算器";
                form.Size = new Size(500, 400);
                form.StartPosition = FormStartPosition.CenterParent;
                form.BackColor = Color.FromArgb(40, 40, 40);
                form.MinimumSize = new Size(500, 400);

                var tabControl = new TabControl { Location = new Point(10, 10), Size = new Size(470, 350), BackColor = Color.FromArgb(40, 40, 40) };

                var derivativeTab = new TabPage { Text = "导数计算", BackColor = Color.FromArgb(50, 50, 50), ForeColor = Color.White };
                var integralTab = new TabPage { Text = "积分计算", BackColor = Color.FromArgb(50, 50, 50), ForeColor = Color.White };

                var funcLabel = new Label { Text = "输入函数 f(x):", Location = new Point(15, 20), ForeColor = Color.White, Font = new Font("Segoe UI", 10) };
                var funcBox = new TextBox { Location = new Point(15, 45), Size = new Size(430, 35), BackColor = Color.FromArgb(60, 60, 60), ForeColor = Color.White, Font = new Font("Consolas", 12) };

                var pointLabel = new Label { Text = "计算点 x =", Location = new Point(15, 95), ForeColor = Color.White, Font = new Font("Segoe UI", 10) };
                var pointBox = new TextBox { Location = new Point(110, 93), Size = new Size(80, 25), BackColor = Color.FromArgb(60, 60, 60), ForeColor = Color.White, Font = new Font("Consolas", 11), Text = "1" };

                var resultLabel = new Label { Text = "结果:", Location = new Point(15, 140), ForeColor = Color.White, Font = new Font("Segoe UI", 10) };
                var resultBox = new TextBox { Location = new Point(15, 165), Size = new Size(430, 80), Multiline = true, ReadOnly = true, BackColor = Color.FromArgb(50, 50, 50), ForeColor = Color.LightGreen, Font = new Font("Consolas", 10), ScrollBars = ScrollBars.Vertical };

                var calcBtn = new Button { Text = "计算导数", Location = new Point(190, 260), Size = new Size(90, 30), BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10) };
                calcBtn.Click += (s, e) => {
                    try
                    {
                        if (!double.TryParse(pointBox.Text, out double x))
                        {
                            resultBox.Text = "错误: x 值无效";
                            return;
                        }

                        double h = 1e-8;
                        double fx = EvaluateSimpleFunction(funcBox.Text, x);
                        double fxh = EvaluateSimpleFunction(funcBox.Text, x + h);
                        double derivative = (fxh - fx) / h;

                        resultBox.Text = $"f({x}) = {fx:F6}\n" +
                                        $"f'({x}) ≈ {derivative:F6}\n\n" +
                                        $"使用数值微分法\n步长 h = {h}";
                    }
                    catch (Exception ex)
                    {
                        resultBox.Text = "计算错误: " + ex.Message;
                    }
                };

                derivativeTab.Controls.AddRange(new Control[] { funcLabel, funcBox, pointLabel, pointBox, resultLabel, resultBox, calcBtn });

                var intFuncLabel = new Label { Text = "输入函数 f(x):", Location = new Point(15, 20), ForeColor = Color.White, Font = new Font("Segoe UI", 10) };
                var intFuncBox = new TextBox { Location = new Point(15, 45), Size = new Size(430, 35), BackColor = Color.FromArgb(60, 60, 60), ForeColor = Color.White, Font = new Font("Consolas", 12) };

                var aLabel = new Label { Text = "下限 a:", Location = new Point(15, 95), ForeColor = Color.White, Font = new Font("Segoe UI", 10) };
                var aBox = new TextBox { Location = new Point(70, 93), Size = new Size(70, 25), BackColor = Color.FromArgb(60, 60, 60), ForeColor = Color.White, Font = new Font("Consolas", 11), Text = "0" };

                var bLabel = new Label { Text = "上限 b:", Location = new Point(155, 95), ForeColor = Color.White, Font = new Font("Segoe UI", 10) };
                var bBox = new TextBox { Location = new Point(210, 93), Size = new Size(70, 25), BackColor = Color.FromArgb(60, 60, 60), ForeColor = Color.White, Font = new Font("Consolas", 11), Text = "1" };

                var nLabel = new Label { Text = "分割数:", Location = new Point(295, 95), ForeColor = Color.White, Font = new Font("Segoe UI", 10) };
                var nBox = new TextBox { Location = new Point(360, 93), Size = new Size(70, 25), BackColor = Color.FromArgb(60, 60, 60), ForeColor = Color.White, Font = new Font("Consolas", 11), Text = "1000" };

                var intResultBox = new TextBox { Location = new Point(15, 140), Size = new Size(430, 100), Multiline = true, ReadOnly = true, BackColor = Color.FromArgb(50, 50, 50), ForeColor = Color.LightGreen, Font = new Font("Consolas", 10), ScrollBars = ScrollBars.Vertical };

                var intCalcBtn = new Button { Text = "数值积分", Location = new Point(190, 255), Size = new Size(90, 30), BackColor = Color.FromArgb(155, 89, 182), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10) };

                intCalcBtn.Click += (s, e) => {
                    try
                    {
                        if (!double.TryParse(aBox.Text, out double a) ||
                            !double.TryParse(bBox.Text, out double b) ||
                            !int.TryParse(nBox.Text, out int n))
                        {
                            intResultBox.Text = "错误: 输入格式无效";
                            return;
                        }

                        if (n <= 0)
                        {
                            intResultBox.Text = "错误: 分割数必须大于0";
                            return;
                        }

                        double h = (b - a) / n;
                        double sum = 0;
                        for (int i = 1; i < n; i++)
                        {
                            double x = a + i * h;
                            sum += EvaluateSimpleFunction(intFuncBox.Text, x);
                        }
                        double integral = (h / 2) * (EvaluateSimpleFunction(intFuncBox.Text, a) + EvaluateSimpleFunction(intFuncBox.Text, b) + 2 * sum);

                        intResultBox.Text = $"∫f(x)dx 从 [{a}, {b}]\n\n" +
                                          $"梯形法结果: {integral:F10}\n\n" +
                                          $"分割数 n = {n}\n" +
                                          $"步长 h = {h:F10}\n" +
                                          $"计算耗时: 快速估算";
                    }
                    catch (Exception ex)
                    {
                        intResultBox.Text = "计算错误: " + ex.Message;
                    }
                };

                integralTab.Controls.AddRange(new Control[] { intFuncLabel, intFuncBox, aLabel, aBox, bLabel, bBox, nLabel, nBox, intResultBox, intCalcBtn });

                tabControl.TabPages.Add(derivativeTab);
                tabControl.TabPages.Add(integralTab);
                form.Controls.Add(tabControl);
                form.ShowDialog();
            }
        }

        private double EvaluateSimpleFunction(string expr, double x)
        {
            expr = expr.Replace("x", x.ToString());
            expr = expr.Replace("^", "**");
            try
            {
                var dt = new System.Data.DataTable();
                var result = dt.Compute(expr, "");
                return Convert.ToDouble(result);
            }
            catch
            {
                return 0;
            }
        }

        private void ScientificCalculatorForm_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.KeyCode >= Keys.NumPad0 && e.KeyCode <= Keys.NumPad9)
                {
                    AppendInput((e.KeyCode - Keys.NumPad0).ToString());
                    e.SuppressKeyPress = true;
                }
                else if (e.KeyCode == Keys.Add)
                {
                    AppendInput("+");
                    e.SuppressKeyPress = true;
                }
                else if (e.KeyCode == Keys.Subtract)
                {
                    AppendInput("-");
                    e.SuppressKeyPress = true;
                }
                else if (e.KeyCode == Keys.Multiply)
                {
                    AppendInput("*");
                    e.SuppressKeyPress = true;
                }
                else if (e.KeyCode == Keys.Divide)
                {
                    AppendInput("/");
                    e.SuppressKeyPress = true;
                }
                else if (e.KeyCode == Keys.Decimal)
                {
                    AppendInput(".");
                    e.SuppressKeyPress = true;
                }
                else if (e.KeyCode == Keys.Enter)
                {
                    Calculate();
                    e.SuppressKeyPress = true;
                }
                else if (e.KeyCode == Keys.Escape)
                {
                    ClearAll();
                    e.SuppressKeyPress = true;
                }
                else if (e.KeyCode == Keys.Back)
                {
                    Backspace();
                    e.SuppressKeyPress = true;
                }
            }
            catch
            {
            }
        }
    }
}
