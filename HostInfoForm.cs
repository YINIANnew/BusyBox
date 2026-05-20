using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows.Forms;

namespace BusyBox
{
    public class HostInfoForm : Form
    {
        private TabControl _tabControl;
        private RichTextBox _infoTextBox;
        private Button _refreshButton;
        private Button _copyButton;
        private Button _exportButton;

        public HostInfoForm()
        {
            InitializeComponent();
            LoadSystemInfo();
        }

        private void InitializeComponent()
        {
            Text = "主机信息";
            Size = new Size(900, 650);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(30, 30, 30);
            MinimumSize = new Size(700, 500);

            _tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.White,
                Padding = new Point(10, 10)
            };

            _tabControl.TabPages.Add(CreateSystemInfoTab());
            _tabControl.TabPages.Add(CreateNetworkTab());
            _tabControl.TabPages.Add(CreateHardwareTab());
            _tabControl.TabPages.Add(CreateToolsTab());

            Controls.Add(_tabControl);

            var bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                BackColor = Color.FromArgb(45, 45, 45)
            };

            _refreshButton = new Button
            {
                Location = new Point(20, 10),
                Size = new Size(100, 30),
                Text = "刷新信息",
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _refreshButton.Click += RefreshButton_Click;
            bottomPanel.Controls.Add(_refreshButton);

            _copyButton = new Button
            {
                Location = new Point(130, 10),
                Size = new Size(100, 30),
                Text = "复制信息",
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _copyButton.Click += CopyButton_Click;
            bottomPanel.Controls.Add(_copyButton);

            _exportButton = new Button
            {
                Location = new Point(240, 10),
                Size = new Size(100, 30),
                Text = "导出报告",
                BackColor = Color.FromArgb(155, 89, 182),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            _exportButton.Click += ExportButton_Click;
            bottomPanel.Controls.Add(_exportButton);

            Controls.Add(bottomPanel);
        }

        private TabPage CreateSystemInfoTab()
        {
            var tab = new TabPage("系统信息");
            tab.BackColor = Color.FromArgb(45, 45, 45);

            _infoTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White,
                Font = new Font("Consolas", 10),
                ReadOnly = true,
                BorderStyle = BorderStyle.None
            };
            tab.Controls.Add(_infoTextBox);

            return tab;
        }

        private TabPage CreateNetworkTab()
        {
            var tab = new TabPage("网络信息");
            tab.BackColor = Color.FromArgb(45, 45, 45);

            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };

            var ipInfoGroup = new GroupBox
            {
                Text = "IP地址信息",
                Location = new Point(0, 0),
                Size = new Size(800, 180),
                ForeColor = Color.White
            };

            var ipTextBox = new RichTextBox
            {
                Location = new Point(15, 25),
                Size = new Size(760, 130),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White,
                Font = new Font("Consolas", 10),
                ReadOnly = true,
                BorderStyle = BorderStyle.None
            };
            ipTextBox.Text = GetNetworkInfo();
            ipInfoGroup.Controls.Add(ipTextBox);
            panel.Controls.Add(ipInfoGroup);

            var pingGroup = new GroupBox
            {
                Text = "网络测试",
                Location = new Point(0, 190),
                Size = new Size(800, 100),
                ForeColor = Color.White
            };

            var pingLabel = new Label { Text = "目标地址:", ForeColor = Color.White, Location = new Point(15, 25), AutoSize = true };
            var pingTextBox = new TextBox { Text = "http://yiniand.com", Location = new Point(90, 22), Size = new Size(200, 25), BackColor = Color.FromArgb(30, 30, 30), ForeColor = Color.White };
            var pingButton = new Button { Text = "Ping测试", Location = new Point(300, 20), Size = new Size(100, 25), BackColor = Color.FromArgb(52, 152, 219), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            var pingResult = new Label { Text = "", ForeColor = Color.LightGreen, Location = new Point(410, 25), AutoSize = true };

            pingButton.Click += (s, e) =>
            {
                try
                {
                    using (var ping = new Ping())
                    {
                        var reply = ping.Send(pingTextBox.Text, 3000);
                        if (reply.Status == IPStatus.Success)
                        {
                            pingResult.Text = $"延迟: {reply.RoundtripTime}ms";
                            pingResult.ForeColor = Color.Green;
                        }
                        else
                        {
                            pingResult.Text = "连接失败";
                            pingResult.ForeColor = Color.Red;
                        }
                    }
                }
                catch
                {
                    pingResult.Text = "测试失败";
                    pingResult.ForeColor = Color.Red;
                }
            };

            pingGroup.Controls.Add(pingLabel);
            pingGroup.Controls.Add(pingTextBox);
            pingGroup.Controls.Add(pingButton);
            pingGroup.Controls.Add(pingResult);
            panel.Controls.Add(pingGroup);

            tab.Controls.Add(panel);
            return tab;
        }

        private TabPage CreateHardwareTab()
        {
            var tab = new TabPage("硬件信息");
            tab.BackColor = Color.FromArgb(45, 45, 45);

            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };

            var cpuGroup = new GroupBox
            {
                Text = "CPU信息",
                Location = new Point(0, 0),
                Size = new Size(400, 150),
                ForeColor = Color.White
            };

            var cpuTextBox = new RichTextBox
            {
                Location = new Point(15, 25),
                Size = new Size(360, 100),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White,
                Font = new Font("Consolas", 9),
                ReadOnly = true,
                BorderStyle = BorderStyle.None
            };
            cpuTextBox.Text = GetCPUInfo();
            cpuGroup.Controls.Add(cpuTextBox);
            panel.Controls.Add(cpuGroup);

            var memoryGroup = new GroupBox
            {
                Text = "内存信息",
                Location = new Point(420, 0),
                Size = new Size(360, 150),
                ForeColor = Color.White
            };

            var memoryTextBox = new RichTextBox
            {
                Location = new Point(15, 25),
                Size = new Size(320, 100),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White,
                Font = new Font("Consolas", 9),
                ReadOnly = true,
                BorderStyle = BorderStyle.None
            };
            memoryTextBox.Text = GetMemoryInfo();
            memoryGroup.Controls.Add(memoryTextBox);
            panel.Controls.Add(memoryGroup);

            var diskGroup = new GroupBox
            {
                Text = "磁盘信息",
                Location = new Point(0, 160),
                Size = new Size(780, 200),
                ForeColor = Color.White
            };

            var diskTextBox = new RichTextBox
            {
                Location = new Point(15, 25),
                Size = new Size(740, 150),
                BackColor = Color.FromArgb(30, 30, 30),
                ForeColor = Color.White,
                Font = new Font("Consolas", 9),
                ReadOnly = true,
                BorderStyle = BorderStyle.None
            };
            diskTextBox.Text = GetDiskInfo();
            diskGroup.Controls.Add(diskTextBox);
            panel.Controls.Add(diskGroup);

            tab.Controls.Add(panel);
            return tab;
        }

        private TabPage CreateToolsTab()
        {
            var tab = new TabPage("系统工具");
            tab.BackColor = Color.FromArgb(45, 45, 45);

            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };

            var systemToolsGroup = new GroupBox
            {
                Text = "系统管理工具",
                Location = new Point(0, 0),
                Size = new Size(800, 150),
                ForeColor = Color.White
            };

            CreateToolButton("设备管理器", "devmgmt.msc", new Point(20, 30), systemToolsGroup);
            CreateToolButton("注册表编辑器", "regedit.exe", new Point(140, 30), systemToolsGroup);
            CreateToolButton("组策略编辑器", "gpedit.msc", new Point(260, 30), systemToolsGroup);
            CreateToolButton("服务管理", "services.msc", new Point(380, 30), systemToolsGroup);
            CreateToolButton("任务计划", "taskschd.msc", new Point(500, 30), systemToolsGroup);
            CreateToolButton("事件查看器", "eventvwr.msc", new Point(620, 30), systemToolsGroup);
            CreateToolButton("系统配置", "msconfig.exe", new Point(20, 75), systemToolsGroup);
            CreateToolButton("性能监视器", "perfmon.exe", new Point(140, 75), systemToolsGroup);

            panel.Controls.Add(systemToolsGroup);

            var diagnosticToolsGroup = new GroupBox
            {
                Text = "诊断工具",
                Location = new Point(0, 160),
                Size = new Size(800, 100),
                ForeColor = Color.White
            };

            CreateToolButton("DirectX诊断", "dxdiag.exe", new Point(20, 30), diagnosticToolsGroup);
            CreateToolButton("系统信息", "msinfo32.exe", new Point(160, 30), diagnosticToolsGroup);
            CreateToolButton("资源监视器", "resmon.exe", new Point(300, 30), diagnosticToolsGroup);
            CreateToolButton("任务管理器", "taskmgr.exe", new Point(440, 30), diagnosticToolsGroup);
            CreateToolButton("命令提示符", "cmd.exe", new Point(580, 30), diagnosticToolsGroup);
            CreateToolButton("PowerShell", "powershell.exe", new Point(720, 30), diagnosticToolsGroup);

            panel.Controls.Add(diagnosticToolsGroup);

            var repairToolsGroup = new GroupBox
            {
                Text = "修复工具",
                Location = new Point(0, 270),
                Size = new Size(800, 150),
                ForeColor = Color.White
            };

            var repairButton1 = new Button { Text = "修复网络连接", Location = new Point(20, 30), Size = new Size(140, 30), BackColor = Color.FromArgb(39, 174, 96), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            repairButton1.Click += (s, e) => RepairNetwork();
            repairToolsGroup.Controls.Add(repairButton1);

            var repairButton2 = new Button { Text = "清理临时文件", Location = new Point(170, 30), Size = new Size(140, 30), BackColor = Color.FromArgb(39, 174, 96), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            repairButton2.Click += (s, e) => CleanTempFiles();
            repairToolsGroup.Controls.Add(repairButton2);

            var repairButton3 = new Button { Text = "重建图标缓存", Location = new Point(320, 30), Size = new Size(140, 30), BackColor = Color.FromArgb(39, 174, 96), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            repairButton3.Click += (s, e) => RebuildIconCache();
            repairToolsGroup.Controls.Add(repairButton3);

            var repairButton4 = new Button { Text = "修复系统文件", Location = new Point(470, 30), Size = new Size(140, 30), BackColor = Color.FromArgb(39, 174, 96), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            repairButton4.Click += (s, e) => RepairSystemFiles();
            repairToolsGroup.Controls.Add(repairButton4);

            var repairButton5 = new Button { Text = "重置网络设置", Location = new Point(620, 30), Size = new Size(140, 30), BackColor = Color.FromArgb(231, 76, 60), ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            repairButton5.Click += (s, e) => ResetNetwork();
            repairToolsGroup.Controls.Add(repairButton5);

            panel.Controls.Add(repairToolsGroup);

            tab.Controls.Add(panel);
            return tab;
        }

        private void CreateToolButton(string text, string command, Point location, GroupBox parent)
        {
            var button = new Button
            {
                Text = text,
                Location = location,
                Size = new Size(120, 28),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            button.Click += (s, e) => RunSystemTool(command);
            parent.Controls.Add(button);
        }

        private void RunSystemTool(string command)
        {
            try
            {
                Process.Start(new ProcessStartInfo(command) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"无法启动 {command}: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadSystemInfo()
        {
            var sb = new StringBuilder();
            sb.AppendLine("═══════════════════════════════════════════════════════════════════════════");
            sb.AppendLine("                        系统信息报告");
            sb.AppendLine("═══════════════════════════════════════════════════════════════════════════");
            sb.AppendLine();

            sb.AppendLine("【操作系统信息】");
            sb.AppendLine($"  操作系统: {Environment.OSVersion}");
            sb.AppendLine($"  操作系统名称: {GetOSName()}");
            sb.AppendLine($"  系统目录: {Environment.SystemDirectory}");
            sb.AppendLine($"  当前目录: {Environment.CurrentDirectory}");
            sb.AppendLine();

            sb.AppendLine("【硬件信息】");
            sb.AppendLine($"  CPU: {GetCPUInfo().Trim()}");
            sb.AppendLine($"  内存: {GetMemoryInfo().Trim()}");
            sb.AppendLine();

            sb.AppendLine("【用户信息】");
            sb.AppendLine($"  用户名: {Environment.UserName}");
            sb.AppendLine($"  机器名: {Environment.MachineName}");
            sb.AppendLine($"  域名: {Environment.UserDomainName}");
            sb.AppendLine();

            sb.AppendLine("【运行时信息】");
            sb.AppendLine($"  .NET版本: {Environment.Version}");
            sb.AppendLine($"  处理器数: {Environment.ProcessorCount}");
            sb.AppendLine($"  是否64位系统: {Environment.Is64BitOperatingSystem}");
            sb.AppendLine($"  是否64位进程: {Environment.Is64BitProcess}");
            sb.AppendLine();

            sb.AppendLine("【环境变量】");
            sb.AppendLine($"  PATH: {Environment.GetEnvironmentVariable("PATH")?.Substring(0, Math.Min(200, Environment.GetEnvironmentVariable("PATH")?.Length ?? 0))}...");
            sb.AppendLine();

            sb.AppendLine("═══════════════════════════════════════════════════════════════════════════");
            sb.AppendLine($"                         生成时间: {DateTime.Now}");
            sb.AppendLine("═══════════════════════════════════════════════════════════════════════════");

            _infoTextBox.Text = sb.ToString();
        }

        private string GetOSName()
        {
            try
            {
                var reg = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
                return $"{reg.GetValue("ProductName")} (Build {reg.GetValue("CurrentBuildNumber")})";
            }
            catch
            {
                return Environment.OSVersion.ToString();
            }
        }

        private string GetCPUInfo()
        {
            try
            {
                var reg = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0");
                return $"{reg.GetValue("ProcessorNameString")}";
            }
            catch
            {
                return "未知";
            }
        }

        private string GetMemoryInfo()
        {
            try
            {
                var totalMemory = (long)new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory;
                var availableMemory = (long)new Microsoft.VisualBasic.Devices.ComputerInfo().AvailablePhysicalMemory;
                return $"总内存: {FormatBytes(totalMemory)}\n可用内存: {FormatBytes(availableMemory)}";
            }
            catch
            {
                return "未知";
            }
        }

        private string GetDiskInfo()
        {
            var sb = new StringBuilder();
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady)
                {
                    sb.AppendLine($"{drive.Name} ({drive.DriveType})");
                    sb.AppendLine($"  总容量: {FormatBytes(drive.TotalSize)}");
                    sb.AppendLine($"  可用空间: {FormatBytes(drive.TotalFreeSpace)}");
                    sb.AppendLine($"  文件系统: {drive.DriveFormat}");
                    sb.AppendLine();
                }
            }
            return sb.ToString();
        }

        private string GetNetworkInfo()
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("【IP地址信息】");
            sb.AppendLine();

            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus == OperationalStatus.Up)
                {
                    sb.AppendLine($"网络适配器: {ni.Name}");
                    sb.AppendLine($"  类型: {ni.NetworkInterfaceType}");
                    sb.AppendLine($"  MAC地址: {ni.GetPhysicalAddress().ToString()}");

                    foreach (var ip in ni.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            sb.AppendLine($"  IPv4地址: {ip.Address}");
                            sb.AppendLine($"  子网掩码: {ip.IPv4Mask}");
                        }
                    }
                    sb.AppendLine();
                }
            }

            sb.AppendLine("【网关信息】");
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (var gateway in ni.GetIPProperties().GatewayAddresses)
                    {
                        if (gateway.Address != null && gateway.Address.ToString() != "0.0.0.0")
                        {
                            sb.AppendLine($"网关: {gateway.Address}");
                        }
                    }
                }
            }

            sb.AppendLine();
            sb.AppendLine("【DNS服务器】");
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (var dns in ni.GetIPProperties().DnsAddresses)
                    {
                        sb.AppendLine($"DNS: {dns}");
                    }
                }
            }

            return sb.ToString();
        }

        private string FormatBytes(long bytes)
        {
            if (bytes < 1024) return bytes + " B";
            if (bytes < 1024 * 1024) return (bytes / 1024.0).ToString("F2") + " KB";
            if (bytes < 1024 * 1024 * 1024) return (bytes / (1024.0 * 1024)).ToString("F2") + " MB";
            return (bytes / (1024.0 * 1024 * 1024)).ToString("F2") + " GB";
        }

        private void RepairNetwork()
        {
            try
            {
                Process.Start(new ProcessStartInfo("cmd.exe", "/c ipconfig /release && ipconfig /renew") { UseShellExecute = true, CreateNoWindow = true });
                MessageBox.Show("网络连接已修复！", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"修复失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CleanTempFiles()
        {
            try
            {
                var tempDirs = new[] { 
                    Path.Combine(Environment.GetEnvironmentVariable("TEMP")),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Temp"),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp")
                };

                int filesDeleted = 0;
                foreach (var dir in tempDirs)
                {
                    if (Directory.Exists(dir))
                    {
                        foreach (var file in Directory.GetFiles(dir))
                        {
                            try { File.Delete(file); filesDeleted++; } catch { }
                        }
                        foreach (var subDir in Directory.GetDirectories(dir))
                        {
                            try { Directory.Delete(subDir, true); } catch { }
                        }
                    }
                }
                MessageBox.Show($"已清理 {filesDeleted} 个临时文件！", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"清理失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RebuildIconCache()
        {
            try
            {
                Process.Start(new ProcessStartInfo("cmd.exe", "/c taskkill /f /im explorer.exe && del /f /s /q \"%localappdata%\\IconCache.db\" && start explorer.exe") { UseShellExecute = true, CreateNoWindow = true });
                MessageBox.Show("图标缓存已重建！", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"重建失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RepairSystemFiles()
        {
            try
            {
                Process.Start(new ProcessStartInfo("cmd.exe", "/c sfc /scannow") { UseShellExecute = true, CreateNoWindow = false });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"启动失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ResetNetwork()
        {
            if (MessageBox.Show("确定要重置网络设置吗？这将清除所有网络配置！", "警告", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    Process.Start(new ProcessStartInfo("cmd.exe", "/c netsh winsock reset && netsh int ip reset") { UseShellExecute = true, CreateNoWindow = true });
                    MessageBox.Show("网络设置已重置，建议重启电脑！", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"重置失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            LoadSystemInfo();
            MessageBox.Show("信息已刷新！", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void CopyButton_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(_infoTextBox.Text);
            MessageBox.Show("信息已复制到剪贴板！", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ExportButton_Click(object sender, EventArgs e)
        {
            using (var saveDialog = new SaveFileDialog
            {
                Filter = "文本文件|*.txt|HTML文件|*.html",
                Title = "导出系统信息"
            })
            {
                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    if (saveDialog.FileName.EndsWith(".html"))
                    {
                        var html = $"<html><body style='background:#1e1e1e;color:#f0f0f0;font-family:Consolas;font-size:12px;white-space:pre-wrap;padding:20px;'>{_infoTextBox.Text.Replace("<", "&lt;").Replace(">", "&gt;")}</body></html>";
                        File.WriteAllText(saveDialog.FileName, html);
                    }
                    else
                    {
                        File.WriteAllText(saveDialog.FileName, _infoTextBox.Text);
                    }
                    MessageBox.Show("报告已导出！", "完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
    }
}
