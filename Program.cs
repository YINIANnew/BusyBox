using System;
using System.Drawing;
using System.Windows.Forms;
using System.Media;
using System.IO;
using System.Collections.Generic;
using BusyBox.Themes;
using NAudio.Wave;

namespace BusyBox
{
    public class ProjectItem
    {
        public string Name { get; set; }
        public string DisplayText { get; set; }
        public string Duration { get; set; }
        public string Type { get; set; }
    }

    public class MainForm : Form
    {
        private Label _titleLabel;
        private Label _currentTimeLabel;
        private Panel _timePanel;
        private List<PictureBox> _numberPictures;
        private Panel _separatorPanel;
        private Label _progressLabel;
        private ProgressBar _progressBar;
        private Button _startButton;
        private Button _settingsButton;
        private Button _themeButton;
        private Button _musicButton;
        private Button _toolsButton;
        private Button _infoButton;
        private Button _exitButton;
        private Timer _timer;
        private Timer _fullscreenTimer;
        private Timer _batteryTimer;
        private int _secondsRemaining;
        private int _totalSeconds;
        private bool _isRunning;
        private bool _isPomodoro;
        private bool _isFullscreen;
        private DateTime _lastActivityTime;
        private List<string> _musicFiles;
        private string _musicFolder;
        private bool _useThemeNumbers;
        private string _settingsPath;
        private List<ProjectItem> _workflowItems;
        private int _currentWorkflowIndex;
        private Label _projectNameLabel;

        public MainForm()
        {
            try
            {
                _settingsPath = Path.Combine(Application.StartupPath, "data", "settings.txt");
                InitializeComponents();
                LoadSettings();
                LoadMusicFiles();
                LoadLastTheme();
                ApplyTheme();
                
                Show();
                Application.DoEvents();
                
                var startupTimer = new Timer { Interval = 500 };
                startupTimer.Tick += (s, e) =>
                {
                    startupTimer.Stop();
                    startupTimer.Dispose();
                    try
                    {
                        StartFullscreen();
                    }
                    catch { }
                };
                startupTimer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"程序启动时出错: {ex.Message}\n\n详细信息: {ex}", 
                    "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void InitializeComponents()
        {
            Text = "BusyBox - Pomodoro Timer";
            Size = new Size(800, 600);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.None;
            BackColor = Color.FromArgb(30, 30, 30);
            DoubleBuffered = true;
            KeyPreview = true;
            AutoScaleMode = AutoScaleMode.None;

            _timer = new Timer { Interval = 1000 };
            _timer.Tick += Timer_Tick;

            _fullscreenTimer = new Timer { Interval = 1000 };
            _fullscreenTimer.Tick += FullscreenTimer_Tick;

            _batteryTimer = new Timer { Interval = 300000 }; 
            _batteryTimer.Tick += BatteryTimer_Tick;

            _titleLabel = new Label
            {
                Text = "BusyBox番茄工作法",
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                ForeColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Height = 50,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent,
                Padding = new Padding(5)
            };
            _titleLabel.SetBounds(0, 60, 800, 50);

            _currentTimeLabel = new Label
            {
                Text = DateTime.Now.ToString("HH:mm:ss"),
                Font = new Font("Segoe UI", 18),
                ForeColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            _currentTimeLabel.SetBounds(0, 30, 800, 30);

            _timePanel = new Panel
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Height = 120,
                BackColor = Color.Transparent
            };
            _timePanel.SetBounds(100, 150, 600, 120);

            _numberPictures = new List<PictureBox>();
            for (int i = 0; i < 4; i++)
            {
                var pb = new PictureBox
                {
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    BackColor = Color.Transparent
                };
                _numberPictures.Add(pb);
                _timePanel.Controls.Add(pb);
            }

            _separatorPanel = new Panel
            {
                BackColor = Color.Transparent
            };
            var separatorLabel = new Label
            {
                Text = ":",
                Font = new Font("Segoe UI", 48, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            _separatorPanel.Controls.Add(separatorLabel);
            _timePanel.Controls.Add(_separatorPanel);

            _progressLabel = new Label
            {
                Text = "点击开始专注",
                Font = new Font("Segoe UI", 16),
                ForeColor = Color.LightGray,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            _progressLabel.SetBounds(0, 290, 800, 30);

            _progressBar = new ProgressBar
            {
                Style = ProgressBarStyle.Continuous,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                ForeColor = Color.FromArgb(231, 76, 60),
                BackColor = Color.FromArgb(60, 60, 60)
            };
            _progressBar.SetBounds(100, 330, 600, 25);

            _startButton = new Button
            {
                Text = "开始",
                Font = new Font("Segoe UI", 14),
                Size = new Size(120, 50),
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _startButton.Click += StartButton_Click;
            _startButton.SetBounds(340, 380, 120, 50);

            _settingsButton = new Button
            {
                Text = "设置",
                Font = new Font("Segoe UI", 11),
                Size = new Size(90, 40),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _settingsButton.Click += SettingsButton_Click;

            _themeButton = new Button
            {
                Text = "主题",
                Font = new Font("Segoe UI", 11),
                Size = new Size(90, 40),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _themeButton.Click += ThemeButton_Click;

            _musicButton = new Button
            {
                Text = "音乐",
                Font = new Font("Segoe UI", 11),
                Size = new Size(90, 40),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _musicButton.Click += MusicButton_Click;

            _exitButton = new Button
            {
                Text = "退出",
                Font = new Font("Segoe UI", 11),
                Size = new Size(90, 40),
                BackColor = Color.FromArgb(192, 57, 43),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _exitButton.Click += ExitButton_Click;

            _toolsButton = new Button
            {
                Text = "小工具",
                Font = new Font("Segoe UI", 11),
                Size = new Size(90, 40),
                BackColor = Color.FromArgb(155, 89, 182),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _toolsButton.Click += ToolsButton_Click;

            _infoButton = new Button
            {
                Text = "信息",
                Font = new Font("Segoe UI", 11),
                Size = new Size(90, 40),
                BackColor = Color.FromArgb(52, 73, 94),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _infoButton.Click += InfoButton_Click;

            Controls.Add(_titleLabel);
            Controls.Add(_currentTimeLabel);
            Controls.Add(_timePanel);
            Controls.Add(_progressLabel);
            Controls.Add(_progressBar);
            Controls.Add(_startButton);
            Controls.Add(_settingsButton);
            Controls.Add(_themeButton);
            Controls.Add(_musicButton);
            Controls.Add(_toolsButton);
            Controls.Add(_infoButton);
            Controls.Add(_exitButton);
            
            _projectNameLabel = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(78, 205, 196),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Height = 25,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            _projectNameLabel.SetBounds(0, 115, 800, 25);
            Controls.Add(_projectNameLabel);

            KeyDown += MainForm_KeyDown;
            FormClosing += MainForm_FormClosing;
            Resize += MainForm_Resize;

            CheckFirstLaunch();
        }

        private void LoadSettings()
        {
            _totalSeconds = 25 * 60;
            _secondsRemaining = _totalSeconds;
            _isPomodoro = true;
            _isRunning = false;
            _lastActivityTime = DateTime.Now;
            _musicFiles = new List<string>();
            _musicFolder = Path.Combine(Application.StartupPath, "music");
            _useThemeNumbers = true;
            _workflowItems = new List<ProjectItem>();
            _currentWorkflowIndex = 0;

            Directory.CreateDirectory(Path.Combine(Application.StartupPath, "data"));

            UpdateTimeDisplay();
            UpdateCurrentTime();
        }

        private void LoadLastTheme()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    var content = File.ReadAllText(_settingsPath).Trim();
                    if (!string.IsNullOrEmpty(content))
                    {
                        ThemeManager.Instance.SetTheme(content);
                        return;
                    }
                }
            }
            catch { }

            ThemeManager.Instance.SetTheme("theme1");
        }

        private void SaveLastTheme()
        {
            try
            {
                File.WriteAllText(_settingsPath, ThemeManager.Instance.CurrentTheme.Name);
            }
            catch { }
        }

        private void LoadMusicFiles()
        {
            try
            {
                if (!Directory.Exists(_musicFolder))
                    Directory.CreateDirectory(_musicFolder);

                var extensions = new[] { ".mp3", ".wav", ".wma" };
                foreach (var ext in extensions)
                {
                    _musicFiles.AddRange(Directory.GetFiles(_musicFolder, "*" + ext));
                }
            }
            catch { }
        }

        public void ApplyTheme()
        {
            var theme = ThemeManager.Instance.CurrentTheme;

            if (theme != null && theme.GetBackground() != null)
            {
                BackgroundImage = theme.GetBackground();
                BackgroundImageLayout = ImageLayout.Stretch;
            }
            else
            {
                BackgroundImage = null;
                BackColor = Color.FromArgb(30, 30, 30);
            }

            UpdateTimeDisplay();
        }

        private void StartFullscreen()
        {
            _isFullscreen = true;
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            Size = Screen.PrimaryScreen.Bounds.Size;
            UpdateControlPositions();
            _fullscreenTimer.Start();
            _batteryTimer.Start();
            PreventSleep();
        }

        private void ExitFullscreen()
        {
            _isFullscreen = false;
            FormBorderStyle = FormBorderStyle.Sizable;
            WindowState = FormWindowState.Normal;
            Size = new Size(800, 600);
            UpdateControlPositions();
            AllowSleep();
        }

        private void UpdateControlPositions()
        {
            int width = Size.Width;
            int height = Size.Height;
            
            int safeMargin = Math.Max(30, width / 50);
            int buttonSpacing = Math.Max(10, width / 100);
            int buttonWidth = Math.Min(100, Math.Max(70, width / 10));
            int buttonHeight = Math.Max(35, height / 20);

            _titleLabel.Font = new Font("Segoe UI", Math.Max(20, Math.Min(36, height / 25)), FontStyle.Bold);
            _titleLabel.Width = width - safeMargin * 2;
            _titleLabel.Height = Math.Max(40, height / 15);
            _titleLabel.Location = new Point(safeMargin, height / 10);

            _projectNameLabel.Font = new Font("Segoe UI", Math.Max(12, Math.Min(18, height / 35)), FontStyle.Bold);
            _projectNameLabel.Width = width - safeMargin * 2;
            _projectNameLabel.Height = Math.Max(25, height / 25);
            _projectNameLabel.Location = new Point(safeMargin, _titleLabel.Location.Y + _titleLabel.Height + 5);

            _currentTimeLabel.Font = new Font("Segoe UI", Math.Max(12, Math.Min(20, height / 35)));
            _currentTimeLabel.Width = width - safeMargin * 2;
            _currentTimeLabel.Height = Math.Max(25, height / 25);
            _currentTimeLabel.Location = new Point(safeMargin, safeMargin);

            int timePanelMaxWidth = Math.Min(width - safeMargin * 4, 650);
            int timePanelMaxHeight = Math.Min(height / 4, 180);
            int timePanelWidth = Math.Max(300, timePanelMaxWidth);
            int timePanelHeight = Math.Max(100, timePanelMaxHeight);

            _timePanel.Size = new Size(timePanelWidth, timePanelHeight);
            _timePanel.Location = new Point((width - timePanelWidth) / 2, _projectNameLabel.Location.Y + _projectNameLabel.Height + safeMargin);

            int numHeight = _timePanel.Height - 10;
            int numWidth = (_timePanel.Width - 40) / 7;
            int spacing = (_timePanel.Width - 4 * numWidth - 30) / 5;
            spacing = Math.Max(5, spacing);

            for (int i = 0; i < _numberPictures.Count; i++)
            {
                int x = i < 2 ? i * (numWidth + spacing) : 2 * (numWidth + spacing) + 30 + spacing + (i - 2) * (numWidth + spacing);
                _numberPictures[i].Size = new Size(numWidth, numHeight);
                _numberPictures[i].Location = new Point(x, 5);
            }

            _separatorPanel.Size = new Size(30, numHeight);
            _separatorPanel.Location = new Point(2 * (numWidth + spacing), 5);

            _progressLabel.Font = new Font("Segoe UI", Math.Max(12, Math.Min(18, height / 35)));
            _progressLabel.Width = width - safeMargin * 2;
            _progressLabel.Height = Math.Max(25, height / 25);
            _progressLabel.Location = new Point(safeMargin, _timePanel.Location.Y + _timePanel.Height + safeMargin);

            int progressBarMaxWidth = Math.Min(width - safeMargin * 4, 650);
            int progressBarWidth = Math.Max(200, progressBarMaxWidth);
            _progressBar.Size = new Size(progressBarWidth, Math.Max(20, height / 30));
            _progressBar.Location = new Point((width - progressBarWidth) / 2, _progressLabel.Location.Y + _progressLabel.Height + safeMargin / 2);

            int startButtonWidth = Math.Max(100, Math.Min(150, width / 7));
            int startButtonHeight = Math.Max(40, height / 15);
            _startButton.Font = new Font("Segoe UI", Math.Max(12, Math.Min(16, height / 35)));
            _startButton.Size = new Size(startButtonWidth, startButtonHeight);
            _startButton.Location = new Point((width - startButtonWidth) / 2, _progressBar.Location.Y + _progressBar.Height + safeMargin * 2);

            int buttonY = height - safeMargin - buttonHeight - safeMargin / 2;
            int totalButtonWidth = 6 * buttonWidth + 5 * buttonSpacing;
            
            int buttonStartX;
            if (totalButtonWidth + safeMargin * 2 <= width)
            {
                buttonStartX = (width - totalButtonWidth) / 2;
            }
            else
            {
                buttonStartX = safeMargin;
                buttonWidth = (width - safeMargin * 2 - 5 * buttonSpacing) / 6;
            }

            _settingsButton.Font = new Font("Segoe UI", Math.Max(10, Math.Min(12, height / 45)));
            _settingsButton.Size = new Size(buttonWidth, buttonHeight);
            _settingsButton.Location = new Point(buttonStartX, buttonY);

            _themeButton.Font = new Font("Segoe UI", Math.Max(10, Math.Min(12, height / 45)));
            _themeButton.Size = new Size(buttonWidth, buttonHeight);
            _themeButton.Location = new Point(buttonStartX + buttonWidth + buttonSpacing, buttonY);

            _musicButton.Font = new Font("Segoe UI", Math.Max(10, Math.Min(12, height / 45)));
            _musicButton.Size = new Size(buttonWidth, buttonHeight);
            _musicButton.Location = new Point(buttonStartX + 2 * (buttonWidth + buttonSpacing), buttonY);

            _toolsButton.Font = new Font("Segoe UI", Math.Max(10, Math.Min(12, height / 45)));
            _toolsButton.Size = new Size(buttonWidth, buttonHeight);
            _toolsButton.Location = new Point(buttonStartX + 3 * (buttonWidth + buttonSpacing), buttonY);

            _infoButton.Font = new Font("Segoe UI", Math.Max(10, Math.Min(12, height / 45)));
            _infoButton.Size = new Size(buttonWidth, buttonHeight);
            _infoButton.Location = new Point(buttonStartX + 4 * (buttonWidth + buttonSpacing), buttonY);

            _exitButton.Font = new Font("Segoe UI", Math.Max(10, Math.Min(12, height / 45)));
            _exitButton.Size = new Size(buttonWidth, buttonHeight);
            _exitButton.Location = new Point(buttonStartX + 5 * (buttonWidth + buttonSpacing), buttonY);
        }

        private void PreventSleep()
        {
            NativeMethods.SetThreadExecutionState(NativeMethods.ES_CONTINUOUS | NativeMethods.ES_DISPLAY_REQUIRED | NativeMethods.ES_SYSTEM_REQUIRED);
        }

        private void AllowSleep()
        {
            NativeMethods.SetThreadExecutionState(NativeMethods.ES_CONTINUOUS);
        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            _lastActivityTime = DateTime.Now;

            if (e.KeyCode == Keys.Escape)
            {
                if (_isFullscreen)
                {
                    ExitFullscreen();
                }
            }
            else if (e.KeyCode == Keys.F11)
            {
                if (_isFullscreen)
                    ExitFullscreen();
                else
                    StartFullscreen();
            }
            else if (e.KeyCode == Keys.Space)
            {
                StartButton_Click(null, null);
            }
        }

        private void ShowExitDialog()
        {
            using (var exitForm = new ExitConfirmForm())
            {
                switch (exitForm.ShowDialog())
                {
                    case DialogResult.OK:
                        _timer.Stop();
                        _isRunning = false;
                        SaveLastTheme();
                        Application.Exit();
                        break;
                    case DialogResult.Cancel:
                        break;
                }
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_isRunning)
            {
                e.Cancel = true;
                ShowExitDialog();
            }
            else
            {
                SaveLastTheme();
                _timer.Stop();
                _fullscreenTimer.Stop();
                _batteryTimer.Stop();
                AllowSleep();
            }
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            UpdateControlPositions();
            ApplyTheme();
        }

        private string GetProgressMessage(int elapsedPercent)
        {
            if (_isPomodoro)
            {
                if (elapsedPercent <= 30)
                    return "坚持就是胜利";
                else if (elapsedPercent <= 50)
                    return "失败是成功之母";
                else if (elapsedPercent <= 80)
                    return "时间快到了";
                else
                    return "计时即将结束";
            }
            else
            {
                if (elapsedPercent <= 50)
                    return "放松一下";
                else
                    return "休息休息";
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateCurrentTime();

            if (_secondsRemaining > 0)
            {
                _secondsRemaining--;
                int elapsed = _totalSeconds - _secondsRemaining;
                int elapsedPercent = (int)((double)elapsed / _totalSeconds * 100);

                if (_workflowItems != null && _workflowItems.Count > 0 && _currentWorkflowIndex < _workflowItems.Count)
                {
                    var currentItem = _workflowItems[_currentWorkflowIndex];
                    _progressLabel.Text = currentItem.DisplayText;
                }
                else
                {
                    _progressLabel.Text = GetProgressMessage(elapsedPercent);
                }

                UpdateTimeDisplay();
            }
            else
            {
                _timer.Stop();
                _isRunning = false;
                _startButton.Text = "开始";

                ThemeManager.Instance.PlayEndSound();

                if (_workflowItems != null && _workflowItems.Count > 0 && _currentWorkflowIndex < _workflowItems.Count)
                {
                    NextWorkflowItem();
                }
                else
                {
                    if (_isPomodoro)
                    {
                        _totalSeconds = 5 * 60;
                        _isPomodoro = false;
                        _titleLabel.Text = "休息时间";
                        _progressLabel.Text = "休息中...";
                        _progressBar.ForeColor = Color.FromArgb(39, 174, 96);
                    }
                    else
                    {
                        _totalSeconds = 25 * 60;
                        _isPomodoro = true;
                        _titleLabel.Text = "番茄工作法";
                        _progressLabel.Text = "专注中...";
                        _progressBar.ForeColor = Color.FromArgb(231, 76, 60);
                    }
                    _secondsRemaining = _totalSeconds;
                    UpdateTimeDisplay();
                }
            }
        }

        private void FullscreenTimer_Tick(object sender, EventArgs e)
        {
            UpdateCurrentTime();

            if (_isFullscreen && (DateTime.Now - _lastActivityTime).TotalSeconds > 10)
            {
                if (WindowState != FormWindowState.Maximized)
                {
                    StartFullscreen();
                }
            }
        }

        private void BatteryTimer_Tick(object sender, EventArgs e)
        {
            CheckBatteryStatus();
        }

        private void CheckBatteryStatus()
        {
            try
            {
                var powerStatus = SystemInformation.PowerStatus;

                if (powerStatus.BatteryChargeStatus != BatteryChargeStatus.NoSystemBattery)
                {
                    int batteryPercent = (int)(powerStatus.BatteryLifePercent * 100);

                    if (powerStatus.PowerLineStatus == PowerLineStatus.Offline && batteryPercent < 20)
                    {
                        MessageBox.Show($"电池电量不足 ({batteryPercent}%)，请尽快充电！", "低电量警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
            catch { }
        }

        private void UpdateCurrentTime()
        {
            _currentTimeLabel.Text = DateTime.Now.ToString("HH:mm:ss");
        }

        private void UpdateTimeDisplay()
        {
            int minutes = _secondsRemaining / 60;
            int seconds = _secondsRemaining % 60;

            int min1 = minutes / 10;
            int min2 = minutes % 10;
            int sec1 = seconds / 10;
            int sec2 = seconds % 10;

            UpdateNumberPicture(0, min1);
            UpdateNumberPicture(1, min2);
            UpdateNumberPicture(2, sec1);
            UpdateNumberPicture(3, sec2);

            _progressBar.Value = (int)((double)_secondsRemaining / _totalSeconds * 100);
        }

        private void UpdateNumberPicture(int index, int number)
        {
            var theme = ThemeManager.Instance.CurrentTheme;
            if (_useThemeNumbers && theme != null && theme.NumbersData[number] != null)
            {
                _numberPictures[index].Image = theme.GetNumberImage(number);
            }
            else
            {
                _numberPictures[index].Image = null;
                _numberPictures[index].BackColor = Color.Transparent;
                using (var bmp = new Bitmap(_numberPictures[index].Width, _numberPictures[index].Height))
                using (var g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.Transparent);
                    using (var font = new Font("Segoe UI", 72, FontStyle.Bold))
                    {
                        var size = g.MeasureString(number.ToString(), font);
                        var x = (bmp.Width - size.Width) / 2;
                        var y = (bmp.Height - size.Height) / 2;
                        g.DrawString(number.ToString(), font, new SolidBrush(Color.White), x, y);
                    }
                    _numberPictures[index].Image = (Bitmap)bmp.Clone();
                }
            }
        }

        public void ClearNumberImages()
        {
            foreach (var pb in _numberPictures)
            {
                if (pb.Image != null)
                {
                    pb.Image.Dispose();
                    pb.Image = null;
                }
            }
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            if (_isRunning)
            {
                _timer.Stop();
                _isRunning = false;
                _startButton.Text = "继续";
                _titleLabel.Text = "已暂停";
                _progressLabel.Text = "已暂停";
            }
            else
            {
                ThemeManager.Instance.PlayStartSound();
                _timer.Start();
                _isRunning = true;
                _startButton.Text = "暂停";

                if (_workflowItems != null && _workflowItems.Count > 0 && _currentWorkflowIndex < _workflowItems.Count)
                {
                    StartWorkflow();
                    StartFullscreen();
                }
                else
                {
                    if (_isPomodoro)
                    {
                        _titleLabel.Text = "BusyBox番茄工作法";
                        StartFullscreen();
                    }
                    else
                    {
                        _titleLabel.Text = "休息时间";
                    }
                    _progressLabel.Text = _isPomodoro ? "专注中..." : "休息中...";
                }
            }
        }

        private void SettingsButton_Click(object sender, EventArgs e)
        {
            if (_isRunning)
            {
                _timer.Stop();
                _isRunning = false;
            }

            using (var settingsForm = new SettingsForm(_totalSeconds / 60))
            {
                if (settingsForm.ShowDialog() == DialogResult.OK && settingsForm.ProjectItems.Count > 0)
                {
                    _workflowItems = settingsForm.ProjectItems;
                    _currentWorkflowIndex = 0;
                    StartWorkflow();
                }
            }
        }

        private void StartWorkflow()
        {
            if (_workflowItems == null || _workflowItems.Count == 0 || _currentWorkflowIndex >= _workflowItems.Count)
            {
                return;
            }

            var currentItem = _workflowItems[_currentWorkflowIndex];
            int duration;
            if (int.TryParse(currentItem.Duration, out duration))
            {
                _totalSeconds = duration * 60;
                _secondsRemaining = _totalSeconds;
            }

            _isPomodoro = currentItem.Type == "忙碌";

            if (_isPomodoro)
            {
                _titleLabel.Text = currentItem.Name;
                _progressBar.ForeColor = Color.FromArgb(231, 76, 60);
            }
            else
            {
                _titleLabel.Text = currentItem.Name;
                _progressBar.ForeColor = Color.FromArgb(39, 174, 96);
            }

            _projectNameLabel.Text = currentItem.DisplayText;
            _progressLabel.Text = $"项目 {(_currentWorkflowIndex + 1)}/{_workflowItems.Count}";
            UpdateTimeDisplay();
        }

        private void NextWorkflowItem()
        {
            _currentWorkflowIndex++;
            if (_currentWorkflowIndex >= _workflowItems.Count)
            {
                _timer.Stop();
                _isRunning = false;
                _startButton.Text = "开始";

                ThemeManager.Instance.PlayEndSound();
                MessageBox.Show("全部项目已完成！", "恭喜", MessageBoxButtons.OK, MessageBoxIcon.Information);

                _titleLabel.Text = "BusyBox番茄工作法";
                _progressLabel.Text = "点击开始专注";
                _projectNameLabel.Text = "";
                _totalSeconds = 25 * 60;
                _secondsRemaining = _totalSeconds;
                _currentWorkflowIndex = 0;
                _workflowItems.Clear();
                UpdateTimeDisplay();
            }
            else
            {
                StartWorkflow();
            }
        }

        private void ThemeButton_Click(object sender, EventArgs e)
        {
            using (var themeForm = new ThemeManagerForm(this))
            {
                if (themeForm.ShowDialog() == DialogResult.OK)
                {
                    SaveLastTheme();
                    ApplyTheme();
                }
            }
        }

        private void MusicButton_Click(object sender, EventArgs e)
        {
            var musicForm = new MusicPlayerForm(_musicFiles, _musicFolder);
            musicForm.Show();
        }

        private void InfoButton_Click(object sender, EventArgs e)
        {
            var infoForm = new InfoForm();
            infoForm.ShowDialog();
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            if (_isRunning)
            {
                ShowExitDialog();
            }
            else
            {
                SaveLastTheme();
                AllowSleep();
                Application.Exit();
            }
        }

        private void ToolsButton_Click(object sender, EventArgs e)
        {
            using (var toolsForm = new ToolsForm())
            {
                toolsForm.ShowDialog(this);
            }
        }

        private void CheckFirstLaunch()
        {
            var firstLaunchFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BusyBox", "first_launch.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(firstLaunchFile));

            if (!File.Exists(firstLaunchFile))
            {
                File.WriteAllText(firstLaunchFile, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                var infoForm = new InfoForm();
                infoForm.ShowDialog();
            }
        }
    }

    public class ToolsForm : Form
    {
        public ToolsForm()
        {
            Text = "小工具";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(40, 40, 40);
            AutoScaleMode = AutoScaleMode.None;
            Size = new Size(480, 280);
            MinimumSize = Size;
            MaximumSize = Size;

            InitializeControls();
        }

        private void InitializeControls()
        {
            var titleLabel = new Label
            {
                Text = "小工具",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(0, 20),
                Size = new Size(480, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };
            Controls.Add(titleLabel);

            var calculatorButton = new Button
            {
                Text = "计算器",
                Font = new Font("Segoe UI", 12),
                Size = new Size(140, 50),
                Location = new Point(35, 70),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            calculatorButton.Click += CalculatorButton_Click;
            Controls.Add(calculatorButton);

            var notesButton = new Button
            {
                Text = "快速便签",
                Font = new Font("Segoe UI", 12),
                Size = new Size(140, 50),
                Location = new Point(170, 70),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            notesButton.Click += NotesButton_Click;
            Controls.Add(notesButton);

            var todoButton = new Button
            {
                Text = "Todo List",
                Font = new Font("Segoe UI", 12),
                Size = new Size(140, 50),
                Location = new Point(305, 70),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            todoButton.Click += TodoButton_Click;
            Controls.Add(todoButton);

            var aiButton = new Button
            {
                Text = "人工智能",
                Font = new Font("Segoe UI", 12),
                Size = new Size(140, 50),
                Location = new Point(35, 135),
                BackColor = Color.FromArgb(155, 89, 182),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            aiButton.Click += AIButton_Click;
            Controls.Add(aiButton);

            var hostInfoButton = new Button
            {
                Text = "主机信息",
                Font = new Font("Segoe UI", 12),
                Size = new Size(140, 50),
                Location = new Point(170, 135),
                BackColor = Color.FromArgb(155, 89, 182),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            hostInfoButton.Click += HostInfoButton_Click;
            Controls.Add(hostInfoButton);
        }

        private void CalculatorButton_Click(object sender, EventArgs e)
        {
            using (var calcForm = new ScientificCalculatorForm())
            {
                calcForm.ShowDialog(this);
            }
        }

        private void NotesButton_Click(object sender, EventArgs e)
        {
            var notesForm = new NotesForm();
            notesForm.Show();
        }

        private void TodoButton_Click(object sender, EventArgs e)
        {
            var todoForm = new TodoForm();
            todoForm.Show();
        }

        private void AIButton_Click(object sender, EventArgs e)
        {
            var aiForm = new AIForm();
            aiForm.Show();
        }

        private void HostInfoButton_Click(object sender, EventArgs e)
        {
            var hostInfoForm = new HostInfoForm();
            hostInfoForm.Show();
        }

        private void OldHostInfoButton_Click(object sender, EventArgs e)
        {
            var info = $"操作系统: {Environment.OSVersion}\n" +
                       $"处理器: {Environment.ProcessorCount} 核\n" +
                       $"内存: {GC.GetTotalMemory(false) / (1024 * 1024):0.00} MB\n" +
                       $"用户名: {Environment.UserName}\n" +
                       $"机器名: {Environment.MachineName}\n" +
                       $"系统目录: {Environment.SystemDirectory}";

            MessageBox.Show(info, "主机信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    public class ExitConfirmForm : Form
    {
        private Label _messageLabel;
        private Button _yesButton;
        private Button _noButton;

        public ExitConfirmForm()
        {
            Text = "退出确认";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(40, 40, 40);
            AutoScaleMode = AutoScaleMode.None;
            Size = new Size(340, 180);
            MinimumSize = Size;
            MaximumSize = Size;

            _messageLabel = new Label
            {
                Text = "当前正在计时，确定要退出吗？",
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.White,
                Location = new Point(15, 15),
                Size = new Size(270, 50),
                TextAlign = ContentAlignment.MiddleCenter
            };

            _yesButton = new Button
            {
                Text = "中止并退出",
                Font = new Font("Segoe UI", 11),
                Size = new Size(120, 38),
                BackColor = Color.FromArgb(192, 57, 43),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.OK,
                Location = new Point(35, 110)
            };

            _noButton = new Button
            {
                Text = "不关闭",
                Font = new Font("Segoe UI", 11),
                Size = new Size(120, 38),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel,
                Location = new Point(175, 110)
            };

            Controls.Add(_messageLabel);
            Controls.Add(_yesButton);
            Controls.Add(_noButton);
        }
    }

    public class SettingsForm : Form
    {
        private Button _newProjectButton;
        private Panel _projectPresetPanel;
        private Button _saveProjectPresetButton;
        private Panel _projectListPanel;
        private ListBox _projectListBox;
        private Panel _workflowPresetPanel;
        private Button _saveWorkflowPresetButton;
        private Button _confirmButton;
        private Button _cancelButton;

        public List<ProjectItem> ProjectItems { get; private set; } = new List<ProjectItem>();

        public SettingsForm(int currentPomodoro)
        {
            Text = "设置";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(245, 245, 245);
            AutoScaleMode = AutoScaleMode.None;
            Size = new Size(500, 690);
            MinimumSize = Size;
            MaximumSize = Size;

            InitializeControls();
        }

        private void InitializeControls()
        {
            _newProjectButton = new Button
            {
                Text = "新建项目",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                Size = new Size(90, 50),
                Location = new Point(25, 25),
                BackColor = Color.FromArgb(78, 205, 196),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _newProjectButton.Click += NewProjectButton_Click;
            Controls.Add(_newProjectButton);

            _projectPresetPanel = new Panel
            {
                Size = new Size(91, 280),
                Location = new Point(25, 75),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var presetLabel = new Label
            {
                Text = "项目预设",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(102, 102, 102),
                Location = new Point(5, 5),
                Size = new Size(80, 20)
            };
            _projectPresetPanel.Controls.Add(presetLabel);

            CreateProjectPresetButton(_projectPresetPanel, "工作/学习", 30, new ProjectItem
            {
                Name = "工作/学习",
                DisplayText = "坚持就是胜利",
                Duration = "25",
                Type = "忙碌"
            });
            CreateProjectPresetButton(_projectPresetPanel, "休息", 65, new ProjectItem
            {
                Name = "休息",
                DisplayText = "放松一下",
                Duration = "5",
                Type = "放松"
            });
            Controls.Add(_projectPresetPanel);

            _saveProjectPresetButton = new Button
            {
                Text = "保存项目预设",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Size = new Size(90, 40),
                Location = new Point(25, 355),
                BackColor = Color.FromArgb(78, 205, 196),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _saveProjectPresetButton.Click += SaveProjectPresetButton_Click;
            Controls.Add(_saveProjectPresetButton);

            _projectListPanel = new Panel
            {
                Size = new Size(130, 430),
                Location = new Point(135, 25),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var headerPanel = new Panel
            {
                Size = new Size(128, 20),
                Location = new Point(0, 0),
                BackColor = Color.FromArgb(245, 245, 245)
            };

            var nameHeader = new Label
            {
                Text = "名称",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(102, 102, 102),
                Location = new Point(2, 0),
                Size = new Size(65, 20)
            };
            headerPanel.Controls.Add(nameHeader);

            var durationHeader = new Label
            {
                Text = "时长",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(102, 102, 102),
                Location = new Point(72, 0),
                Size = new Size(56, 20)
            };
            headerPanel.Controls.Add(durationHeader);
            _projectListPanel.Controls.Add(headerPanel);

            _projectListBox = new ListBox
            {
                Size = new Size(128, 410),
                Location = new Point(0, 20),
                BackColor = Color.White,
                ForeColor = Color.Black,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 11),
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 35
            };
            _projectListBox.DrawItem += ProjectListBox_DrawItem;
            _projectListBox.DoubleClick += ProjectListBox_DoubleClick;
            _projectListPanel.Controls.Add(_projectListBox);
            Controls.Add(_projectListPanel);

            _workflowPresetPanel = new Panel
            {
                Size = new Size(130, 420),
                Location = new Point(285, 25),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            var workflowLabel = new Label
            {
                Text = "工作流预设",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(102, 102, 102),
                Location = new Point(5, 5),
                Size = new Size(80, 20)
            };
            _workflowPresetPanel.Controls.Add(workflowLabel);

            var efficientStudyItems = new List<ProjectItem>
            {
                new ProjectItem { Name = "深度学习", DisplayText = "不经一番寒彻骨，怎得梅花扑鼻香", Duration = "25", Type = "忙碌" },
                new ProjectItem { Name = "休息", DisplayText = "休息一下", Duration = "5", Type = "放松" },
                new ProjectItem { Name = "深度学习", DisplayText = "书山有路勤为径，学海无涯苦作舟", Duration = "25", Type = "忙碌" },
                new ProjectItem { Name = "休息", DisplayText = "休息一下", Duration = "5", Type = "放松" },
                new ProjectItem { Name = "复习收尾", DisplayText = "长风破浪会有时，直挂云帆济沧海", Duration = "35", Type = "忙碌" }
            };
            CreateWorkflowPresetButton(_workflowPresetPanel, "高效学习", 30, efficientStudyItems);

            var quickWorkItems = new List<ProjectItem>
            {
                new ProjectItem { Name = "工作", DisplayText = "现在的态度，决定十年后是人物还是废物", Duration = "35", Type = "忙碌" },
                new ProjectItem { Name = "休息", DisplayText = "简单休整", Duration = "5", Type = "放松" },
                new ProjectItem { Name = "工作", DisplayText = "成功即站起比倒下多一次", Duration = "35", Type = "忙碌" },
                new ProjectItem { Name = "休息", DisplayText = "休息一下", Duration = "5", Type = "放松" }
            };
            CreateWorkflowPresetButton(_workflowPresetPanel, "快速工作", 68, quickWorkItems);

            Controls.Add(_workflowPresetPanel);

            _saveWorkflowPresetButton = new Button
            {
                Text = "保存工作流预设",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Size = new Size(90, 50),
                Location = new Point(285, 445),
                BackColor = Color.FromArgb(255, 107, 107),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _saveWorkflowPresetButton.Click += SaveWorkflowPresetButton_Click;
            Controls.Add(_saveWorkflowPresetButton);

            _confirmButton = new Button
            {
                Text = "确定",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Size = new Size(100, 60),
                Location = new Point(25, 605),
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _confirmButton.Click += ConfirmButton_Click;
            Controls.Add(_confirmButton);

            _cancelButton = new Button
            {
                Text = "取消",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Size = new Size(100, 60),
                Location = new Point(165, 605),
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _cancelButton.Click += CancelButton_Click;
            Controls.Add(_cancelButton);
        }

        private void NewProjectButton_Click(object sender, EventArgs e)
        {
            if (_projectListBox.Items.Count >= 16)
            {
                MessageBox.Show("项目数量已达上限（最多16个）", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int count = 1;
            while (true)
            {
                string name = $"新建项目{count}";
                bool exists = false;
                foreach (var item in _projectListBox.Items)
                {
                    if (((ProjectItem)item).Name == name)
                    {
                        exists = true;
                        break;
                    }
                }
                if (!exists)
                {
                    _projectListBox.Items.Add(new ProjectItem
                    {
                        Name = name,
                        DisplayText = "未设定",
                        Duration = "25",
                        Type = "忙碌"
                    });
                    break;
                }
                count++;
            }
        }

        private void SaveProjectPresetButton_Click(object sender, EventArgs e)
        {
            var presetButtons = new List<Button>();
            foreach (var ctrl in _projectPresetPanel.Controls)
            {
                if (ctrl is Button)
                {
                    var btn = (Button)ctrl;
                    var tag = btn.Tag as object[];
                    if (tag != null && tag.Length >= 3)
                    {
                        presetButtons.Add(btn);
                    }
                }
            }
            if (presetButtons.Count >= 7)
            {
                MessageBox.Show("项目预设已达上限（最多7个）", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var inputForm = new ProjectPresetInputForm())
            {
                if (inputForm.ShowDialog() == DialogResult.OK)
                {
                    bool exists = false;
                    foreach (var btn in presetButtons)
                    {
                        if (btn.Text == inputForm.PresetName)
                        {
                            exists = true;
                            break;
                        }
                    }
                    if (exists)
                    {
                        MessageBox.Show("该预设名称已存在", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    var item = new ProjectItem
                    {
                        Name = inputForm.ItemName,
                        DisplayText = inputForm.DisplayText,
                        Duration = inputForm.Duration,
                        Type = inputForm.Type
                    };

                    int newY = 30 + presetButtons.Count * 35;
                    CreateProjectPresetButton(_projectPresetPanel, inputForm.PresetName, newY, item);
                    MessageBox.Show("项目预设已保存", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void CreateProjectPresetButton(Panel parentPanel, string name, int y, ProjectItem item = null)
        {
            if (item == null)
            {
                item = new ProjectItem
                {
                    Name = name,
                    DisplayText = "未设定",
                    Duration = "25",
                    Type = "忙碌"
                };
            }

            var btn = new Button
            {
                Text = name,
                Font = new Font("Segoe UI", 11),
                Size = new Size(50, 25),
                Location = new Point(5, y),
                BackColor = Color.FromArgb(238, 238, 238),
                ForeColor = Color.FromArgb(51, 51, 51),
                FlatStyle = FlatStyle.Flat
            };

            var addBtn = new Button
            {
                Text = "添加",
                Font = new Font("Segoe UI", 9),
                Size = new Size(35, 11),
                Location = new Point(57, y + 1),
                BackColor = Color.FromArgb(78, 205, 196),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Visible = false,
                Tag = btn
            };
            addBtn.MouseEnter += AddPresetBtn_MouseEnter;
            addBtn.MouseLeave += AddPresetBtn_MouseLeave;
            addBtn.Click += AddPresetBtn_Click;

            var deleteBtn = new Button
            {
                Text = "删除",
                Font = new Font("Segoe UI", 9),
                Size = new Size(35, 11),
                Location = new Point(57, y + 13),
                BackColor = Color.FromArgb(255, 107, 107),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Visible = false,
                Tag = btn
            };
            deleteBtn.MouseEnter += AddPresetBtn_MouseEnter;
            deleteBtn.MouseLeave += AddPresetBtn_MouseLeave;
            deleteBtn.Click += DeleteProjectPresetBtn_Click;

            btn.MouseEnter += PresetBtn_MouseEnter;
            btn.Tag = new object[] { addBtn, deleteBtn, item };
            parentPanel.Controls.Add(btn);
            parentPanel.Controls.Add(addBtn);
            parentPanel.Controls.Add(deleteBtn);
        }

        private void PresetBtn_MouseEnter(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            var tag = btn.Tag as object[];
            if (tag != null)
            {
                ((Button)tag[0]).Visible = true;
                ((Button)tag[1]).Visible = true;
            }
        }

        private void AddPresetBtn_MouseEnter(object sender, EventArgs e)
        {
            var panel = (Panel)((Button)sender).Parent;
            foreach (var ctrl in panel.Controls)
            {
                if (ctrl is Button)
                {
                    var b = (Button)ctrl;
                    var t = b.Tag as object[];
                    if (t != null)
                    {
                        ((Button)t[0]).Visible = true;
                        ((Button)t[1]).Visible = true;
                    }
                }
            }
        }

        private void AddPresetBtn_MouseLeave(object sender, EventArgs e)
        {
            var senderBtn = sender as Button;
            if (senderBtn == null) return;

            var panel = senderBtn.Parent as Panel;
            if (panel == null) return;

            foreach (var ctrl in panel.Controls)
            {
                if (ctrl is Button)
                {
                    var btn = (Button)ctrl;
                    var tag = btn.Tag as object[];
                    if (tag != null && tag.Length >= 2)
                    {
                        if (tag[0] is Button && tag[1] is Button)
                        {
                            ((Button)tag[0]).Visible = false;
                            ((Button)tag[1]).Visible = false;
                        }
                    }
                }
            }
        }

        private void AddPresetBtn_Click(object sender, EventArgs e)
        {
            var addBtn = (Button)sender;
            var mainBtn = (Button)addBtn.Tag;
            if (_projectListBox.Items.Count >= 16)
            {
                MessageBox.Show("项目数量已达上限（最多16个）", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var tag = mainBtn.Tag as object[];
            ProjectItem item = null;
            if (tag != null && tag.Length >= 3)
            {
                item = tag[2] as ProjectItem;
            }

            if (item != null)
            {
                _projectListBox.Items.Add(new ProjectItem
                {
                    Name = item.Name,
                    DisplayText = item.DisplayText,
                    Duration = item.Duration,
                    Type = item.Type
                });
            }
            else
            {
                _projectListBox.Items.Add(new ProjectItem
                {
                    Name = mainBtn.Text,
                    DisplayText = "未设定",
                    Duration = "25",
                    Type = "忙碌"
                });
            }
        }

        private void DeleteProjectPresetBtn_Click(object sender, EventArgs e)
        {
            var deleteBtn = (Button)sender;
            var mainBtn = (Button)deleteBtn.Tag;
            var panel = _projectPresetPanel;

            var tag = mainBtn.Tag as object[];
            if (tag != null)
            {
                panel.Controls.Remove((Button)tag[0]);
                panel.Controls.Remove((Button)tag[1]);
            }
            panel.Controls.Remove(mainBtn);

            ReorganizeProjectPresetButtons();
        }

        private void ReorganizeProjectPresetButtons()
        {
            int y = 30;
            var buttons = new List<Button>();
            foreach (var ctrl in _projectPresetPanel.Controls)
            {
                if (ctrl is Button)
                {
                    var btn = (Button)ctrl;
                    var tag = btn.Tag;
                    if (tag is object[])
                    {
                        buttons.Add(btn);
                    }
                }
            }

            foreach (var btn in buttons)
            {
                btn.Location = new Point(5, y);
                var tag = btn.Tag as object[];
                if (tag != null)
                {
                    ((Button)tag[0]).Location = new Point(57, y + 1);
                    ((Button)tag[1]).Location = new Point(57, y + 13);
                }
                y += 35;
            }
        }

        private void SaveWorkflowPresetButton_Click(object sender, EventArgs e)
        {
            if (_projectListBox.Items.Count == 0)
            {
                MessageBox.Show("请先添加项目", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var workflowButtons = new List<Button>();
            foreach (var ctrl in _workflowPresetPanel.Controls)
            {
                if (ctrl is Button)
                {
                    var btn = (Button)ctrl;
                    var tag = btn.Tag;
                    if (tag is object[])
                    {
                        workflowButtons.Add(btn);
                    }
                }
            }
            if (workflowButtons.Count >= 10)
            {
                MessageBox.Show("工作流预设已达上限（最多10个）", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var inputForm = new InputNameForm("保存工作流预设", "请输入工作流名称："))
            {
                if (inputForm.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(inputForm.InputName))
                {
                    bool exists = false;
                    foreach (var btn in workflowButtons)
                    {
                        if (btn.Text == inputForm.InputName)
                        {
                            exists = true;
                            break;
                        }
                    }
                    if (exists)
                    {
                        MessageBox.Show("该工作流名称已存在", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    var workflowItems = new List<ProjectItem>();
                    foreach (var item in _projectListBox.Items)
                    {
                        var pi = (ProjectItem)item;
                        workflowItems.Add(new ProjectItem
                        {
                            Name = pi.Name,
                            DisplayText = pi.DisplayText,
                            Duration = pi.Duration,
                            Type = pi.Type
                        });
                    }

                    int newY = 30 + workflowButtons.Count * 38;
                    CreateWorkflowPresetButton(_workflowPresetPanel, inputForm.InputName, newY, workflowItems);
                    MessageBox.Show("工作流预设已保存", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void CreateWorkflowPresetButton(Panel parentPanel, string name, int y, List<ProjectItem> items)
        {
            var btn = new Button
            {
                Text = name,
                Font = new Font("Segoe UI", 11),
                Size = new Size(50, 25),
                Location = new Point(5, y),
                BackColor = Color.FromArgb(238, 238, 238),
                ForeColor = Color.FromArgb(51, 51, 51),
                FlatStyle = FlatStyle.Flat,
                Tag = items
            };

            var useBtn = new Button
            {
                Text = "使用",
                Font = new Font("Segoe UI", 9),
                Size = new Size(35, 11),
                Location = new Point(57, y + 1),
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Visible = false,
                Tag = btn
            };
            useBtn.MouseEnter += WorkflowPresetBtn_MouseEnter;
            useBtn.MouseLeave += WorkflowPresetBtn_MouseLeave;
            useBtn.Click += UseWorkflowBtn_Click;

            var deleteBtn = new Button
            {
                Text = "删除",
                Font = new Font("Segoe UI", 9),
                Size = new Size(35, 11),
                Location = new Point(57, y + 13),
                BackColor = Color.FromArgb(255, 107, 107),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Visible = false,
                Tag = btn
            };
            deleteBtn.MouseEnter += WorkflowPresetBtn_MouseEnter;
            deleteBtn.MouseLeave += WorkflowPresetBtn_MouseLeave;
            deleteBtn.Click += DeleteWorkflowPresetBtn_Click;

            btn.MouseEnter += WorkflowPresetBtn_MouseEnter;
            btn.Tag = items;
            parentPanel.Controls.Add(btn);
            parentPanel.Controls.Add(useBtn);
            parentPanel.Controls.Add(deleteBtn);

            btn.Tag = new object[] { useBtn, deleteBtn, items };
        }

        private void WorkflowPresetBtn_MouseEnter(object sender, EventArgs e)
        {
            var senderBtn = sender as Button;
            if (senderBtn == null) return;

            var parentPanel = senderBtn.Parent as Panel;
            if (parentPanel == null) return;

            foreach (var ctrl in parentPanel.Controls)
            {
                if (ctrl is Button)
                {
                    var b = (Button)ctrl;
                    var tag = b.Tag as object[];
                    if (tag != null && tag.Length >= 2)
                    {
                        if (tag[0] is Button && tag[1] is Button)
                        {
                            ((Button)tag[0]).Visible = true;
                            ((Button)tag[1]).Visible = true;
                        }
                    }
                }
            }
        }

        private void WorkflowPresetBtn_MouseLeave(object sender, EventArgs e)
        {
            var senderBtn = sender as Button;
            if (senderBtn == null) return;

            var parentPanel = senderBtn.Parent as Panel;
            if (parentPanel == null) return;

            foreach (var ctrl in parentPanel.Controls)
            {
                if (ctrl is Button)
                {
                    var b = (Button)ctrl;
                    var tag = b.Tag as object[];
                    if (tag != null && tag.Length >= 2)
                    {
                        if (tag[0] is Button && tag[1] is Button)
                        {
                            ((Button)tag[0]).Visible = false;
                            ((Button)tag[1]).Visible = false;
                        }
                    }
                }
            }
        }

        private void UseWorkflowBtn_Click(object sender, EventArgs e)
        {
            var useBtn = (Button)sender;
            var tag = useBtn.Tag as Button;
            if (tag == null) return;

            var mainTag = tag.Tag as object[];
            if (mainTag == null) return;

            var workflowItems = mainTag[2] as List<ProjectItem>;
            if (workflowItems == null) return;

            _projectListBox.Items.Clear();
            foreach (var item in workflowItems)
            {
                _projectListBox.Items.Add(new ProjectItem
                {
                    Name = item.Name,
                    DisplayText = item.DisplayText,
                    Duration = item.Duration,
                    Type = item.Type
                });
            }
        }

        private void DeleteWorkflowPresetBtn_Click(object sender, EventArgs e)
        {
            var deleteBtn = (Button)sender;
            var mainBtn = deleteBtn.Tag as Button;
            var panel = _workflowPresetPanel;

            if (mainBtn == null) return;

            var tag = mainBtn.Tag as object[];
            if (tag != null)
            {
                panel.Controls.Remove((Button)tag[0]);
                panel.Controls.Remove((Button)tag[1]);
            }
            panel.Controls.Remove(mainBtn);

            ReorganizeWorkflowPresetButtons();
        }

        private void ReorganizeWorkflowPresetButtons()
        {
            int y = 30;
            var buttons = new List<Button>();
            foreach (var ctrl in _workflowPresetPanel.Controls)
            {
                if (ctrl is Button)
                {
                    var btn = (Button)ctrl;
                    var tag = btn.Tag;
                    if (tag is object[])
                    {
                        buttons.Add(btn);
                    }
                }
            }

            foreach (var btn in buttons)
            {
                btn.Location = new Point(5, y);
                var tag = btn.Tag as object[];
                if (tag != null)
                {
                    ((Button)tag[0]).Location = new Point(57, y + 1);
                    ((Button)tag[1]).Location = new Point(57, y + 13);
                }
                y += 38;
            }
        }

        private void ConfirmButton_Click(object sender, EventArgs e)
        {
            ProjectItems.Clear();
            foreach (ProjectItem item in _projectListBox.Items)
            {
                ProjectItems.Add(new ProjectItem
                {
                    Name = item.Name,
                    DisplayText = item.DisplayText,
                    Duration = item.Duration,
                    Type = item.Type
                });
            }
            DialogResult = DialogResult.OK;
            Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void ProjectListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            var item = (ProjectItem)_projectListBox.Items[e.Index];
            e.DrawBackground();

            Brush nameBrush = item.Type == "忙碌" ? Brushes.Red : Brushes.Green;
            using (var brush = new SolidBrush(e.ForeColor))
            {
                e.Graphics.DrawString(item.Name, e.Font, nameBrush, e.Bounds.Left + 2, e.Bounds.Top + 8);
                e.Graphics.DrawString(item.Duration, e.Font, brush, e.Bounds.Left + 100, e.Bounds.Top + 8);
            }

            if (e.Bounds.Bottom < _projectListBox.ClientSize.Height - 1)
            {
                e.Graphics.DrawLine(Pens.LightGray, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
            }

            e.DrawFocusRectangle();
        }

        private void ProjectListBox_DoubleClick(object sender, EventArgs e)
        {
            if (_projectListBox.SelectedItem != null)
            {
                var item = (ProjectItem)_projectListBox.SelectedItem;
                int itemIndex = _projectListBox.SelectedIndex;
                using (var detailForm = new ProjectDetailForm(item, _projectListBox, itemIndex))
                {
                    if (detailForm.ShowDialog() == DialogResult.OK)
                    {
                        _projectListBox.Invalidate();
                    }
                }
            }
        }

        private class InputNameForm : Form
        {
            public string InputName { get; private set; }
            private TextBox _nameTextBox;

            public InputNameForm(string title, string labelText)
            {
                Text = title;
                StartPosition = FormStartPosition.CenterParent;
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MaximizeBox = false;
                MinimizeBox = false;
                BackColor = Color.White;
                AutoScaleMode = AutoScaleMode.None;
                Size = new Size(300, 150);
                MinimumSize = Size;
                MaximumSize = Size;

                var label = new Label
                {
                    Text = labelText,
                    Font = new Font("Segoe UI", 11),
                    ForeColor = Color.FromArgb(51, 51, 51),
                    Location = new Point(20, 20),
                    Size = new Size(260, 20)
                };
                Controls.Add(label);

                _nameTextBox = new TextBox
                {
                    Font = new Font("Segoe UI", 11),
                    Location = new Point(20, 45),
                    Size = new Size(260, 25),
                    BorderStyle = BorderStyle.FixedSingle
                };
                Controls.Add(_nameTextBox);

                var okButton = new Button
                {
                    Text = "确定",
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    Size = new Size(100, 35),
                    Location = new Point(45, 85),
                    BackColor = Color.FromArgb(39, 174, 96),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                okButton.Click += OkButton_Click;
                Controls.Add(okButton);

                var cancelButton = new Button
                {
                    Text = "取消",
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    Size = new Size(100, 35),
                    Location = new Point(155, 85),
                    BackColor = Color.FromArgb(149, 165, 166),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                cancelButton.Click += CancelButton_Click;
                Controls.Add(cancelButton);
            }

            private void OkButton_Click(object sender, EventArgs e)
            {
                InputName = _nameTextBox.Text.Trim();
                if (string.IsNullOrEmpty(InputName))
                {
                    MessageBox.Show("请输入名称", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                DialogResult = DialogResult.OK;
                Close();
            }

            private void CancelButton_Click(object sender, EventArgs e)
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }

        private class ProjectPresetInputForm : Form
        {
            public string PresetName { get; private set; }
            public string ItemName { get; private set; }
            public string DisplayText { get; private set; }
            public string Duration { get; private set; }
            public string Type { get; private set; }

            private TextBox _presetNameTextBox;
            private TextBox _itemNameTextBox;
            private TextBox _displayTextBox;
            private TextBox _durationTextBox;
            private ComboBox _typeComboBox;

            public ProjectPresetInputForm()
            {
                Text = "保存项目预设";
                StartPosition = FormStartPosition.CenterParent;
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MaximizeBox = false;
                MinimizeBox = false;
                BackColor = Color.White;
                AutoScaleMode = AutoScaleMode.None;
                Size = new Size(350, 320);
                MinimumSize = Size;
                MaximumSize = Size;

                InitializeControls();
            }

            private void InitializeControls()
            {
                int labelWidth = 70;
                int controlWidth = 220;
                int y = 20;
                int spacing = 35;

                var presetLabel = new Label
                {
                    Text = "预设名称:",
                    Font = new Font("Segoe UI", 11),
                    ForeColor = Color.FromArgb(51, 51, 51),
                    Location = new Point(20, y),
                    Size = new Size(labelWidth, 20)
                };
                Controls.Add(presetLabel);

                _presetNameTextBox = new TextBox
                {
                    Font = new Font("Segoe UI", 11),
                    Location = new Point(95, y),
                    Size = new Size(controlWidth, 25),
                    BorderStyle = BorderStyle.FixedSingle
                };
                Controls.Add(_presetNameTextBox);
                y += spacing;

                var nameLabel = new Label
                {
                    Text = "项目名称:",
                    Font = new Font("Segoe UI", 11),
                    ForeColor = Color.FromArgb(51, 51, 51),
                    Location = new Point(20, y),
                    Size = new Size(labelWidth, 20)
                };
                Controls.Add(nameLabel);

                _itemNameTextBox = new TextBox
                {
                    Font = new Font("Segoe UI", 11),
                    Location = new Point(95, y),
                    Size = new Size(controlWidth, 25),
                    BorderStyle = BorderStyle.FixedSingle
                };
                Controls.Add(_itemNameTextBox);
                y += spacing;

                var displayLabel = new Label
                {
                    Text = "显示文字:",
                    Font = new Font("Segoe UI", 11),
                    ForeColor = Color.FromArgb(51, 51, 51),
                    Location = new Point(20, y),
                    Size = new Size(labelWidth, 20)
                };
                Controls.Add(displayLabel);

                _displayTextBox = new TextBox
                {
                    Font = new Font("Segoe UI", 11),
                    Location = new Point(95, y),
                    Size = new Size(controlWidth, 25),
                    BorderStyle = BorderStyle.FixedSingle
                };
                Controls.Add(_displayTextBox);
                y += spacing;

                var durationLabel = new Label
                {
                    Text = "时长(分):",
                    Font = new Font("Segoe UI", 11),
                    ForeColor = Color.FromArgb(51, 51, 51),
                    Location = new Point(20, y),
                    Size = new Size(labelWidth, 20)
                };
                Controls.Add(durationLabel);

                _durationTextBox = new TextBox
                {
                    Font = new Font("Segoe UI", 11),
                    Location = new Point(95, y),
                    Size = new Size(controlWidth, 25),
                    BorderStyle = BorderStyle.FixedSingle
                };
                Controls.Add(_durationTextBox);
                y += spacing;

                var typeLabel = new Label
                {
                    Text = "性质:",
                    Font = new Font("Segoe UI", 11),
                    ForeColor = Color.FromArgb(51, 51, 51),
                    Location = new Point(20, y),
                    Size = new Size(labelWidth, 20)
                };
                Controls.Add(typeLabel);

                _typeComboBox = new ComboBox
                {
                    Font = new Font("Segoe UI", 11),
                    Location = new Point(95, y),
                    Size = new Size(controlWidth, 25),
                    DropDownStyle = ComboBoxStyle.DropDownList
                };
                _typeComboBox.Items.AddRange(new[] { "忙碌", "放松" });
                _typeComboBox.SelectedIndex = 0;
                Controls.Add(_typeComboBox);
                y += 35;

                var okButton = new Button
                {
                    Text = "确定",
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    Size = new Size(120, 40),
                    Location = new Point(55, y),
                    BackColor = Color.FromArgb(39, 174, 96),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                okButton.Click += OkButton_Click;
                Controls.Add(okButton);

                var cancelButton = new Button
                {
                    Text = "取消",
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    Size = new Size(120, 40),
                    Location = new Point(175, y),
                    BackColor = Color.FromArgb(149, 165, 166),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                cancelButton.Click += CancelButton_Click;
                Controls.Add(cancelButton);
            }

            private void OkButton_Click(object sender, EventArgs e)
            {
                PresetName = _presetNameTextBox.Text.Trim();
                ItemName = _itemNameTextBox.Text.Trim();
                DisplayText = _displayTextBox.Text.Trim();
                Duration = _durationTextBox.Text.Trim();
                Type = _typeComboBox.SelectedItem.ToString();

                if (string.IsNullOrEmpty(PresetName))
                {
                    MessageBox.Show("请输入预设名称", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (string.IsNullOrEmpty(ItemName))
                {
                    MessageBox.Show("请输入项目名称", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int duration;
                if (!int.TryParse(Duration, out duration) || duration < 1 || duration > 99)
                {
                    MessageBox.Show("时长必须在1-99之间", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DialogResult = DialogResult.OK;
                Close();
            }

            private void CancelButton_Click(object sender, EventArgs e)
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }

        private class ProjectDetailForm : Form
        {
            private Label _nameLabel;
            private TextBox _nameTextBox;
            private Label _displayLabel;
            private TextBox _displayTextBox;
            private Label _durationLabel;
            private TextBox _durationTextBox;
            private Label _typeLabel;
            private ComboBox _typeComboBox;
            private Button _okButton;
            private Button _cancelButton;
            private Button _deleteButton;
            private ProjectItem _item;
            private ListBox _parentListBox;
            private int _itemIndex;

            public ProjectDetailForm(ProjectItem item, ListBox parentListBox, int itemIndex)
            {
                _item = item;
                _parentListBox = parentListBox;
                _itemIndex = itemIndex;
                Text = "项目详情";
                StartPosition = FormStartPosition.CenterParent;
                FormBorderStyle = FormBorderStyle.FixedDialog;
                MaximizeBox = false;
                MinimizeBox = false;
                BackColor = Color.White;
                AutoScaleMode = AutoScaleMode.None;
                Size = new Size(320, 330);
                MinimumSize = Size;
                MaximumSize = Size;

                InitializeControls();
            }

            private void InitializeControls()
            {
                int labelWidth = 70;
                int controlWidth = 200;
                int y = 20;
                int spacing = 35;

                _nameLabel = new Label
                {
                    Text = "名称:",
                    Font = new Font("Segoe UI", 11),
                    ForeColor = Color.FromArgb(51, 51, 51),
                    Location = new Point(20, y),
                    Size = new Size(labelWidth, 20)
                };
                Controls.Add(_nameLabel);

                _nameTextBox = new TextBox
                {
                    Text = _item.Name,
                    Font = new Font("Segoe UI", 11),
                    Location = new Point(95, y),
                    Size = new Size(controlWidth, 25),
                    BorderStyle = BorderStyle.FixedSingle
                };
                Controls.Add(_nameTextBox);
                y += spacing;

                _displayLabel = new Label
                {
                    Text = "显示:",
                    Font = new Font("Segoe UI", 11),
                    ForeColor = Color.FromArgb(51, 51, 51),
                    Location = new Point(20, y),
                    Size = new Size(labelWidth, 20)
                };
                Controls.Add(_displayLabel);

                _displayTextBox = new TextBox
                {
                    Text = _item.DisplayText,
                    Font = new Font("Segoe UI", 11),
                    Location = new Point(95, y),
                    Size = new Size(controlWidth, 25),
                    BorderStyle = BorderStyle.FixedSingle
                };
                Controls.Add(_displayTextBox);
                y += spacing;

                _durationLabel = new Label
                {
                    Text = "时长(分):",
                    Font = new Font("Segoe UI", 11),
                    ForeColor = Color.FromArgb(51, 51, 51),
                    Location = new Point(20, y),
                    Size = new Size(labelWidth, 20)
                };
                Controls.Add(_durationLabel);

                _durationTextBox = new TextBox
                {
                    Text = _item.Duration,
                    Font = new Font("Segoe UI", 11),
                    Location = new Point(95, y),
                    Size = new Size(controlWidth, 25),
                    BorderStyle = BorderStyle.FixedSingle
                };
                Controls.Add(_durationTextBox);
                y += spacing;

                _typeLabel = new Label
                {
                    Text = "性质:",
                    Font = new Font("Segoe UI", 11),
                    ForeColor = Color.FromArgb(51, 51, 51),
                    Location = new Point(20, y),
                    Size = new Size(labelWidth, 20)
                };
                Controls.Add(_typeLabel);

                _typeComboBox = new ComboBox
                {
                    Font = new Font("Segoe UI", 11),
                    Location = new Point(95, y),
                    Size = new Size(controlWidth, 25),
                    DropDownStyle = ComboBoxStyle.DropDownList
                };
                _typeComboBox.Items.AddRange(new[] { "忙碌", "放松" });
                _typeComboBox.SelectedIndex = _item.Type == "忙碌" ? 0 : 1;
                Controls.Add(_typeComboBox);
                y += 40;

                _okButton = new Button
                {
                    Text = "确定",
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    Size = new Size(80, 40),
                    Location = new Point(20, y),
                    BackColor = Color.FromArgb(39, 174, 96),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                _okButton.Click += OkButton_Click;
                Controls.Add(_okButton);

                _cancelButton = new Button
                {
                    Text = "取消",
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    Size = new Size(80, 40),
                    Location = new Point(115, y),
                    BackColor = Color.FromArgb(149, 165, 166),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                _cancelButton.Click += CancelButton_Click;
                Controls.Add(_cancelButton);

                _deleteButton = new Button
                {
                    Text = "删除",
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    Size = new Size(80, 40),
                    Location = new Point(210, y),
                    BackColor = Color.FromArgb(192, 57, 43),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                _deleteButton.Click += DeleteButton_Click;
                Controls.Add(_deleteButton);
            }

            private void OkButton_Click(object sender, EventArgs e)
            {
                int duration;
                if (!int.TryParse(_durationTextBox.Text, out duration) || duration < 1 || duration > 99)
                {
                    MessageBox.Show("时长必须在1-99之间", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _item.Name = _nameTextBox.Text;
                _item.DisplayText = _displayTextBox.Text;
                _item.Duration = duration.ToString();
                _item.Type = _typeComboBox.SelectedItem.ToString();
                DialogResult = DialogResult.OK;
                Close();
            }

            private void CancelButton_Click(object sender, EventArgs e)
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }

            private void DeleteButton_Click(object sender, EventArgs e)
            {
                var result = MessageBox.Show("确定要删除此项目吗？", "确认删除", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    if (_parentListBox != null && _itemIndex >= 0 && _itemIndex < _parentListBox.Items.Count)
                    {
                        _parentListBox.Items.RemoveAt(_itemIndex);
                    }
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else if (result == DialogResult.Cancel)
                {
                    DialogResult = DialogResult.Cancel;
                    Close();
                }
            }
        }
    }

    public class MusicPlayerForm : Form
    {
        private List<string> _musicFiles;
        private string _musicFolder;
        private NAudio.Wave.WaveOutEvent _waveOut;
        private NAudio.Wave.AudioFileReader _audioFileReader;
        private int _currentIndex = -1;
        private bool _isPlaying = false;
        private PlayMode _playMode = PlayMode.Sequential;
        private bool _isAlwaysOnTop = false;
        private bool _isDraggingProgress = false;
        private double _currentPosition = 0;
        private double _totalDuration = 0;
        private int _selectedDeviceNumber = -1;

        private Button _topMostButton;
        private PictureBox _coverPictureBox;
        private Label _songTitleLabel;
        private Label _artistLabel;
        private Button _prevButton;
        private Button _playPauseButton;
        private Button _nextButton;
        private TrackBar _volumeTrackBar;
        private Button _muteButton;
        private Panel _playlistPanel;
        private ListBox _playlistListBox;
        private Button _importFolderButton;
        private Button _importFilesButton;
        private Button _deleteSongButton;
        private Button _clearPlaylistButton;
        private Button _speakerButton;
        private Button _modeButton;
        private TrackBar _progressTrackBar;
        private Label _timeLabel;
        private Timer _progressTimer;

        public enum PlayMode
        {
            Sequential,
            Repeat,
            Shuffle
        }

        public MusicPlayerForm(List<string> musicFiles, string musicFolder)
        {
            _musicFiles = musicFiles ?? new List<string>();
            _musicFolder = musicFolder;

            InitializeForm();
            InitializeControls();
            LoadPlaylist();
        }

        private void InitializeForm()
        {
            Text = "音乐播放器";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(30, 30, 30);
            AutoScaleMode = AutoScaleMode.None;
            Size = new Size(635, 565);
            MinimumSize = new Size(500, 460);
        }

        private void InitializeControls()
        {
            _topMostButton = new Button
            {
                Text = "📌",
                Font = new Font("Segoe UI", 12),
                Size = new Size(37, 28),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(12, 12),
                TextAlign = ContentAlignment.MiddleCenter
            };
            _topMostButton.Click += TopMostButton_Click;
            _topMostButton.MouseEnter += (s, e) => ((Button)s).BackColor = Color.FromArgb(80, 80, 80);
            _topMostButton.MouseLeave += (s, e) => ((Button)s).BackColor = _isAlwaysOnTop ? Color.FromArgb(52, 152, 219) : Color.FromArgb(60, 60, 60);

            _coverPictureBox = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.FromArgb(60, 60, 60),
                Location = new Point(12, 50),
                Size = new Size(347, 313)
            };
            SetDefaultCover();

            _songTitleLabel = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Location = new Point(-1000, 0),
                Size = new Size(0, 0)
            };

            _artistLabel = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.Gray,
                BackColor = Color.Transparent,
                Location = new Point(-1000, 0),
                Size = new Size(0, 0)
            };

            _modeButton = new Button
            {
                Text = "🔁",
                Font = new Font("Segoe UI", 12),
                Size = new Size(43, 28),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(12, 484),
                TextAlign = ContentAlignment.MiddleCenter
            };
            _modeButton.Click += ModeButton_Click;
            _modeButton.MouseEnter += (s, e) => ((Button)s).BackColor = Color.FromArgb(80, 80, 80);
            _modeButton.MouseLeave += (s, e) => ((Button)s).BackColor = Color.FromArgb(60, 60, 60);

            _progressTrackBar = new TrackBar
            {
                Minimum = 0,
                Maximum = 100,
                Value = 0,
                TickStyle = TickStyle.None,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.FromArgb(52, 152, 219),
                Location = new Point(12, 363),
                Size = new Size(347, 8)
            };
            _progressTrackBar.Scroll += ProgressTrackBar_Scroll;
            _progressTrackBar.MouseDown += ProgressTrackBar_MouseDown;
            _progressTrackBar.MouseUp += ProgressTrackBar_MouseUp;

            _timeLabel = new Label
            {
                Text = "00:00 / 00:00",
                Font = new Font("Segoe UI", 7),
                ForeColor = Color.Gray,
                BackColor = Color.Transparent,
                Location = new Point(311, 363),
                Size = new Size(48, 8),
                TextAlign = ContentAlignment.MiddleRight
            };

            _prevButton = new Button
            {
                Text = "⏮",
                Font = new Font("Segoe UI", 11),
                Size = new Size(50, 33),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(99, 385),
                TextAlign = ContentAlignment.MiddleCenter
            };
            _prevButton.Click += PrevButton_Click;
            _prevButton.MouseEnter += (s, e) => ((Button)s).BackColor = Color.FromArgb(41, 128, 185);
            _prevButton.MouseLeave += (s, e) => ((Button)s).BackColor = Color.FromArgb(52, 152, 219);

            _playPauseButton = new Button
            {
                Text = "▶",
                Font = new Font("Segoe UI", 16),
                Size = new Size(62, 44),
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(161, 380),
                TextAlign = ContentAlignment.MiddleCenter
            };
            _playPauseButton.Click += PlayPauseButton_Click;
            _playPauseButton.MouseEnter += (s, e) => ((Button)s).BackColor = Color.FromArgb(34, 153, 84);
            _playPauseButton.MouseLeave += (s, e) => ((Button)s).BackColor = Color.FromArgb(39, 174, 96);

            _nextButton = new Button
            {
                Text = "⏭",
                Font = new Font("Segoe UI", 11),
                Size = new Size(50, 33),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(236, 385),
                TextAlign = ContentAlignment.MiddleCenter
            };
            _nextButton.Click += NextButton_Click;
            _nextButton.MouseEnter += (s, e) => ((Button)s).BackColor = Color.FromArgb(41, 128, 185);
            _nextButton.MouseLeave += (s, e) => ((Button)s).BackColor = Color.FromArgb(52, 152, 219);

            _volumeTrackBar = new TrackBar
            {
                Minimum = 0,
                Maximum = 100,
                Value = 70,
                TickStyle = TickStyle.None,
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.FromArgb(52, 152, 219),
                Location = new Point(12, 435),
                Size = new Size(310, 8)
            };

            _volumeTrackBar.Scroll += VolumeTrackBar_Scroll;

            _muteButton = new Button
            {
                Text = "🔇",
                Font = new Font("Segoe UI", 10),
                Size = new Size(35, 35),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(334, 424),
                TextAlign = ContentAlignment.MiddleCenter
            };
            _muteButton.Click += MuteButton_Click;
            _muteButton.MouseEnter += (s, e) => ((Button)s).BackColor = Color.FromArgb(80, 80, 80);
            _muteButton.MouseLeave += (s, e) => ((Button)s).BackColor = Color.FromArgb(60, 60, 60);

            _speakerButton = new Button
            {
                Text = "音频输出调节",
                Font = new Font("Segoe UI", 8),
                Size = new Size(248, 28),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(69, 484),
                TextAlign = ContentAlignment.MiddleCenter
            };
            _speakerButton.Click += SpeakerButton_Click;
            _speakerButton.MouseEnter += (s, e) => ((Button)s).BackColor = Color.FromArgb(80, 80, 80);
            _speakerButton.MouseLeave += (s, e) => ((Button)s).BackColor = Color.FromArgb(60, 60, 60);

            _playlistPanel = new Panel
            {
                BackColor = Color.FromArgb(40, 40, 40),
                Location = new Point(397, 50),
                Size = new Size(226, 462)
            };

            _importFolderButton = new Button
            {
                Text = "导入歌单",
                Font = new Font("Segoe UI", 8),
                Size = new Size(99, 28),
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(0, 5),
                TextAlign = ContentAlignment.MiddleCenter
            };
            _importFolderButton.Click += ImportFolderButton_Click;
            _importFolderButton.MouseEnter += (s, e) => ((Button)s).BackColor = Color.FromArgb(34, 153, 84);
            _importFolderButton.MouseLeave += (s, e) => ((Button)s).BackColor = Color.FromArgb(39, 174, 96);

            _importFilesButton = new Button
            {
                Text = "导入单曲",
                Font = new Font("Segoe UI", 8),
                Size = new Size(99, 28),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(103, 5),
                TextAlign = ContentAlignment.MiddleCenter
            };
            _importFilesButton.Click += ImportFilesButton_Click;
            _importFilesButton.MouseEnter += (s, e) => ((Button)s).BackColor = Color.FromArgb(41, 128, 185);
            _importFilesButton.MouseLeave += (s, e) => ((Button)s).BackColor = Color.FromArgb(52, 152, 219);

            _deleteSongButton = new Button
            {
                Text = "删除选中",
                Font = new Font("Segoe UI", 8),
                Size = new Size(99, 28),
                BackColor = Color.FromArgb(192, 57, 43),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(0, 428),
                TextAlign = ContentAlignment.MiddleCenter
            };
            _deleteSongButton.Click += DeleteSongButton_Click;
            _deleteSongButton.MouseEnter += (s, e) => ((Button)s).BackColor = Color.FromArgb(170, 50, 38);
            _deleteSongButton.MouseLeave += (s, e) => ((Button)s).BackColor = Color.FromArgb(192, 57, 43);

            _clearPlaylistButton = new Button
            {
                Text = "清空列表",
                Font = new Font("Segoe UI", 8),
                Size = new Size(99, 28),
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(103, 428),
                TextAlign = ContentAlignment.MiddleCenter
            };
            _clearPlaylistButton.Click += ClearPlaylistButton_Click;
            _clearPlaylistButton.MouseEnter += (s, e) => ((Button)s).BackColor = Color.FromArgb(178, 190, 195);
            _clearPlaylistButton.MouseLeave += (s, e) => ((Button)s).BackColor = Color.FromArgb(149, 165, 166);

            _playlistListBox = new ListBox
            {
                Location = new Point(0, 40),
                Size = new Size(216, 378),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 8),
                SelectionMode = SelectionMode.One
            };
            _playlistListBox.DoubleClick += PlaylistListBox_DoubleClick;
            _playlistListBox.SelectedIndexChanged += PlaylistListBox_SelectedIndexChanged;

            _playlistPanel.Controls.Add(_importFolderButton);
            _playlistPanel.Controls.Add(_importFilesButton);
            _playlistPanel.Controls.Add(_deleteSongButton);
            _playlistPanel.Controls.Add(_clearPlaylistButton);
            _playlistPanel.Controls.Add(_playlistListBox);

            Controls.Add(_topMostButton);
            Controls.Add(_coverPictureBox);
            Controls.Add(_songTitleLabel);
            Controls.Add(_artistLabel);
            Controls.Add(_modeButton);
            Controls.Add(_prevButton);
            Controls.Add(_playPauseButton);
            Controls.Add(_nextButton);
            Controls.Add(_progressTrackBar);
            Controls.Add(_timeLabel);
            Controls.Add(_volumeTrackBar);
            Controls.Add(_muteButton);
            Controls.Add(_speakerButton);
            Controls.Add(_playlistPanel);

            _progressTimer = new Timer { Interval = 100 };
            _progressTimer.Tick += ProgressTimer_Tick;
            _progressTimer.Start();
        }

        private void SetDefaultCover()
        {
            using (var bmp = new Bitmap(347, 313))
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.FromArgb(60, 60, 60));
                using (var font = new Font("Segoe UI", 16, FontStyle.Bold))
                {
                    var text = "未播放";
                    var size = g.MeasureString(text, font);
                    var x = (347 - size.Width) / 2;
                    var y = (313 - size.Height) / 2 - 10;
                    g.DrawString(text, font, Brushes.White, x, y);
                }
                using (var font2 = new Font("Segoe UI", 10))
                {
                    var text2 = "点击播放列表选择歌曲";
                    var size2 = g.MeasureString(text2, font2);
                    var x2 = (347 - size2.Width) / 2;
                    var y2 = (313 / 2) + 15;
                    g.DrawString(text2, font2, Brushes.Gray, x2, y2);
                }
                _coverPictureBox.Image = (Bitmap)bmp.Clone();
            }
        }

        private void UpdateCoverDisplay(string title, string status)
        {
            using (var bmp = new Bitmap(347, 313))
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(Color.FromArgb(60, 60, 60));
                using (var font = new Font("Segoe UI", 14, FontStyle.Bold))
                {
                    var text = string.IsNullOrEmpty(title) ? "未播放" : title;
                    var size = g.MeasureString(text, font);
                    var x = (347 - size.Width) / 2;
                    var y = (313 - size.Height) / 2 - 10;
                    g.DrawString(text, font, Brushes.White, x, y);
                }
                using (var font2 = new Font("Segoe UI", 10))
                {
                    var text2 = string.IsNullOrEmpty(status) ? "" : status;
                    if (!string.IsNullOrEmpty(text2))
                    {
                        var size2 = g.MeasureString(text2, font2);
                        var x2 = (347 - size2.Width) / 2;
                        var y2 = (313 / 2) + 15;
                        g.DrawString(text2, font2, Brushes.Gray, x2, y2);
                    }
                }
                _coverPictureBox.Image = (Bitmap)bmp.Clone();
            }
        }

        private void LoadPlaylist()
        {
            _playlistListBox.Items.Clear();
            foreach (var file in _musicFiles)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                _playlistListBox.Items.Add(fileName);
            }
        }

        private void TopMostButton_Click(object sender, EventArgs e)
        {
            _isAlwaysOnTop = !_isAlwaysOnTop;
            TopMost = _isAlwaysOnTop;
            _topMostButton.BackColor = _isAlwaysOnTop ? Color.FromArgb(52, 152, 219) : Color.FromArgb(60, 60, 60);
            _topMostButton.Text = _isAlwaysOnTop ? "🔓" : "🔒";
        }

        private void ModeButton_Click(object sender, EventArgs e)
        {
            switch (_playMode)
            {
                case PlayMode.Sequential:
                    _playMode = PlayMode.Repeat;
                    _modeButton.Text = "🔂";
                    break;
                case PlayMode.Repeat:
                    _playMode = PlayMode.Shuffle;
                    _modeButton.Text = "🔀";
                    break;
                case PlayMode.Shuffle:
                    _playMode = PlayMode.Sequential;
                    _modeButton.Text = "🔁";
                    break;
            }
        }

        private void PrevButton_Click(object sender, EventArgs e)
        {
            if (_musicFiles.Count == 0) return;

            StopPlayer();

            if (_playMode == PlayMode.Shuffle)
            {
                Random rnd = new Random();
                _currentIndex = rnd.Next(_musicFiles.Count);
            }
            else
            {
                _currentIndex--;
                if (_currentIndex < 0)
                    _currentIndex = _musicFiles.Count - 1;
            }

            PlayCurrentSong();
        }

        private void PlayPauseButton_Click(object sender, EventArgs e)
        {
            if (_isPlaying)
            {
                Pause();
            }
            else
            {
                if (_audioFileReader != null && _waveOut != null && 
                    _waveOut.PlaybackState == NAudio.Wave.PlaybackState.Paused)
                {
                    Resume();
                }
                else
                {
                    if (_currentIndex < 0 && _musicFiles.Count > 0)
                    {
                        _currentIndex = 0;
                    }
                    PlayCurrentSong();
                }
            }
        }

        private void NextButton_Click(object sender, EventArgs e)
        {
            if (_musicFiles.Count == 0) return;

            StopPlayer();

            if (_playMode == PlayMode.Shuffle)
            {
                Random rnd = new Random();
                _currentIndex = rnd.Next(_musicFiles.Count);
            }
            else if (_playMode == PlayMode.Repeat)
            {
            }
            else
            {
                _currentIndex++;
                if (_currentIndex >= _musicFiles.Count)
                    _currentIndex = 0;
            }

            PlayCurrentSong();
        }

        private void PlayCurrentSong()
        {
            if (_currentIndex < 0 || _currentIndex >= _musicFiles.Count) return;

            try
            {
                StopPlayer();

                _audioFileReader = new NAudio.Wave.AudioFileReader(_musicFiles[_currentIndex]);
                _waveOut = new NAudio.Wave.WaveOutEvent();
                if (_selectedDeviceNumber >= 0)
                {
                    _waveOut.DeviceNumber = _selectedDeviceNumber;
                }
                _waveOut.Init(_audioFileReader);
                _waveOut.Play();
                _waveOut.PlaybackStopped += WaveOut_PlaybackStopped;

                _totalDuration = _audioFileReader.TotalTime.TotalSeconds;
                _currentPosition = 0;

                _isPlaying = true;
                _playPauseButton.Text = "⏸";
                _playlistListBox.SelectedIndex = _currentIndex;

                string fileName = Path.GetFileNameWithoutExtension(_musicFiles[_currentIndex]);
                UpdateCoverDisplay(fileName, "正在播放");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"播放失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Pause()
        {
            if (_waveOut != null)
            {
                _waveOut.Pause();
            }
            _isPlaying = false;
            _playPauseButton.Text = "▶";
            string title = _songTitleLabel.Text;
            UpdateCoverDisplay(title, "已暂停");
        }

        private void Resume()
        {
            if (_waveOut != null)
            {
                _waveOut.Play();
            }
            _isPlaying = true;
            _playPauseButton.Text = "⏸";
            string title = _songTitleLabel.Text;
            UpdateCoverDisplay(title, "正在播放");
        }

        private void StopPlayer()
        {
            if (_waveOut != null)
            {
                _waveOut.PlaybackStopped -= WaveOut_PlaybackStopped;
                _waveOut.Stop();
                _waveOut.Dispose();
                _waveOut = null;
            }
            if (_audioFileReader != null)
            {
                _audioFileReader.Dispose();
                _audioFileReader = null;
            }
            _isPlaying = false;
        }

        private void WaveOut_PlaybackStopped(object sender, EventArgs e)
        {
            if (_playMode == PlayMode.Repeat && _currentIndex >= 0)
            {
                PlayCurrentSong();
            }
            else if (_playMode != PlayMode.Repeat && _musicFiles.Count > 1)
            {
                NextButton_Click(null, null);
            }
            else
            {
                _isPlaying = false;
                _playPauseButton.Text = "▶";
                string title = _songTitleLabel.Text;
                UpdateCoverDisplay(title, "播放完毕");
            }
        }

        private void ProgressTrackBar_Scroll(object sender, EventArgs e)
        {
            if (_isDraggingProgress && _audioFileReader != null && _totalDuration > 0)
            {
                double position = (double)_progressTrackBar.Value / 100 * _totalDuration;
                _audioFileReader.CurrentTime = TimeSpan.FromSeconds(position);
                _currentPosition = position;
            }
        }

        private void ProgressTrackBar_MouseDown(object sender, MouseEventArgs e)
        {
            _isDraggingProgress = true;
        }

        private void ProgressTrackBar_MouseUp(object sender, MouseEventArgs e)
        {
            _isDraggingProgress = false;
        }

        private void ProgressTimer_Tick(object sender, EventArgs e)
        {
            if (_audioFileReader != null && !_isDraggingProgress)
            {
                _currentPosition = _audioFileReader.CurrentTime.TotalSeconds;

                if (_totalDuration > 0)
                {
                    _progressTrackBar.Value = (int)(_currentPosition / _totalDuration * 100);
                    
                    TimeSpan current = TimeSpan.FromSeconds(_currentPosition);
                    TimeSpan total = TimeSpan.FromSeconds(_totalDuration);
                    _timeLabel.Text = $"{current.Minutes:D2}:{current.Seconds:D2} / {total.Minutes:D2}:{total.Seconds:D2}";
                }
            }
        }

        private void VolumeTrackBar_Scroll(object sender, EventArgs e)
        {
            if (_waveOut != null)
            {
                _waveOut.Volume = _volumeTrackBar.Value / 100f;
            }
            _muteButton.Text = _volumeTrackBar.Value > 0 ? "🔊" : "🔇";
        }

        private void MuteButton_Click(object sender, EventArgs e)
        {
            if (_volumeTrackBar.Value > 0)
            {
                if (_waveOut != null)
                {
                    _waveOut.Volume = 0;
                }
                _volumeTrackBar.Value = 0;
                _muteButton.Text = "🔇";
            }
            else
            {
                if (_waveOut != null)
                {
                    _waveOut.Volume = 0.7f;
                }
                _volumeTrackBar.Value = 70;
                _muteButton.Text = "🔊";
            }
        }

        private void SpeakerButton_Click(object sender, EventArgs e)
        {
            var deviceMenu = new ContextMenuStrip();
            deviceMenu.Items.Add("默认设备", null, (s, ev) => SelectAudioDevice(-1));

            int deviceCount = NAudio.Wave.WaveOut.DeviceCount;
            for (int i = 0; i < deviceCount; i++)
            {
                var caps = NAudio.Wave.WaveOut.GetCapabilities(i);
                string deviceName = caps.ProductName;
                var item = deviceMenu.Items.Add(deviceName, null, (s, ev) => SelectAudioDevice(i));
            }

            var locate = _speakerButton.PointToScreen(new Point(0, _speakerButton.Height));
            deviceMenu.Show(locate);
        }

        private void SelectAudioDevice(int deviceNumber)
        {
            _selectedDeviceNumber = deviceNumber;

            string deviceName = deviceNumber == -1 ? "默认设备" : NAudio.Wave.WaveOut.GetCapabilities(deviceNumber).ProductName;
            _speakerButton.Text = deviceName.Length > 15 ? deviceName.Substring(0, 12) + "..." : deviceName;

            if (_waveOut != null && _audioFileReader != null)
            {
                bool wasPlaying = _isPlaying && _waveOut.PlaybackState == NAudio.Wave.PlaybackState.Playing;
                double currentPos = 0;

                if (wasPlaying)
                {
                    currentPos = _audioFileReader.CurrentTime.TotalSeconds;
                }

                _waveOut.Stop();
                _waveOut.Dispose();
                _waveOut = null;

                _waveOut = new NAudio.Wave.WaveOutEvent();
                if (deviceNumber >= 0)
                {
                    _waveOut.DeviceNumber = deviceNumber;
                }
                _waveOut.Init(_audioFileReader);
                _waveOut.PlaybackStopped += WaveOut_PlaybackStopped;

                if (wasPlaying)
                {
                    _audioFileReader.CurrentTime = TimeSpan.FromSeconds(currentPos);
                    _waveOut.Play();
                }
            }
        }

        private void PlaylistListBox_DoubleClick(object sender, EventArgs e)
        {
            if (_playlistListBox.SelectedIndex >= 0)
            {
                StopPlayer();
                _currentIndex = _playlistListBox.SelectedIndex;
                PlayCurrentSong();
            }
        }

        private void PlaylistListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            _currentIndex = _playlistListBox.SelectedIndex;
        }

        private void ImportFolderButton_Click(object sender, EventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    string[] audioFiles = Directory.GetFiles(folderDialog.SelectedPath, "*.wav");
                    foreach (var file in audioFiles)
                    {
                        string destFile = Path.Combine(_musicFolder, Path.GetFileName(file));
                        if (!_musicFiles.Contains(destFile))
                        {
                            if (!File.Exists(destFile))
                                File.Copy(file, destFile);
                            _musicFiles.Add(destFile);
                            _playlistListBox.Items.Add(Path.GetFileNameWithoutExtension(file));
                        }
                    }
                }
            }
        }

        private void ImportFilesButton_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "音频文件|*.wav";
                openFileDialog.Multiselect = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (var file in openFileDialog.FileNames)
                    {
                        string destFile = Path.Combine(_musicFolder, Path.GetFileName(file));
                        if (!_musicFiles.Contains(destFile))
                        {
                            if (!File.Exists(destFile))
                                File.Copy(file, destFile);
                            _musicFiles.Add(destFile);
                            _playlistListBox.Items.Add(Path.GetFileNameWithoutExtension(file));
                        }
                    }
                }
            }
        }

        private void DeleteSongButton_Click(object sender, EventArgs e)
        {
            if (_playlistListBox.SelectedIndex >= 0)
            {
                string filePath = _musicFiles[_playlistListBox.SelectedIndex];
                
                if (_currentIndex == _playlistListBox.SelectedIndex && _isPlaying)
                {
                    StopPlayer();
                }

                _musicFiles.RemoveAt(_playlistListBox.SelectedIndex);
                _playlistListBox.Items.RemoveAt(_playlistListBox.SelectedIndex);

                try
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                }
                catch { }

                if (_currentIndex >= _musicFiles.Count)
                    _currentIndex = _musicFiles.Count - 1;
            }
        }

        private void ClearPlaylistButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("确定要清空播放列表吗？此操作将删除所有已导入的歌曲文件。", "确认清空", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                StopPlayer();
                
                foreach (var file in _musicFiles)
                {
                    try
                    {
                        if (File.Exists(file))
                            File.Delete(file);
                    }
                    catch { }
                }
                
                _musicFiles.Clear();
                _playlistListBox.Items.Clear();
                _currentIndex = -1;
                _songTitleLabel.Text = "未播放";
                UpdateCoverDisplay("", "播放列表已清空");
                _progressTrackBar.Value = 0;
                _timeLabel.Text = "00:00 / 00:00";
                _totalDuration = 0;
                _currentPosition = 0;
                SetDefaultCover();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            StopPlayer();
            base.OnFormClosing(e);
        }
    }



    public class ThemeManagerForm : Form
    {
        private FlowLayoutPanel _themeFlowPanel;
        private Button _addThemeButton;
        private Button _closeButton;
        private MainForm _mainForm;

        public ThemeManagerForm(MainForm mainForm = null)
        {
            _mainForm = mainForm;
            Text = "主题管理";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(40, 40, 40);
            AutoScaleMode = AutoScaleMode.None;
            Size = new Size(680, 520);
            MinimumSize = Size;
            MaximumSize = Size;

            var scrollPanel = new Panel
            {
                Location = new Point(20, 20),
                Size = new Size(640, 400),
                AutoScroll = true,
                BackColor = Color.FromArgb(50, 50, 50),
                BorderStyle = BorderStyle.FixedSingle
            };

            _themeFlowPanel = new FlowLayoutPanel
            {
                Location = new Point(10, 10),
                Size = new Size(610, 380),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowOnly,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                BackColor = Color.Transparent
            };

            LoadThemes();

            scrollPanel.Controls.Add(_themeFlowPanel);

            _addThemeButton = new Button
            {
                Text = "新增主题",
                Font = new Font("Segoe UI", 11),
                Size = new Size(130, 40),
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(20, 430)
            };
            _addThemeButton.Click += AddThemeButton_Click;

            _closeButton = new Button
            {
                Text = "关闭",
                Font = new Font("Segoe UI", 11),
                Size = new Size(130, 40),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel,
                Location = new Point(530, 430)
            };

            Controls.Add(scrollPanel);
            Controls.Add(_addThemeButton);
            Controls.Add(_closeButton);
        }

        private void LoadThemes()
        {
            _themeFlowPanel.Controls.Clear();
            string themesPath = Path.Combine(Application.StartupPath, "resourse");

            if (!Directory.Exists(themesPath))
                Directory.CreateDirectory(themesPath);

            string[] themeFolders = Directory.GetDirectories(themesPath);

            foreach (string themeFolder in themeFolders)
            {
                string themeName = Path.GetFileName(themeFolder);
                AddThemePanel(themeName);
            }
        }

        private void AddThemePanel(string themeName)
        {
            bool isDefaultTheme = themeName.Equals("theme1", StringComparison.OrdinalIgnoreCase) || 
                                   themeName.Equals("theme2", StringComparison.OrdinalIgnoreCase);
            
            Panel panel = new Panel
            {
                BackColor = Color.FromArgb(60, 60, 60),
                BorderStyle = BorderStyle.FixedSingle,
                Size = isDefaultTheme ? new Size(280, 320) : new Size(280, 360),
                Margin = new Padding(10)
            };

            PictureBox preview = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.StretchImage,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(10, 10),
                Size = new Size(260, 180)
            };
            LoadThemePreview(themeName, preview);

            Label label = new Label
            {
                Text = themeName,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(0, 200),
                Size = new Size(280, 30)
            };

            Button selectButton = new Button
            {
                Text = "选择此主题",
                Font = new Font("Segoe UI", 11),
                Size = new Size(140, 40),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(70, 250)
            };
            selectButton.Click += (s, e) => { SelectTheme(themeName); };

            panel.Controls.Add(preview);
            panel.Controls.Add(label);
            panel.Controls.Add(selectButton);

            if (!isDefaultTheme)
            {
                Button deleteButton = new Button
                {
                    Text = "删除",
                    Font = new Font("Segoe UI", 10),
                    Size = new Size(140, 35),
                    BackColor = Color.FromArgb(192, 57, 43),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Location = new Point(70, 300)
                };
                deleteButton.Click += (s, e) => { DeleteTheme(themeName); };

                panel.Controls.Add(deleteButton);
            }

            _themeFlowPanel.Controls.Add(panel);
        }

        private void DeleteTheme(string themeName)
        {
            if (MessageBox.Show($"确定要删除主题 \"{themeName}\" 吗？此操作无法撤销。", "确认删除", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    var themeManager = ThemeManager.Instance;
                    
                    themeManager.StopAllSounds();

                    ClearThemeImages();

                    if (themeManager.CurrentTheme?.Name.Equals(themeName, StringComparison.OrdinalIgnoreCase) == true)
                    {
                        var defaultTheme = themeManager.Themes.Find(t => t.Name.Equals("theme1") || t.Name.Equals("theme2"));
                        if (defaultTheme != null)
                        {
                            themeManager.SetTheme(defaultTheme.Name);
                        }
                    }

                    themeManager.RemoveTheme(themeName);

                    string themePath = Path.Combine(Application.StartupPath, "Resources", themeName);
                    if (Directory.Exists(themePath))
                    {
                        ForceDeleteDirectory(themePath);
                        LoadThemes();
                        MessageBox.Show("主题删除成功！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"删除主题失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ClearThemeImages()
        {
            foreach (Control ctrl in _themeFlowPanel.Controls)
            {
                if (ctrl is Panel panel)
                {
                    foreach (Control child in panel.Controls)
                    {
                        if (child is PictureBox pb)
                        {
                            if (pb.Image != null)
                            {
                                pb.Image.Dispose();
                                pb.Image = null;
                            }
                        }
                    }
                }
            }
            _themeFlowPanel.Controls.Clear();
        }

        private void ForceDeleteDirectory(string path)
        {
            string[] files = Directory.GetFiles(path);
            string[] dirs = Directory.GetDirectories(path);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                ForceDeleteDirectory(dir);
            }

            Directory.Delete(path);
        }

        private void AddThemeButton_Click(object sender, EventArgs e)
        {
            using (var addForm = new AddThemeForm())
            {
                if (addForm.ShowDialog() == DialogResult.OK)
                {
                    LoadThemes();
                }
            }
        }

        private void LoadThemePreview(string themeName, PictureBox preview)
        {
            try
            {
                string themePath = Path.Combine(Application.StartupPath, "Resources", themeName, "l");
                if (Directory.Exists(themePath))
                {
                    string[] images = Directory.GetFiles(themePath, "*.jpg");
                    if (images.Length > 0)
                    {
                        preview.Image = LoadImageWithoutLock(images[0]);
                        return;
                    }
                }
            }
            catch { }
        }

        private Image LoadImageWithoutLock(string filePath)
        {
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                return Image.FromStream(stream);
            }
        }

        private void SelectTheme(string themeName)
        {
            ThemeManager.Instance.SetTheme(themeName);
            DialogResult = DialogResult.OK;
        }
    }

    public class AddThemeForm : Form
    {
        private TextBox _themeNameTextBox;
        private TextBox _landscapePathTextBox;
        private TextBox _portraitPathTextBox;
        private TextBox[] _numberPaths;
        private TextBox _startSoundPathTextBox;
        private TextBox _endSoundPathTextBox;
        private string _editingThemeName;

        public AddThemeForm() : this(null) { }

        public AddThemeForm(string themeName)
        {
            _editingThemeName = themeName;
            Text = string.IsNullOrEmpty(themeName) ? "新增主题" : "编辑主题";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.FromArgb(40, 40, 40);
            AutoScaleMode = AutoScaleMode.None;
            Size = new Size(550, 620);
            MinimumSize = Size;
            MaximumSize = Size;

            int y = 15;

            var nameLabel = new Label
            {
                Text = "主题名称:",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.White,
                Location = new Point(15, y),
                AutoSize = true
            };
            y += 25;

            _themeNameTextBox = new TextBox
            {
                Font = new Font("Segoe UI", 11),
                Size = new Size(300, 25),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(120, y - 25),
                Text = _editingThemeName ?? "",
                ReadOnly = !string.IsNullOrEmpty(_editingThemeName)
            };

            var landscapeLabel = new Label
            {
                Text = "横屏图片:",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.White,
                Location = new Point(15, y += 30),
                AutoSize = true
            };

            _landscapePathTextBox = new TextBox
            {
                Font = new Font("Segoe UI", 11),
                Size = new Size(300, 25),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(120, y),
                ReadOnly = true
            };

            var landscapeButton = new Button
            {
                Text = "浏览",
                Font = new Font("Segoe UI", 10),
                Size = new Size(70, 25),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(430, y)
            };
            landscapeButton.Click += (s, e) => BrowseImage(_landscapePathTextBox);

            var portraitLabel = new Label
            {
                Text = "竖屏图片:",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.White,
                Location = new Point(15, y += 35),
                AutoSize = true
            };

            _portraitPathTextBox = new TextBox
            {
                Font = new Font("Segoe UI", 11),
                Size = new Size(300, 25),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(120, y),
                ReadOnly = true
            };

            var portraitButton = new Button
            {
                Text = "浏览",
                Font = new Font("Segoe UI", 10),
                Size = new Size(70, 25),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(430, y)
            };
            portraitButton.Click += (s, e) => BrowseImage(_portraitPathTextBox);

            var numberLabel = new Label
            {
                Text = "数字图片(0-9):",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.White,
                Location = new Point(15, y += 35),
                AutoSize = true
            };

            _numberPaths = new TextBox[10];
            for (int i = 0; i < 10; i++)
            {
                Label numLabel = new Label
                {
                    Text = $"{i}:",
                    Font = new Font("Segoe UI", 10),
                    ForeColor = Color.White,
                    Location = new Point(15, y += 28),
                    AutoSize = true
                };

                _numberPaths[i] = new TextBox
                {
                    Font = new Font("Segoe UI", 10),
                    Size = new Size(300, 22),
                    BackColor = Color.FromArgb(60, 60, 60),
                    ForeColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle,
                    Location = new Point(45, y),
                    ReadOnly = true
                };

                Button numButton = new Button
                {
                    Text = "浏览",
                    Font = new Font("Segoe UI", 9),
                    Size = new Size(60, 22),
                    BackColor = Color.FromArgb(52, 152, 219),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Location = new Point(355, y)
                };
                int index = i;
                numButton.Click += (s, e) => BrowseImage(_numberPaths[index]);

                Controls.Add(numLabel);
                Controls.Add(_numberPaths[i]);
                Controls.Add(numButton);
            }

            var startSoundLabel = new Label
            {
                Text = "开始音效:",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.White,
                Location = new Point(15, y += 30),
                AutoSize = true
            };

            _startSoundPathTextBox = new TextBox
            {
                Font = new Font("Segoe UI", 11),
                Size = new Size(300, 25),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(120, y),
                ReadOnly = true
            };

            var startSoundButton = new Button
            {
                Text = "浏览",
                Font = new Font("Segoe UI", 10),
                Size = new Size(70, 25),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(430, y)
            };
            startSoundButton.Click += (s, e) => BrowseSound(_startSoundPathTextBox);

            var endSoundLabel = new Label
            {
                Text = "结束音效:",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.White,
                Location = new Point(15, y += 35),
                AutoSize = true
            };

            _endSoundPathTextBox = new TextBox
            {
                Font = new Font("Segoe UI", 11),
                Size = new Size(300, 25),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(120, y),
                ReadOnly = true
            };

            var endSoundButton = new Button
            {
                Text = "浏览",
                Font = new Font("Segoe UI", 10),
                Size = new Size(70, 25),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(430, y)
            };
            endSoundButton.Click += (s, e) => BrowseSound(_endSoundPathTextBox);

            var okButton = new Button
            {
                Text = "确定",
                Font = new Font("Segoe UI", 11),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.OK,
                Location = new Point(190, y += 45)
            };
            okButton.Click += OkButton_Click;

            var cancelButton = new Button
            {
                Text = "取消",
                Font = new Font("Segoe UI", 11),
                Size = new Size(100, 35),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel,
                Location = new Point(310, y)
            };

            Controls.Add(nameLabel);
            Controls.Add(_themeNameTextBox);
            Controls.Add(landscapeLabel);
            Controls.Add(_landscapePathTextBox);
            Controls.Add(landscapeButton);
            Controls.Add(portraitLabel);
            Controls.Add(_portraitPathTextBox);
            Controls.Add(portraitButton);
            Controls.Add(numberLabel);
            Controls.Add(startSoundLabel);
            Controls.Add(_startSoundPathTextBox);
            Controls.Add(startSoundButton);
            Controls.Add(endSoundLabel);
            Controls.Add(_endSoundPathTextBox);
            Controls.Add(endSoundButton);
            Controls.Add(okButton);
            Controls.Add(cancelButton);

            if (!string.IsNullOrEmpty(_editingThemeName))
            {
                LoadExistingThemeFiles();
            }
        }

        private void LoadExistingThemeFiles()
        {
            string themePath = Path.Combine(Application.StartupPath, "Resources", _editingThemeName);
            
            string lPath = Path.Combine(themePath, "l");
            if (Directory.Exists(lPath))
            {
                string[] files = Directory.GetFiles(lPath, "*.jpg");
                if (files.Length > 0) _landscapePathTextBox.Text = files[0];
            }

            string vPath = Path.Combine(themePath, "v");
            if (Directory.Exists(vPath))
            {
                string[] files = Directory.GetFiles(vPath, "*.jpg");
                if (files.Length > 0) _portraitPathTextBox.Text = files[0];
            }

            string numPath = Path.Combine(themePath, "num");
            if (Directory.Exists(numPath))
            {
                for (int i = 0; i < 10; i++)
                {
                    string[] files = Directory.GetFiles(numPath, $"{i}.*");
                    if (files.Length > 0) _numberPaths[i].Text = files[0];
                }
            }

            string[] soundFiles = Directory.GetFiles(themePath, "start.*");
            if (soundFiles.Length > 0) _startSoundPathTextBox.Text = soundFiles[0];

            soundFiles = Directory.GetFiles(themePath, "end.*");
            if (soundFiles.Length > 0) _endSoundPathTextBox.Text = soundFiles[0];
        }

        private void BrowseImage(TextBox textBox)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "图片文件|*.jpg;*.jpeg;*.png;*.bmp";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    textBox.Text = ofd.FileName;
                }
            }
        }

        private void BrowseSound(TextBox textBox)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "音频文件|*.wav;*.mp3";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    textBox.Text = ofd.FileName;
                }
            }
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            string themeName = _themeNameTextBox.Text.Trim();
            if (string.IsNullOrEmpty(themeName))
            {
                MessageBox.Show("请输入主题名称", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(_landscapePathTextBox.Text))
            {
                MessageBox.Show("请选择横屏图片", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(_portraitPathTextBox.Text))
            {
                MessageBox.Show("请选择竖屏图片", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            for (int i = 0; i < 10; i++)
            {
                if (string.IsNullOrEmpty(_numberPaths[i].Text))
                {
                    MessageBox.Show($"请选择数字 {i} 的图片", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            if (string.IsNullOrEmpty(_startSoundPathTextBox.Text))
            {
                MessageBox.Show("请选择开始音效", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (string.IsNullOrEmpty(_endSoundPathTextBox.Text))
            {
                MessageBox.Show("请选择结束音效", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                string themePath = Path.Combine(Application.StartupPath, "Resources", themeName);
                string lPath = Path.Combine(themePath, "l");
                string vPath = Path.Combine(themePath, "v");
                string numPath = Path.Combine(themePath, "num");

                ThemeManager.Instance.StopAllSounds();

                Directory.CreateDirectory(lPath);
                Directory.CreateDirectory(vPath);
                Directory.CreateDirectory(numPath);

                File.Copy(_landscapePathTextBox.Text, Path.Combine(lPath, "picture.jpg"), true);
                File.Copy(_portraitPathTextBox.Text, Path.Combine(vPath, "picture.jpg"), true);

                for (int i = 0; i < 10; i++)
                {
                    string ext = Path.GetExtension(_numberPaths[i].Text);
                    File.Copy(_numberPaths[i].Text, Path.Combine(numPath, $"{i}{ext}"), true);
                }

                string startExt = Path.GetExtension(_startSoundPathTextBox.Text);
                string endExt = Path.GetExtension(_endSoundPathTextBox.Text);
                File.Copy(_startSoundPathTextBox.Text, Path.Combine(themePath, $"start{startExt}"), true);
                File.Copy(_endSoundPathTextBox.Text, Path.Combine(themePath, $"end{endExt}"), true);

                MessageBox.Show(!string.IsNullOrEmpty(_editingThemeName) ? "主题修改成功!" : "主题创建成功!", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{(!string.IsNullOrEmpty(_editingThemeName) ? "修改" : "创建")}主题失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    internal static class NativeMethods
    {
        public const uint ES_CONTINUOUS = 0x80000000;
        public const uint ES_DISPLAY_REQUIRED = 0x00000002;
        public const uint ES_SYSTEM_REQUIRED = 0x00000001;

        [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
        public static extern uint SetThreadExecutionState(uint esFlags);
    }

    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}