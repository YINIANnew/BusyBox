using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

namespace BusyBox
{
    public class AIState
    {
        public long DailyTokens { get; set; }
        public long CurrentSessionTokens { get; set; }
        public DateTime LastRequestTime { get; set; }
        public int RequestCount10Min { get; set; }
        public DateTime Last10MinReset { get; set; }
        public DateTime LastDateReset { get; set; }
        public string MachineCode { get; set; }
    }

    public class ChatMessage
    {
        public string Sender { get; set; }
        public string Message { get; set; }
        public DateTime Time { get; set; }
    }

    public class AIForm : Form
    {
        public AIForm()
        {
            Text = "人工智能";
            Size = new Size(900, 700);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(30, 30, 30);

            var tabControl = new TabControl { Dock = DockStyle.Fill, BackColor = Color.FromArgb(45, 45, 45), ForeColor = Color.White };

            tabControl.TabPages.Add(CreateLocalTab());
            tabControl.TabPages.Add(CreateCloudTab());
            tabControl.TabPages.Add(CreateActivityTab());

            Controls.Add(tabControl);
        }

        private TabPage CreateLocalTab()
        {
            var tab = new TabPage("本地AI");
            tab.BackColor = Color.FromArgb(45, 45, 45);

            var panel = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            int y = 20;

            var infoGroup = new GroupBox { Text = "模型信息", Location = new Point(20, y), Size = new Size(820, 120), ForeColor = Color.White };
            y += 130;

            var modelNameLabel = new Label { Text = "模型名称:", ForeColor = Color.White, Location = new Point(20, 25), AutoSize = true };
            var modelNameBox = new TextBox { Text = "model.gguf", Location = new Point(100, 22), Size = new Size(200, 25), BackColor = Color.FromArgb(30, 30, 30), ForeColor = Color.White };
            infoGroup.Controls.Add(modelNameLabel);
            infoGroup.Controls.Add(modelNameBox);

            var formatLabel = new Label { Text = "模型格式:", ForeColor = Color.White, Location = new Point(320, 25), AutoSize = true };
            var formatCombo = new ComboBox { Location = new Point(400, 22), Size = new Size(150, 25), BackColor = Color.FromArgb(30, 30, 30), ForeColor = Color.White, Items = { "GGUF", "GGML", "PT" }, SelectedIndex = 0 };
            infoGroup.Controls.Add(formatLabel);
            infoGroup.Controls.Add(formatCombo);

            var archLabel = new Label { Text = "模型架构:", ForeColor = Color.White, Location = new Point(570, 25), AutoSize = true };
            var archCombo = new ComboBox { Location = new Point(650, 22), Size = new Size(150, 25), BackColor = Color.FromArgb(30, 30, 30), ForeColor = Color.White, Items = { "LLaMA", "Mistral", "GPT-NeoX", "Qwen", "Phi" }, SelectedIndex = 0 };
            infoGroup.Controls.Add(archLabel);
            infoGroup.Controls.Add(archCombo);

            var browseBtn = new Button { Text = "浏览模型文件", Location = new Point(20, 65), Size = new Size(120, 25), BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            browseBtn.Click += (s, e) =>
            {
                using (var dialog = new OpenFileDialog { Filter = "GGUF文件|*.gguf|GGML文件|*.ggml|所有文件|*.*" })
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        modelNameBox.Text = dialog.FileName;
                    }
                }
            };
            infoGroup.Controls.Add(browseBtn);
            panel.Controls.Add(infoGroup);

            var paramsGroup = new GroupBox { Text = "核心参数", Location = new Point(20, y), Size = new Size(820, 120), ForeColor = Color.White };
            y += 130;

            AddParamRow(paramsGroup, 25, "Max Tokens:", "2048");
            AddParamRow(paramsGroup, 55, "Temperature:", "0.7");
            AddParamRow(paramsGroup, 85, "最大响应长度:", "1024");
            AddParamRow(paramsGroup, 115, "上下文溢出:", "256");
            panel.Controls.Add(paramsGroup);

            var samplingGroup = new GroupBox { Text = "采样参数", Location = new Point(20, y), Size = new Size(820, 120), ForeColor = Color.White };
            y += 130;

            AddParamRow(samplingGroup, 25, "Top K:", "40");
            AddParamRow(samplingGroup, 55, "Top P:", "0.95");
            AddParamRow(samplingGroup, 85, "最小P采样:", "0.05");
            AddParamRow(samplingGroup, 115, "重复惩罚:", "1.1");
            panel.Controls.Add(samplingGroup);

            var advancedGroup = new GroupBox { Text = "高级设置", Location = new Point(20, y), Size = new Size(820, 150), ForeColor = Color.White };
            y += 160;

            AddParamRow(advancedGroup, 25, "ROPE频率集合:", "10000");
            AddParamRow(advancedGroup, 55, "ROPE频率比例:", "1.0");
            AddParamRow(advancedGroup, 85, "评估批处理大小:", "512");
            AddParamRow(advancedGroup, 115, "CPU线程池:", "8");
            AddParamRow(advancedGroup, 145, "Seed:", "-1");
            panel.Controls.Add(advancedGroup);

            var gpuGroup = new GroupBox { Text = "GPU设置", Location = new Point(20, y), Size = new Size(820, 100), ForeColor = Color.White };
            y += 110;

            var gpuOffloadLabel = new Label { Text = "GPU卸载:", ForeColor = Color.White, Location = new Point(20, 25), AutoSize = true };
            var gpuOffloadBox = new TextBox { Text = "2048", Location = new Point(100, 22), Size = new Size(100, 25), BackColor = Color.FromArgb(30, 30, 30), ForeColor = Color.White };
            gpuGroup.Controls.Add(gpuOffloadLabel);
            gpuGroup.Controls.Add(gpuOffloadBox);

            var mmapCheck = new CheckBox { Text = "尝试mmap", Location = new Point(220, 25), ForeColor = Color.White, Checked = true };
            gpuGroup.Controls.Add(mmapCheck);

            var kvCacheCheck = new CheckBox { Text = "K/V Cache Quant", Location = new Point(350, 25), ForeColor = Color.White };
            gpuGroup.Controls.Add(kvCacheCheck);
            panel.Controls.Add(gpuGroup);

            var promptGroup = new GroupBox { Text = "Prompt设置", Location = new Point(20, y), Size = new Size(820, 120), ForeColor = Color.White };
            y += 130;

            var stopTokensLabel = new Label { Text = "Stop Tokens:", ForeColor = Color.White, Location = new Point(20, 25), AutoSize = true };
            var stopTokensBox = new TextBox { Text = "</s>", Location = new Point(100, 22), Size = new Size(200, 25), BackColor = Color.FromArgb(30, 30, 30), ForeColor = Color.White };
            promptGroup.Controls.Add(stopTokensLabel);
            promptGroup.Controls.Add(stopTokensBox);

            var sysPromptLabel = new Label { Text = "System Prompt:", ForeColor = Color.White, Location = new Point(20, 60), AutoSize = true };
            var sysPromptBox = new TextBox { Text = "你是一个有帮助的助手。", Location = new Point(120, 57), Size = new Size(680, 25), BackColor = Color.FromArgb(30, 30, 30), ForeColor = Color.White };
            promptGroup.Controls.Add(sysPromptLabel);
            promptGroup.Controls.Add(sysPromptBox);
            panel.Controls.Add(promptGroup);

            var templateGroup = new GroupBox { Text = "模板设置", Location = new Point(20, y), Size = new Size(820, 100), ForeColor = Color.White };
            y += 110;

            var templateCombo = new ComboBox { Location = new Point(100, 22), Size = new Size(300, 25), BackColor = Color.FromArgb(30, 30, 30), ForeColor = Color.White, Items = { "LLaMA", "ChatML", "Alpaca", "Vicuna", "Qwen", "Custom" }, SelectedIndex = 0 };
            templateGroup.Controls.Add(new Label { Text = "Prompt模板:", ForeColor = Color.White, Location = new Point(20, 25) });
            templateGroup.Controls.Add(templateCombo);
            panel.Controls.Add(templateGroup);

            var startButton = new Button { Location = new Point(20, y), Size = new Size(200, 40), Text = "开始本地对话", BackColor = Color.FromArgb(39, 174, 96), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 12) };
            startButton.Click += (s, e) => OpenChat("local");
            panel.Controls.Add(startButton);

            tab.Controls.Add(panel);
            return tab;
        }

        private void AddParamRow(Control parent, int y, string label, string defaultValue)
        {
            var lbl = new Label { Text = label, ForeColor = Color.White, Location = new Point(20, y), AutoSize = true };
            var box = new TextBox { Text = defaultValue, Location = new Point(120, y - 3), Size = new Size(100, 25), BackColor = Color.FromArgb(30, 30, 30), ForeColor = Color.White };
            parent.Controls.Add(lbl);
            parent.Controls.Add(box);
        }

        private TabPage CreateActivityTab()
        {
            var tab = new TabPage("意念AI (限时活动)");
            tab.BackColor = Color.FromArgb(45, 45, 45);

            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            var bannerPanel = new Panel { Location = new Point(20, 20), Size = new Size(820, 80), BackColor = Color.FromArgb(155, 89, 182) };
            var bannerLabel = new Label { Text = "意念请你玩转人工智能【限时】\n每日免费额度：8000 Tokens", Location = new Point(20, 15), Size = new Size(780, 50), ForeColor = Color.White, Font = new Font("Segoe UI", 14, FontStyle.Bold) };
            bannerPanel.Controls.Add(bannerLabel);
            panel.Controls.Add(bannerPanel);

            var deepseekButton = new Button
            {
                Location = new Point(20, 120),
                Size = new Size(390, 80),
                Text = "DeepSeek\n(多模态)",
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 14)
            };
            deepseekButton.Click += (s, e) => OpenChat("deepseek");
            panel.Controls.Add(deepseekButton);

            var doubaoButton = new Button
            {
                Location = new Point(430, 120),
                Size = new Size(390, 80),
                Text = "豆包\n(多模态)",
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 14)
            };
            doubaoButton.Click += (s, e) => OpenChat("doubao");
            panel.Controls.Add(doubaoButton);

            var rulesLabel = new Label
            {
                Text = "活动规则：\n• 每日调用总Tokens不超过8000\n• 单轮对话不超过1500 Tokens\n• 每75秒内最多一次请求\n• 单机器码每日不超过1600 Tokens\n• 每10分钟最多5次请求",
                Location = new Point(20, 220),
                Size = new Size(820, 100),
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 11)
            };
            panel.Controls.Add(rulesLabel);

            tab.Controls.Add(panel);
            return tab;
        }

        private TabPage CreateCloudTab()
        {
            var tab = new TabPage("云端API");
            tab.BackColor = Color.FromArgb(45, 45, 45);

            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            int y = 20;

            var platformCombo = new ComboBox { Location = new Point(100, y), Size = new Size(200, 25), BackColor = Color.FromArgb(45, 45, 45), ForeColor = Color.White, Items = { "OpenAI", "DeepSeek" }, SelectedIndex = 0 };
            panel.Controls.Add(new Label { Text = "平台:", ForeColor = Color.White, Location = new Point(20, y + 3) });
            panel.Controls.Add(platformCombo);
            y += 40;

            var apiKeyBox = new TextBox { Location = new Point(100, y), Size = new Size(350, 25), BackColor = Color.FromArgb(45, 45, 45), ForeColor = Color.White, PasswordChar = '*' };
            panel.Controls.Add(new Label { Text = "API Key:", ForeColor = Color.White, Location = new Point(20, y + 3) });
            panel.Controls.Add(apiKeyBox);
            y += 40;

            var modelBox = new TextBox { Location = new Point(100, y), Size = new Size(200, 25), BackColor = Color.FromArgb(45, 45, 45), ForeColor = Color.White, Text = "gpt-4o-mini" };
            panel.Controls.Add(new Label { Text = "模型:", ForeColor = Color.White, Location = new Point(20, y + 3) });
            panel.Controls.Add(modelBox);
            y += 40;

            var startButton = new Button { Location = new Point(20, y), Size = new Size(200, 35), Text = "开始对话", BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            startButton.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(apiKeyBox.Text))
                {
                    MessageBox.Show("请输入API Key！");
                    return;
                }
                OpenChat("cloud", platformCombo.SelectedItem.ToString(), apiKeyBox.Text, modelBox.Text);
            };
            panel.Controls.Add(startButton);

            tab.Controls.Add(panel);
            return tab;
        }

        private void OpenChat(string provider, params string[] args)
        {
            var chatForm = new AIChatForm(provider, args);
            chatForm.Show();
        }
    }

    public class AIChatForm : Form
    {
        private RichTextBox _chatBox;
        private TextBox _inputBox;
        private Button _sendButton;
        private Label _statusLabel;
        private string _provider;
        private string[] _args;
        private HttpClient _httpClient;
        private List<ChatMessage> _chatMessages;
        private string _chatLogPath;
        private DateTime _lastRequestTime = DateTime.MinValue;
        private long _dailyTokensUsed = 0;
        private DateTime _lastDateReset = DateTime.MinValue;
        private int _requestCount10Min = 0;
        private DateTime _last10MinReset = DateTime.MinValue;
        private string _machineCode = "";
        private bool _isReceiving = false;
        private string _currentResponse = "";

        public AIChatForm(string provider, string[] args)
        {
            _provider = provider;
            _args = args;
            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
            _chatMessages = new List<ChatMessage>();
            _chatLogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BusyBox", $"ChatLog_{provider}.json");
            _machineCode = GetMachineCode();
            LoadState();
            LoadChatHistory();
            InitializeUI();
        }

        private string GetMachineCode()
        {
            var cpuId = "";
            var mbId = "";
            try
            {
                using (var searcher = new System.Management.ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        cpuId = obj["ProcessorId"]?.ToString() ?? "";
                        break;
                    }
                }
                using (var searcher = new System.Management.ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        mbId = obj["SerialNumber"]?.ToString() ?? "";
                        break;
                    }
                }
            }
            catch { }
            return cpuId + mbId;
        }

        private void LoadState()
        {
            try
            {
                var statePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BusyBox", "ai_state.json");
                if (File.Exists(statePath))
                {
                    var json = File.ReadAllText(statePath);
                    var state = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
                    if (state != null)
                    {
                        if (state.TryGetValue("LastDateReset", out var dateReset))
                            _lastDateReset = dateReset.GetDateTime();
                        if (state.TryGetValue("DailyTokensUsed", out var tokens))
                            _dailyTokensUsed = tokens.GetInt64();
                        if (state.TryGetValue("LastRequestTime", out var reqTime))
                            _lastRequestTime = reqTime.GetDateTime();
                        if (state.TryGetValue("RequestCount10Min", out var count))
                            _requestCount10Min = count.GetInt32();
                        if (state.TryGetValue("Last10MinReset", out var tenMinReset))
                            _last10MinReset = tenMinReset.GetDateTime();
                    }
                }
            }
            catch { }
            ResetDailyLimitIfNeeded();
            Reset10MinLimitIfNeeded();
        }

        private void SaveState()
        {
            try
            {
                var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BusyBox");
                Directory.CreateDirectory(dir);
                var statePath = Path.Combine(dir, "ai_state.json");
                var state = new Dictionary<string, object>
                {
                    { "LastDateReset", _lastDateReset },
                    { "DailyTokensUsed", _dailyTokensUsed },
                    { "LastRequestTime", _lastRequestTime },
                    { "RequestCount10Min", _requestCount10Min },
                    { "Last10MinReset", _last10MinReset }
                };
                File.WriteAllText(statePath, JsonSerializer.Serialize(state));
            }
            catch { }
        }

        private void ResetDailyLimitIfNeeded()
        {
            if (_lastDateReset.Date < DateTime.Now.Date)
            {
                _dailyTokensUsed = 0;
                _lastDateReset = DateTime.Now;
                SaveState();
            }
        }

        private void Reset10MinLimitIfNeeded()
        {
            if ((DateTime.Now - _last10MinReset).TotalMinutes >= 10)
            {
                _requestCount10Min = 0;
                _last10MinReset = DateTime.Now;
                SaveState();
            }
        }

        private bool CheckRateLimit()
        {
            ResetDailyLimitIfNeeded();
            Reset10MinLimitIfNeeded();

            if (_dailyTokensUsed >= 8000)
            {
                DisplayMessage("系统", "每日8000 Tokens额度已用完，请明天再试。");
                return false;
            }

            if ((DateTime.Now - _lastRequestTime).TotalSeconds < 75)
            {
                var remainingSeconds = 75 - (int)(DateTime.Now - _lastRequestTime).TotalSeconds;
                DisplayMessage("系统", $"距离下次请求还需 {remainingSeconds} 秒。");
                return false;
            }

            if (_requestCount10Min >= 5)
            {
                var remainingMinutes = 10 - (int)(DateTime.Now - _last10MinReset).TotalMinutes;
                DisplayMessage("系统", $"每10分钟最多5次请求，请在 {remainingMinutes} 分钟后再试。");
                return false;
            }

            return true;
        }

        private void UpdateRateLimitState(int tokensUsed)
        {
            _lastRequestTime = DateTime.Now;
            _dailyTokensUsed += tokensUsed;
            _requestCount10Min++;
            SaveState();
        }

        private void LoadChatHistory()
        {
            try
            {
                if (File.Exists(_chatLogPath))
                {
                    var json = File.ReadAllText(_chatLogPath);
                    var messages = JsonSerializer.Deserialize<List<ChatMessage>>(json);
                    if (messages != null) _chatMessages.AddRange(messages);
                }
            }
            catch { }
        }

        private void SaveChatHistory()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_chatLogPath));
                File.WriteAllText(_chatLogPath, JsonSerializer.Serialize(_chatMessages));
            }
            catch { }
        }

        private void InitializeUI()
        {
            Text = "AI对话";
            Size = new Size(700, 600);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(30, 30, 30);

            _chatBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(20, 20, 20),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12),
                ReadOnly = true,
                BorderStyle = BorderStyle.None
            };
            Controls.Add(_chatBox);

            var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 80, BackColor = Color.FromArgb(45, 45, 45) };

            _inputBox = new TextBox { Location = new Point(10, 15), Size = new Size(550, 30), BackColor = Color.FromArgb(30, 30, 30), ForeColor = Color.White, Font = new Font("Segoe UI", 12) };
            _inputBox.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter && !_isReceiving) SendMessage(); };
            bottomPanel.Controls.Add(_inputBox);

            _sendButton = new Button { Location = new Point(570, 15), Size = new Size(100, 30), Text = "发送", BackColor = Color.FromArgb(39, 174, 96), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            _sendButton.Click += (s, e) => { if (!_isReceiving) SendMessage(); };
            bottomPanel.Controls.Add(_sendButton);

            _statusLabel = new Label { Location = new Point(10, 50), Size = new Size(680, 20), ForeColor = Color.Gray, Text = $"今日已用: {_dailyTokensUsed}/8000 Tokens" };
            bottomPanel.Controls.Add(_statusLabel);

            Controls.Add(bottomPanel);

            foreach (var msg in _chatMessages)
            {
                DisplayMessage(msg.Sender, msg.Message, msg.Time);
            }
            if (_chatMessages.Count == 0)
            {
                DisplayMessage("系统", "欢迎您，今日剩余额度: " + (8000 - _dailyTokensUsed) + " Tokens");
            }
        }

        private void DisplayMessage(string sender, string message) => DisplayMessage(sender, message, DateTime.Now);

        private void DisplayMessage(string sender, string message, DateTime time)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => DisplayMessage(sender, message, time)));
                return;
            }

            _chatBox.SelectionStart = _chatBox.Text.Length;
            _chatBox.SelectionColor = sender == "你" ? Color.LightBlue : sender == "AI" ? Color.LightGreen : Color.Gray;
            _chatBox.SelectedText = $"[{sender}] {time:HH:mm:ss}\n";
            _chatBox.SelectionColor = Color.White;
            _chatBox.SelectedText = $"{message}\n\n";
            _chatBox.ScrollToCaret();
        }

        private void UpdatePartialResponse(string text)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdatePartialResponse(text)));
                return;
            }

            if (_currentResponse == text) return;

            int lastIndex = _chatBox.Text.LastIndexOf("[AI]");
            if (lastIndex < 0) return;

            int endOfAiMarker = _chatBox.Text.IndexOf("\n", lastIndex);
            if (endOfAiMarker < 0) return;

            int nextMarker = _chatBox.Text.IndexOf("[", endOfAiMarker + 1);
            int endPos = nextMarker > 0 ? nextMarker - 1 : _chatBox.Text.Length;

            _chatBox.Text = _chatBox.Text.Substring(0, endOfAiMarker) + "\n" + text + "\n\n";
            _currentResponse = text;
            _chatBox.ScrollToCaret();
        }

        private async void SendMessage()
        {
            if (_isReceiving) return;

            string message = _inputBox.Text.Trim();
            if (string.IsNullOrWhiteSpace(message)) return;

            if (!CheckRateLimit())
            {
                _statusLabel.Text = "请求被限制";
                return;
            }

            _isReceiving = true;
            _sendButton.Enabled = false;
            _inputBox.Clear();
            _statusLabel.Text = "正在请求...";

            DisplayMessage("你", message);

            _currentResponse = "";
            int aiMarkerPos = _chatBox.Text.Length;
            _chatBox.SelectionStart = _chatBox.Text.Length;
            _chatBox.SelectionColor = Color.LightGreen;
            _chatBox.SelectedText = $"[AI] {DateTime.Now:HH:mm:ss}\n";
            _chatBox.SelectionColor = Color.White;
            _chatBox.SelectedText = "\n\n";
            _chatBox.ScrollToCaret();

            try
            {
                string response = await GetResponse(message);
                UpdatePartialResponse(response);
                _chatMessages.Add(new ChatMessage { Sender = "你", Message = message, Time = DateTime.Now });
                _chatMessages.Add(new ChatMessage { Sender = "AI", Message = response, Time = DateTime.Now });
                SaveChatHistory();

                int tokensEstimate = (message.Length + response.Length) / 4;
                UpdateRateLimitState(tokensEstimate);
                _statusLabel.Text = $"今日已用: {_dailyTokensUsed}/8000 Tokens";
            }
            catch (Exception ex)
            {
                UpdatePartialResponse($"请求失败: {ex.Message}");
            }

            _isReceiving = false;
            _sendButton.Enabled = true;
        }

        private async System.Threading.Tasks.Task<string> GetResponse(string message)
        {
            switch (_provider)
            {
                case "deepseek":
                    return await CallDeepSeek(message);
                case "doubao":
                    return await CallDoubao(message);
                case "cloud":
                    return await CallCloudAPI(message);
                default:
                    return "未知服务";
            }
        }

        private async System.Threading.Tasks.Task<string> CallDeepSeek(string message)
        {
            if (string.IsNullOrWhiteSpace(_args[0]))
            {
                return "API密钥未配置";
            }

            try
            {
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };

                var requestData = new
                {
                    model = "deepseek-chat",
                    messages = new[] { new { role = "user", content = message } },
                    max_tokens = 1500
                };

                var content = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _args[0]);

                using var response = await client.PostAsync("https://api.deepseek.com/v1/chat/completions", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return $"DeepSeek API错误: {response.StatusCode}\n{errorContent}";
                }

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.TryGetProperty("choices", out var choices) && choices.GetArrayLength() > 0)
                {
                    var messageObj = choices[0].GetProperty("message");
                    if (messageObj.TryGetProperty("content", out var contentProp))
                    {
                        return contentProp.GetString() ?? "未收到有效响应";
                    }
                }

                return "DeepSeek API返回格式未知\n原始响应: " + json;
            }
            catch (Exception ex)
            {
                return $"DeepSeek调用失败: {ex.Message}";
            }
        }

        private async System.Threading.Tasks.Task<string> CallDoubao(string message)
        {
            if (string.IsNullOrWhiteSpace(_args[0]))
            {
                return "API密钥未配置";
            }

            try
            {
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };

                var requestBody = new
                {
                    model = "doubao-seed-2-0-mini-260428",
                    input = new[]
                    {
                        new
                        {
                            role = "user",
                            content = new[]
                            {
                                new { type = "input_text", text = message }
                            }
                        }
                    }
                };

                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _args[0]);

                var response = await client.PostAsync("https://ark.cn-beijing.volces.com/api/v3/responses", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    return $"豆包API错误\n状态码: {response.StatusCode}\n响应: {errorContent}";
                }

                var json = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(json))
                {
                    return "豆包API返回空响应";
                }

                try
                {
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("output", out var output))
                    {
                        foreach (var item in output.EnumerateArray())
                        {
                            if (item.TryGetProperty("type", out var type) && type.GetString() == "message")
                            {
                                if (item.TryGetProperty("role", out var role) && role.GetString() == "assistant")
                                {
                                    if (item.TryGetProperty("content", out var contentArray))
                                    {
                                        foreach (var contentItem in contentArray.EnumerateArray())
                                        {
                                            if (contentItem.TryGetProperty("type", out var contentType) &&
                                                contentType.GetString() == "output_text")
                                            {
                                                if (contentItem.TryGetProperty("text", out var text))
                                                {
                                                    return text.GetString() ?? "";
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    return "豆包API返回格式未知\n原始响应: " + json;
                }
                catch (Exception ex)
                {
                    return $"解析响应失败: {ex.Message}\n原始响应: {json}";
                }
            }
            catch (Exception ex)
            {
                return $"豆包调用失败: {ex.Message}";
            }
        }

        private async System.Threading.Tasks.Task<string> CallCloudAPI(string message)
        {
            string platform = _args[0];
            string apiKey = _args[1];
            string model = _args[2];

            var data = new { model = model, messages = new[] { new { role = "user", content = message } }, max_tokens = 1024 };
            var content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            string url = platform == "DeepSeek" ? "https://api.deepseek.com/v1/chat/completions" : "https://api.openai.com/v1/chat/completions";
            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode) return $"API错误: {response.StatusCode}";

            var json = await response.Content.ReadAsStringAsync();
            var obj = JsonDocument.Parse(json).RootElement;
            return obj.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
        }
    }
}
