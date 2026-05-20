using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace BusyBox
{
    public class NotesForm : Form
    {
        private TextBox _editor;
        private WebBrowser _preview;
        private ToolStrip _toolbar;
        private StatusStrip _statusBar;
        private ToolStripStatusLabel _statusLabel;
        private string _currentFile = null;
        private bool _previewVisible = false;

        public NotesForm()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                MessageBox.Show("错误: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (!IsDisposed) Close();
            }
        }

        private void InitializeComponent()
        {
            Text = "快速便签";
            Size = new Size(1000, 650);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(30, 30, 30);
            MinimumSize = new Size(800, 500);

            _toolbar = new ToolStrip
            {
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.White,
                GripStyle = ToolStripGripStyle.Hidden,
                Dock = DockStyle.Right
            };
            _toolbar.Items.Add(new ToolStripLabel("操作"));
            _toolbar.Items.Add(new ToolStripSeparator());

            var newButton = new ToolStripButton("新建");
            newButton.Click += NewButton_Click;
            _toolbar.Items.Add(newButton);

            var openButton = new ToolStripButton("打开");
            openButton.Click += OpenButton_Click;
            _toolbar.Items.Add(openButton);

            var saveButton = new ToolStripButton("保存");
            saveButton.Click += SaveButton_Click;
            _toolbar.Items.Add(saveButton);

            _toolbar.Items.Add(new ToolStripSeparator());

            var saveMdButton = new ToolStripButton("保存为MD");
            saveMdButton.Click += SaveMdButton_Click;
            _toolbar.Items.Add(saveMdButton);

            var saveTxtButton = new ToolStripButton("保存为TXT");
            saveTxtButton.Click += SaveTxtButton_Click;
            _toolbar.Items.Add(saveTxtButton);

            var saveHtmlButton = new ToolStripButton("保存为HTML");
            saveHtmlButton.Click += SaveHtmlButton_Click;
            _toolbar.Items.Add(saveHtmlButton);

            _toolbar.Items.Add(new ToolStripSeparator());

            var previewButton = new ToolStripButton("预览");
            previewButton.Click += PreviewButton_Click;
            _toolbar.Items.Add(previewButton);

            Controls.Add(_toolbar);

            _statusBar = new StatusStrip
            {
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.White,
                Dock = DockStyle.Bottom
            };

            _statusLabel = new ToolStripStatusLabel { Text = "就绪 | 字符: 0 | 行: 1" };
            _statusBar.Items.Add(_statusLabel);
            Controls.Add(_statusBar);

            _editor = new TextBox
            {
                Multiline = true,
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.White,
                Font = new Font("Consolas", 11),
                BorderStyle = BorderStyle.FixedSingle,
                ScrollBars = ScrollBars.Both,
                WordWrap = false,
                AcceptsReturn = true,
                AcceptsTab = true,
                ReadOnly = false,
                Dock = DockStyle.Fill
            };
            _editor.TextChanged += Editor_TextChanged;
            Controls.Add(_editor);

            _preview = new WebBrowser
            {
                ScriptErrorsSuppressed = true,
                Visible = false,
                Dock = DockStyle.Fill
            };
            Controls.Add(_preview);

            _editor.Text = "";
            _editor.Focus();
        }

        private void Editor_TextChanged(object sender, EventArgs e)
        {
            try
            {
                int charCount = _editor.Text.Length;
                int lineCount = _editor.Text.Split('\n').Length;
                _statusLabel.Text = $"就绪 | 字符: {charCount} | 行: {lineCount}";

                if (_previewVisible)
                {
                    UpdatePreview();
                }
            }
            catch { }
        }

        private void PreviewButton_Click(object sender, EventArgs e)
        {
            try
            {
                _previewVisible = !_previewVisible;

                if (_previewVisible)
                {
                    _editor.Dock = DockStyle.Left;
                    _editor.Width = ClientSize.Width / 2;
                    _preview.Visible = true;
                    _preview.Dock = DockStyle.Fill;
                    UpdatePreview();
                }
                else
                {
                    _preview.Visible = false;
                    _editor.Dock = DockStyle.Fill;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("预览失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdatePreview()
        {
            try
            {
                string html = ConvertToHtml(_editor.Text);
                _preview.DocumentText = html;
            }
            catch { }
        }

        private string ConvertToHtml(string markdown)
        {
            if (string.IsNullOrWhiteSpace(markdown))
                return "<html><body style='background:#2d2d2d;color:#f0f0f0;padding:20px;'></body></html>";

            StringBuilder html = new StringBuilder();
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html><head><meta charset='utf-8'>");
            html.AppendLine("<style>");
            html.AppendLine("body { font-family: Segoe UI, Arial, sans-serif; line-height: 1.6; padding: 20px; color: #f0f0f0; background: #2d2d2d; }");
            html.AppendLine("h1 { font-size: 2em; color: #4ec9b0; border-bottom: 2px solid #4ec9b0; padding-bottom: 5px; margin-top: 20px; }");
            html.AppendLine("h2 { font-size: 1.5em; color: #4ec9b0; border-bottom: 1px solid #444; padding-bottom: 3px; margin-top: 18px; }");
            html.AppendLine("h3 { font-size: 1.25em; color: #4ec9b0; margin-top: 16px; }");
            html.AppendLine("strong { color: #ce9178; font-weight: bold; }");
            html.AppendLine("em { color: #ce9178; font-style: italic; }");
            html.AppendLine("code { background: #1e1e1e; padding: 2px 6px; border-radius: 3px; font-family: Consolas, monospace; color: #dcdcaa; }");
            html.AppendLine("pre { background: #1e1e1e; padding: 15px; border-radius: 5px; overflow-x: auto; border-left: 4px solid #4ec9b0; }");
            html.AppendLine("pre code { background: transparent; padding: 0; }");
            html.AppendLine("ul { list-style-type: disc; padding-left: 30px; margin: 10px 0; }");
            html.AppendLine("ol { list-style-type: decimal; padding-left: 30px; margin: 10px 0; }");
            html.AppendLine("li { margin: 5px 0; }");
            html.AppendLine("a { color: #4ec9b0; text-decoration: none; }");
            html.AppendLine("a:hover { text-decoration: underline; }");
            html.AppendLine("p { margin: 10px 0; }");
            html.AppendLine("hr { border: none; border-top: 1px solid #555; margin: 20px 0; }");
            html.AppendLine("blockquote { border-left: 4px solid #dcdcaa; padding-left: 15px; margin: 10px 0; color: #888; }");
            html.AppendLine("table { border-collapse: collapse; width: 100%; margin: 15px 0; }");
            html.AppendLine("th, td { border: 1px solid #555; padding: 10px; text-align: left; }");
            html.AppendLine("th { background: #1e1e1e; color: #4ec9b0; }");
            html.AppendLine("tr:nth-child(even) { background: #333; }");
            html.AppendLine("</style></head><body>");

            string[] lines = markdown.Split('\n');
            bool inCode = false;
            bool inList = false;

            foreach (string line in lines)
            {
                string trimmed = line.Trim();

                if (trimmed.StartsWith("```"))
                {
                    if (inList) { html.AppendLine("</ul>"); inList = false; }
                    inCode = !inCode;
                    html.AppendLine(inCode ? "<pre><code>" : "</code></pre>");
                    continue;
                }

                if (inCode)
                {
                    html.AppendLine(EscapeHtml(line));
                    continue;
                }

                if (trimmed.StartsWith("#"))
                {
                    if (inList) { html.AppendLine("</ul>"); inList = false; }
                    int level = 0;
                    while (level < trimmed.Length && trimmed[level] == '#') level++;
                    string content = trimmed.Substring(level).Trim();
                    html.AppendLine($"<h{Math.Min(level, 6)}>{ProcessInline(content)}</h{Math.Min(level, 6)}>");
                }
                else if (trimmed.StartsWith("- ") || trimmed.StartsWith("* "))
                {
                    if (!inList)
                    {
                        html.AppendLine("<ul>");
                        inList = true;
                    }
                    string content = trimmed.Substring(2);
                    html.AppendLine($"<li>{ProcessInline(content)}</li>");
                }
                else if (Regex.IsMatch(trimmed, @"^\d+\.\s"))
                {
                    if (!inList)
                    {
                        html.AppendLine("<ol>");
                        inList = true;
                    }
                    string content = Regex.Replace(trimmed, @"^\d+\.\s", "");
                    html.AppendLine($"<li>{ProcessInline(content)}</li>");
                }
                else if (trimmed.StartsWith(">"))
                {
                    if (inList) { html.AppendLine("</ul>"); inList = false; }
                    html.AppendLine($"<blockquote><p>{ProcessInline(trimmed.Substring(1).Trim())}</p></blockquote>");
                }
                else if (trimmed == "---" || trimmed == "***" || trimmed == "___")
                {
                    if (inList) { html.AppendLine("</ul>"); inList = false; }
                    html.AppendLine("<hr/>");
                }
                else
                {
                    if (inList) { html.AppendLine("</ul>"); inList = false; }
                    if (!string.IsNullOrWhiteSpace(trimmed))
                    {
                        html.AppendLine($"<p>{ProcessInline(trimmed)}</p>");
                    }
                }
            }

            if (inList) html.AppendLine("</ul>");

            html.AppendLine("</body></html>");
            return html.ToString();
        }

        private string EscapeHtml(string text)
        {
            return text.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
        }

        private string ProcessInline(string text)
        {
            text = Regex.Replace(text, @"\*\*(.+?)\*\*", "<strong>$1</strong>");
            text = Regex.Replace(text, @"\*(.+?)\*", "<em>$1</em>");
            text = Regex.Replace(text, @"~~(.+?)~~", "<del>$1</del>");
            text = Regex.Replace(text, @"`(.+?)`", "<code>$1</code>");
            text = Regex.Replace(text, @"\[([^\]]+)\]\(([^)]+)\)", "<a href=\"$2\" target=\"_blank\">$1</a>");
            return text;
        }

        private void NewButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(_editor.Text))
                {
                    var result = MessageBox.Show("是否保存当前内容？", "新建文件",
                        MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                        SaveFile();
                    else if (result == DialogResult.Cancel)
                        return;
                }

                _editor.Text = "";
                _currentFile = null;
                Text = "快速便签 [未命名]";
            }
            catch (Exception ex)
            {
                MessageBox.Show("操作失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (var openDialog = new OpenFileDialog
                {
                    Filter = "所有文件|*.*",
                    Title = "打开文件"
                })
                {
                    if (openDialog.ShowDialog() == DialogResult.OK)
                    {
                        _editor.Text = File.ReadAllText(openDialog.FileName, Encoding.UTF8);
                        _currentFile = openDialog.FileName;
                        Text = "快速便签 - " + Path.GetFileName(openDialog.FileName);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("打开失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFile();
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveMdButton_Click(object sender, EventArgs e)
        {
            try
            {
                SaveAs("Markdown文件|*.md", "md");
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveTxtButton_Click(object sender, EventArgs e)
        {
            try
            {
                SaveAs("文本文件|*.txt", "txt");
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveHtmlButton_Click(object sender, EventArgs e)
        {
            try
            {
                SaveAs("HTML文件|*.html", "html");
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存失败: " + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveFile()
        {
            if (_currentFile != null)
            {
                File.WriteAllText(_currentFile, _editor.Text, Encoding.UTF8);
                MessageBox.Show("文件已保存！", "保存成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                SaveAs("Markdown文件|*.md|文本文件|*.txt|所有文件|*.*", "md");
            }
        }

        private void SaveAs(string filter, string defaultExt)
        {
            using (var saveDialog = new SaveFileDialog
            {
                Filter = filter,
                Title = "另存为",
                DefaultExt = defaultExt
            })
            {
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    string content = _editor.Text;
                    if (defaultExt == "html")
                    {
                        content = ConvertToHtml(_editor.Text);
                    }

                    File.WriteAllText(saveDialog.FileName, content, Encoding.UTF8);
                    _currentFile = saveDialog.FileName;
                    Text = "快速便签 - " + Path.GetFileName(saveDialog.FileName);
                    MessageBox.Show("文件已保存！", "保存成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
    }
}
