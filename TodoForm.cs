using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace BusyBox
{
    public class TodoItem
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public bool Completed { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class TodoForm : Form
    {
        private List<TodoItem> _items = new List<TodoItem>();
        private ListBox _listBox;
        private TextBox _inputBox;
        private ToolStrip _toolbar;
        private Button _closeButton;
        private bool _isFloating = false;
        private bool _floatLeft = true;

        public TodoForm()
        {
            InitializeComponent();
            LoadItems();
        }

        private void InitializeComponent()
        {
            Text = "待办事项";
            Size = new Size(350, 500);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(30, 30, 30);
            MinimumSize = new Size(300, 400);
            KeyPreview = true;

            _listBox = new ListBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.None,
                SelectionMode = SelectionMode.One,
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 28
            };
            _listBox.DrawItem += ListBox_DrawItem;
            _listBox.DoubleClick += ListBox_DoubleClick;
            _listBox.KeyDown += ListBox_KeyDown;
            Controls.Add(_listBox);

            _toolbar = new ToolStrip
            {
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.White,
                GripStyle = ToolStripGripStyle.Hidden,
                Dock = DockStyle.Bottom
            };

            var addButton = new ToolStripButton("添加");
            addButton.Click += AddButton_Click;
            _toolbar.Items.Add(addButton);

            var completeButton = new ToolStripButton("完成");
            completeButton.Click += CompleteButton_Click;
            _toolbar.Items.Add(completeButton);

            var deleteButton = new ToolStripButton("删除");
            deleteButton.Click += DeleteButton_Click;
            _toolbar.Items.Add(deleteButton);

            var editButton = new ToolStripButton("编辑");
            editButton.Click += EditButton_Click;
            _toolbar.Items.Add(editButton);

            _toolbar.Items.Add(new ToolStripSeparator());

            var floatLeftButton = new ToolStripButton("悬浮左侧");
            floatLeftButton.Click += FloatLeftButton_Click;
            _toolbar.Items.Add(floatLeftButton);

            var floatRightButton = new ToolStripButton("悬浮右侧");
            floatRightButton.Click += FloatRightButton_Click;
            _toolbar.Items.Add(floatRightButton);

            var normalButton = new ToolStripButton("正常模式");
            normalButton.Click += NormalButton_Click;
            _toolbar.Items.Add(normalButton);

            _toolbar.Items.Add(new ToolStripSeparator());

            var saveJsonButton = new ToolStripButton("保存JSON");
            saveJsonButton.Click += SaveJsonButton_Click;
            _toolbar.Items.Add(saveJsonButton);

            var saveCsvButton = new ToolStripButton("保存CSV");
            saveCsvButton.Click += SaveCsvButton_Click;
            _toolbar.Items.Add(saveCsvButton);

            Controls.Add(_toolbar);

            _inputBox = new TextBox
            {
                Dock = DockStyle.Bottom,
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11),
                BorderStyle = BorderStyle.FixedSingle,
                PlaceholderText = "输入待办事项..."
            };
            _inputBox.KeyDown += InputBox_KeyDown;
            Controls.Add(_inputBox);

            _closeButton = new Button
            {
                Text = "✕",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Size = new Size(30, 30),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Visible = false
            };
            _closeButton.Click += CloseButton_Click;
            Controls.Add(_closeButton);

            KeyDown += TodoForm_KeyDown;
            _inputBox.Focus();
        }

        private void TodoForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape && _isFloating)
            {
                SetNormalMode();
            }
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= _items.Count) return;

            e.DrawBackground();

            var item = _items[e.Index];
            var text = item.Text;
            var fontStyle = item.Completed ? FontStyle.Strikeout : FontStyle.Regular;

            using (var font = new Font("Segoe UI", 11, fontStyle))
            {
                var color = item.Completed ? Color.Gray : Color.White;
                
                e.Graphics.DrawString(
                    item.Completed ? "[✓] " + text : "[ ] " + text,
                    font,
                    new SolidBrush(color),
                    e.Bounds.Left + 8,
                    e.Bounds.Top + 4
                );
            }

            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            {
                using (var pen = new Pen(Color.FromArgb(52, 152, 219), 2))
                {
                    e.Graphics.DrawRectangle(pen, e.Bounds.Left, e.Bounds.Top, e.Bounds.Width - 1, e.Bounds.Height - 1);
                }
            }
        }

        private void AddItem(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            _items.Add(new TodoItem
            {
                Id = Guid.NewGuid().ToString(),
                Text = text.Trim(),
                Completed = false,
                CreatedAt = DateTime.Now
            });

            RefreshList();
            _inputBox.Clear();
            SaveItems();
        }

        private void ToggleComplete(int index)
        {
            if (index >= 0 && index < _items.Count)
            {
                _items[index].Completed = !_items[index].Completed;
                RefreshList();
                SaveItems();
            }
        }

        private void DeleteItem(int index)
        {
            if (index >= 0 && index < _items.Count)
            {
                _items.RemoveAt(index);
                RefreshList();
                SaveItems();
            }
        }

        private void EditItem(int index)
        {
            if (index >= 0 && index < _items.Count)
            {
                var item = _items[index];
                var result = Microsoft.VisualBasic.Interaction.InputBox("编辑待办事项:", "编辑", item.Text);
                
                if (!string.IsNullOrWhiteSpace(result) && result != item.Text)
                {
                    item.Text = result.Trim();
                    RefreshList();
                    SaveItems();
                }
            }
        }

        private void RefreshList()
        {
            _listBox.Items.Clear();
            foreach (var item in _items)
            {
                _listBox.Items.Add(item);
            }
            _listBox.Invalidate();
        }

        private void LoadItems()
        {
            try
            {
                var path = Path.Combine(Application.StartupPath, "data", "todo.json");
                if (File.Exists(path))
                {
                    var json = File.ReadAllText(path);
                    _items = JsonConvert.DeserializeObject<List<TodoItem>>(json) ?? new List<TodoItem>();
                    RefreshList();
                }
            }
            catch { }
        }

        private void SaveItems()
        {
            try
            {
                var dataDir = Path.Combine(Application.StartupPath, "data");
                if (!Directory.Exists(dataDir)) Directory.CreateDirectory(dataDir);
                
                var path = Path.Combine(dataDir, "todo.json");
                var json = JsonConvert.SerializeObject(_items, Formatting.Indented);
                File.WriteAllText(path, json);
            }
            catch { }
        }

        private void SetFloatingPosition(bool left)
        {
            _isFloating = true;
            _floatLeft = left;

            FormBorderStyle = FormBorderStyle.None;
            TopMost = true;
            ShowInTaskbar = false;

            _closeButton.Visible = true;
            _closeButton.Location = new Point(Width - 35, 5);

            if (left)
            {
                Location = new Point(0, Screen.PrimaryScreen.WorkingArea.Height / 2 - Height / 2);
            }
            else
            {
                Location = new Point(Screen.PrimaryScreen.WorkingArea.Width - Width, 
                    Screen.PrimaryScreen.WorkingArea.Height / 2 - Height / 2);
            }
        }

        private void SetNormalMode()
        {
            _isFloating = false;
            FormBorderStyle = FormBorderStyle.Sizable;
            TopMost = false;
            ShowInTaskbar = true;
            _closeButton.Visible = false;
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            AddItem(_inputBox.Text);
        }

        private void CompleteButton_Click(object sender, EventArgs e)
        {
            ToggleComplete(_listBox.SelectedIndex);
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            if (_listBox.SelectedIndex >= 0)
            {
                if (MessageBox.Show("确定要删除此待办事项吗？", "确认删除", 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    DeleteItem(_listBox.SelectedIndex);
                }
            }
        }

        private void EditButton_Click(object sender, EventArgs e)
        {
            EditItem(_listBox.SelectedIndex);
        }

        private void FloatLeftButton_Click(object sender, EventArgs e)
        {
            SetFloatingPosition(true);
        }

        private void FloatRightButton_Click(object sender, EventArgs e)
        {
            SetFloatingPosition(false);
        }

        private void NormalButton_Click(object sender, EventArgs e)
        {
            SetNormalMode();
        }

        private void SaveJsonButton_Click(object sender, EventArgs e)
        {
            using (var saveDialog = new SaveFileDialog
            {
                Filter = "JSON文件|*.json",
                Title = "保存为JSON"
            })
            {
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    var json = JsonConvert.SerializeObject(_items, Formatting.Indented);
                    File.WriteAllText(saveDialog.FileName, json);
                    MessageBox.Show("保存成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void SaveCsvButton_Click(object sender, EventArgs e)
        {
            using (var saveDialog = new SaveFileDialog
            {
                Filter = "CSV文件|*.csv",
                Title = "保存为CSV"
            })
            {
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    var csv = new System.Text.StringBuilder();
                    csv.AppendLine("ID,内容,完成状态,创建时间");
                    
                    foreach (var item in _items)
                    {
                        csv.AppendLine($"{item.Id},\"{item.Text.Replace("\"", "\"\"")}\",{(item.Completed ? "已完成" : "未完成")},{item.CreatedAt:yyyy-MM-dd HH:mm:ss}");
                    }

                    File.WriteAllText(saveDialog.FileName, csv.ToString(), System.Text.Encoding.UTF8);
                    MessageBox.Show("保存成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                AddItem(_inputBox.Text);
                e.SuppressKeyPress = true;
            }
        }

        private void ListBox_DoubleClick(object sender, EventArgs e)
        {
            ToggleComplete(_listBox.SelectedIndex);
        }

        private void ListBox_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                    ToggleComplete(_listBox.SelectedIndex);
                    break;
                case Keys.Delete:
                    DeleteItem(_listBox.SelectedIndex);
                    break;
                case Keys.F2:
                    EditItem(_listBox.SelectedIndex);
                    break;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            SaveItems();
            base.OnFormClosing(e);
        }
    }
}
