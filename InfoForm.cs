using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace BusyBox
{
    public class InfoForm : Form
    {
        private bool _showFullIP = false;
        private string _userIP = "";
        private List<string> _logs;
        private string _logsFilePath;

        public InfoForm()
        {
            Text = "信息";
            Size = new Size(950, 700);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(30, 30, 30);

            _logsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BusyBox", "Logs", $"log_{DateTime.Now:yyyyMMdd}.txt");
            _logs = LoadLogs();

            InitializeUI();
        }

        private List<string> LoadLogs()
        {
            var logs = new List<string>();
            try
            {
                if (File.Exists(_logsFilePath))
                {
                    var lines = File.ReadAllLines(_logsFilePath);
                    logs.AddRange(lines.TakeLast(100));
                }
            }
            catch { }
            return logs;
        }

        private void InitializeUI()
        {
            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };
            
            var titleLabel = new Label
            {
                Text = "BusyBox 信息",
                Location = new Point(20, 10),
                Size = new Size(440, 40),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            panel.Controls.Add(titleLabel);

            var leftPanel = new Panel { Location = new Point(20, 60), Size = new Size(440, 580) };
            
            var infoGroup = new GroupBox
            {
                Text = "基本信息",
                Location = new Point(0, 0),
                Size = new Size(440, 250),
                ForeColor = Color.White
            };

            int infoY = 25;
            AddInfoRow(infoGroup, ref infoY, "版本:", "正式版 v1.0.0");
            AddInfoRow(infoGroup, ref infoY, "系统:", Environment.OSVersion.ToString());
            AddInfoRow(infoGroup, ref infoY, "设备:", $"{Environment.MachineName} / {Environment.UserName}");
            AddInfoRow(infoGroup, ref infoY, "处理器:", $"{Environment.ProcessorCount} 核心");
            AddInfoRow(infoGroup, ref infoY, "安装日期:", GetInstallDate());
            
            var ipPanel = new Panel { Location = new Point(100, infoY), Size = new Size(300, 25) };
            var ipLabel = new Label { Text = "用户IP:", Location = new Point(0, 3), ForeColor = Color.White, AutoSize = true };
            var ipValueLabel = new Label { Text = "加载中...", Location = new Point(60, 3), ForeColor = Color.LightBlue, AutoSize = true };
            var eyeButton = new Button { Text = "显示/隐藏", Location = new Point(240, 0), Size = new Size(80, 25), BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 10) };
            eyeButton.Click += (s, e) => ToggleIPVisibility(ipValueLabel, eyeButton);

            ipPanel.Controls.Add(ipLabel);
            ipPanel.Controls.Add(ipValueLabel);
            ipPanel.Controls.Add(eyeButton);
            infoGroup.Controls.Add(ipPanel);

            LoadUserIP(ipValueLabel);

            leftPanel.Controls.Add(infoGroup);

            var logGroup = new GroupBox
            {
                Text = $"运行日志 ({DateTime.Now:yyyy-MM-dd})",
                Location = new Point(0, 260),
                Size = new Size(440, 150),
                ForeColor = Color.White
            };

            var logBox = new ListBox
            {
                Location = new Point(10, 25),
                Size = new Size(420, 95),
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.LightGray,
                Font = new Font("Consolas", 9)
            };

            foreach (var log in _logs.TakeLast(10))
            {
                logBox.Items.Add(log);
            }

            var downloadLogBtn = new Button { Text = "下载完整日志", Location = new Point(10, 125), Size = new Size(120, 20), BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 8) };
            downloadLogBtn.Click += (s, e) => DownloadLogs();

            logGroup.Controls.Add(logBox);
            logGroup.Controls.Add(downloadLogBtn);
            leftPanel.Controls.Add(logGroup);

            panel.Controls.Add(leftPanel);

            var rightPanel = new Panel { Location = new Point(480, 60), Size = new Size(440, 580) };

            var rightTitle = new Label
            {
                Text = "快捷链接",
                Location = new Point(0, 0),
                Size = new Size(440, 30),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            rightPanel.Controls.Add(rightTitle);

            int btnY = 40;
            AddActionButton(rightPanel, ref btnY, "我是新用户", Color.FromArgb(52, 152, 219), () => OpenUrl("https://busybox.devynhub.org/#/documents"));
            AddActionButton(rightPanel, ref btnY, "详细信息&帮助", Color.FromArgb(155, 89, 182), () => OpenUrl("https://busybox.devynhub.org/#/help"));
            AddActionButton(rightPanel, ref btnY, "用户反馈", Color.FromArgb(39, 174, 96), () => OpenUrl("https://busybox.devynhub.org/#/feedback"));
            AddActionButton(rightPanel, ref btnY, "项目官网", Color.FromArgb(243, 156, 18), () => OpenUrl("https://busybox.devynhub.org/#/"));
            AddActionButton(rightPanel, ref btnY, "Donate", Color.FromArgb(231, 76, 60), ShowDonateDialog);
            AddActionButton(rightPanel, ref btnY, "程序源码", Color.FromArgb(44, 62, 80), ShowSourceCodeDialog);
            AddActionButton(rightPanel, ref btnY, "致谢", Color.FromArgb(255, 99, 71), () => OpenUrl("https://busybox.devynhub.org/#/acknowledgements"));
            AddActionButton(rightPanel, ref btnY, "程序权限", Color.FromArgb(127, 140, 141), ShowPermissionsDialog);

            panel.Controls.Add(rightPanel);

            Controls.Add(panel);
        }

        private void AddInfoRow(Control parent, ref int y, string label, string value)
        {
            var lbl = new Label { Text = label, Location = new Point(10, y), ForeColor = Color.White, AutoSize = true };
            var val = new Label { Text = value, Location = new Point(100, y), ForeColor = Color.LightGreen, AutoSize = true };
            parent.Controls.Add(lbl);
            parent.Controls.Add(val);
            y += 25;
        }

        private void AddActionButton(Panel parent, ref int y, string text, Color backColor, Action action)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(20, y),
                Size = new Size(440, 35),
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10)
            };
            btn.Click += (s, e) => action();
            parent.Controls.Add(btn);
            y += 45;
        }

        private string GetInstallDate()
        {
            var installFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BusyBox", "install_date.txt");
            if (File.Exists(installFile))
            {
                return File.ReadAllText(installFile);
            }
            else
            {
                Directory.CreateDirectory(Path.GetDirectoryName(installFile));
                var date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                File.WriteAllText(installFile, date);
                return date;
            }
        }

        private async void LoadUserIP(Label label)
        {
            try
            {
                using (var client = new WebClient())
                {
                    var ip = await client.DownloadStringTaskAsync("https://api.ipify.org");
                    _userIP = ip.Trim();
                    label.Text = MaskIP(_userIP, false);
                }
            }
            catch
            {
                _userIP = "无法获取";
                label.Text = "无法获取";
            }
        }

        private string MaskIP(string ip, bool full)
        {
            if (ip == "无法获取" || string.IsNullOrEmpty(ip)) return ip;
            if (full) return ip;

            var parts = ip.Split('.');
            if (parts.Length == 4)
            {
                return $"{parts[0]}.{parts[1]}.*.*";
            }
            return ip;
        }

        private void ToggleIPVisibility(Label label, Button btn)
        {
            _showFullIP = !_showFullIP;
            label.Text = MaskIP(_userIP, _showFullIP);
            btn.Text = _showFullIP ? "隐藏IP" : "显示IP";
        }

        private void DownloadLogs()
        {
            using (var saveDialog = new SaveFileDialog
            {
                Filter = "文本文件|*.txt",
                FileName = $"BusyBox_Log_{DateTime.Now:yyyyMMdd}.txt"
            })
            {
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllLines(saveDialog.FileName, _logs);
                    MessageBox.Show("日志已下载！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"无法打开链接: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowDonateDialog()
        {
            var dialog = new Form
            {
                Text = "Donate",
                Size = new Size(350, 220),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            var warningIcon = new Label { Text = "警告", Location = new Point(20, 20), Font = new Font("Segoe UI", 32), AutoSize = true };
            panel.Controls.Add(warningIcon);

            var messageLabel = new Label
            {
                Text = "捐款声明\n\n1. 本项目为非盈利，拒绝未成年或无收入人群donate\n2. 如您确实想要支持，建议小额donate\n\n是否继续？",
                Location = new Point(70, 20),
                Size = new Size(240, 120),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10)
            };
            panel.Controls.Add(messageLabel);

            var yesBtn = new Button
            {
                Text = "是(Y)",
                Location = new Point(80, 150),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Yes
            };
            yesBtn.Click += (s, e) =>
            {
                dialog.Close();
                OpenUrl("https://busybox.devynhub.org/#/donate");
            };
            panel.Controls.Add(yesBtn);

            var noBtn = new Button
            {
                Text = "否(N)",
                Location = new Point(170, 150),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(128, 128, 128),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.No
            };
            noBtn.Click += (s, e) => dialog.Close();
            panel.Controls.Add(noBtn);

            dialog.Controls.Add(panel);
            dialog.ShowDialog();
        }

        private void ShowSourceCodeDialog()
        {
            MessageBox.Show(
                "程序源码\n\n" +
                "源码下载请前往项目官网，在下载页面查找对应版本。\n\n" +
                "即将为您打开下载页面...",
                "程序源码",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
            OpenUrl("https://busybox.devynhub.org/#/download");
        }

        private void ShowPermissionsDialog()
        {
            var permissions = @"BusyBox 程序权限说明

【已获取的系统权限】

✓ 文件系统访问
  - 读取/写入配置文件
  - 保存用户数据
  - 读写音乐文件
  - 导出日志文件

✓ 网络访问
  - 连接互联网获取用户IP
  - API接口调用
  - 下载在线资源

✓ 系统集成
  - 启动系统程序(如计算器)
  - 访问系统设置
  - 管理进程

✓ 硬件交互
  - 访问音频设备
  - 读取系统信息
  - 监控设备状态

【用户操作触发的权限行为】

• 点击'打开程序' - 启动子进程
• 点击'设置' - 打开系统设置窗口
• 点击'音乐播放' - 访问音频文件
• 点击'主机信息' - 执行系统命令
• 使用AI功能 - 网络请求
• 使用智能设备 - 网络/蓝牙通信

【隐私保护】

• 所有本地数据存储在用户设备
• 不收集用户个人信息
• 不上传敏感数据
• 日志仅保留当天记录";

            MessageBox.Show(permissions, "程序权限", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}