using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

internal static class AntigravityProxyLauncher
{
    private static readonly string AppDir = AppDomain.CurrentDomain.BaseDirectory;
    private static readonly string LogDir = ResolveDataDirectory();
    private static readonly string LogFile = Path.Combine(LogDir, "launcher.log");

    [STAThread]
    private static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new LauncherForm());
    }

    private sealed class LauncherForm : Form
    {
        private readonly Label status;
        private readonly Button closeButton;

        public LauncherForm()
        {
            Text = "Antigravity 免 TUN 代理启动器";
            Width = 560;
            Height = 245;
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            var title = new Label { AutoSize = true, Left = 24, Top = 18, Text = "正在修复代理文件并启动 Antigravity…", Font = new System.Drawing.Font("Microsoft YaHei UI", 12F) };
            status = new Label { Left = 24, Top = 58, Width = 500, Height = 95, AutoEllipsis = false, Text = "请稍候…", Font = new System.Drawing.Font("Microsoft YaHei UI", 9F) };
            closeButton = new Button { Left = 420, Top = 165, Width = 105, Height = 30, Text = "关闭", Enabled = false };
            closeButton.Click += (s, e) => Close();
            Controls.Add(title);
            Controls.Add(status);
            Controls.Add(closeButton);
            Shown += (s, e) =>
            {
                var startupTimer = new System.Windows.Forms.Timer { Interval = 150 };
                startupTimer.Tick += (sender, args) =>
                {
                    startupTimer.Stop();
                    startupTimer.Dispose();
                    status.Text = "正在执行…";
                    RunRepairAndLaunch();
                };
                startupTimer.Start();
            };
        }

        private void RunRepairAndLaunch()
        {
            try
            {
                Directory.CreateDirectory(LogDir);
                Log("Run started");
                var result = RepairAndLaunch();
                status.Text = result;
                closeButton.Enabled = true;
                closeButton.Text = "完成";
                var timer = new System.Windows.Forms.Timer { Interval = 2800 };
                timer.Tick += (s, e) => { timer.Stop(); timer.Dispose(); Close(); };
                timer.Start();
            }
            catch (Exception ex)
            {
                Log("ERROR: " + ex);
                status.Text = "操作失败：\r\n" + ex.Message + "\r\n\r\n详细日志：" + LogFile;
                closeButton.Enabled = true;
                MessageBox.Show(this, status.Text, "Antigravity 代理启动失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private static string RepairAndLaunch()
    {
        Directory.CreateDirectory(LogDir);
        var sourceDll = Path.Combine(AppDir, "payload", "version.dll");
        var sourceConfig = Path.Combine(AppDir, "payload", "config.json");
        if (!File.Exists(sourceDll) || !File.Exists(sourceConfig))
            throw new FileNotFoundException("启动器目录缺少 version.dll 或 config.json。请保持这两个文件与 exe 放在一起。", AppDir);

        var targetDir = FindAntigravityDirectory();
        if (targetDir == null)
            throw new DirectoryNotFoundException("没有找到 Antigravity.exe。请确认 Antigravity 已安装。\r\n常见目录：%LOCALAPPDATA%\\Programs\\Antigravity");

        Log("Target: " + targetDir);
        StopAntigravityProcesses();
        Thread.Sleep(700);

        var backupDir = Path.Combine(LogDir, "backups", DateTime.Now.ToString("yyyyMMdd-HHmmss"));
        Directory.CreateDirectory(backupDir);
        BackupIfPresent(Path.Combine(targetDir, "version.dll"), backupDir);
        BackupIfPresent(Path.Combine(targetDir, "config.json"), backupDir);

        var config = File.ReadAllText(sourceConfig);
        var port = DetectClashPort();
        config = ReplaceProxyPort(config, port);
        File.Copy(sourceDll, Path.Combine(targetDir, "version.dll"), true);
        File.WriteAllText(Path.Combine(targetDir, "config.json"), config, new System.Text.UTF8Encoding(false));
        Log("Copied files; proxy port=" + port);

        Process.Start(new ProcessStartInfo
        {
            FileName = Path.Combine(targetDir, "Antigravity.exe"),
            WorkingDirectory = targetDir,
            UseShellExecute = true
        });
        Log("Antigravity started");
        return "已完成。\r\n代理端口：127.0.0.1:" + port + "\r\n安装目录：" + targetDir + "\r\n备份目录：" + backupDir + "\r\n\r\nAntigravity 已启动，可以保持 TUN/虚拟网卡关闭。";
    }

    private static string FindAntigravityDirectory()
    {
        var candidates = new List<string>();
        var local = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
        foreach (var root in new[] { Path.Combine(local, "Programs"), programFiles, programFilesX86 })
        {
            foreach (var name in new[] { "Antigravity", "antigravity" })
            {
                var path = Path.Combine(root, name, "Antigravity.exe");
                if (File.Exists(path)) candidates.Add(Path.GetDirectoryName(path));
            }
        }
        foreach (var hive in new[] { Registry.CurrentUser, Registry.LocalMachine })
        {
            foreach (var keyName in new[] { @"Software\Microsoft\Windows\CurrentVersion\Uninstall", @"Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall" })
            {
                using (var root = hive.OpenSubKey(keyName))
                {
                    if (root == null) continue;
                    foreach (var subName in root.GetSubKeyNames())
                    {
                        using (var sub = root.OpenSubKey(subName))
                        {
                            var display = Convert.ToString(sub == null ? null : sub.GetValue("DisplayName"));
                            var location = Convert.ToString(sub == null ? null : sub.GetValue("InstallLocation"));
                            if (display.IndexOf("Antigravity", StringComparison.OrdinalIgnoreCase) >= 0 && !String.IsNullOrWhiteSpace(location))
                            {
                                var exe = Path.Combine(location, "Antigravity.exe");
                                if (File.Exists(exe)) candidates.Add(location);
                            }
                        }
                    }
                }
            }
        }
        return candidates.Distinct(StringComparer.OrdinalIgnoreCase).OrderByDescending(Directory.GetLastWriteTimeUtc).FirstOrDefault();
    }

    private static void StopAntigravityProcesses()
    {
        foreach (var processName in new[] { "Antigravity", "language_server", "language_server_windows_x64", "agy" })
        {
            foreach (var process in Process.GetProcessesByName(processName))
            {
                try
                {
                    Log("Stopping " + process.ProcessName + " (" + process.Id + ")");
                    if (!process.HasExited) process.CloseMainWindow();
                    if (!process.WaitForExit(1200) && !process.HasExited) process.Kill();
                }
                catch (Exception ex) { Log("Could not stop " + processName + ": " + ex.Message); }
                finally { process.Dispose(); }
            }
        }
    }

    private static void BackupIfPresent(string path, string backupDir)
    {
        if (!File.Exists(path)) return;
        File.Copy(path, Path.Combine(backupDir, Path.GetFileName(path)), true);
        Log("Backed up " + path);
    }

    private static int DetectClashPort()
    {
        var roots = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "io.github.clash-verge-rev.clash-verge-rev"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "clash_win")
        };
        foreach (var root in roots)
        {
            foreach (var file in new[] { "clash-verge.yaml", "config.yaml", "verge.yaml" })
            {
                var path = Path.Combine(root, file);
                if (!File.Exists(path)) continue;
                var text = File.ReadAllText(path);
                var match = Regex.Match(text, @"(?m)^\s*(?:mixed-port|socks-port|port)\s*:\s*(\d+)");
                int port;
                if (match.Success && Int32.TryParse(match.Groups[1].Value, out port) && port > 0 && port < 65536) return port;
            }
        }
        return 7897;
    }

    private static string ReplaceProxyPort(string config, int port)
    {
        var proxyBlock = Regex.Match(config, @"(""proxy""\s*:\s*\{.*?)(""port""\s*:\s*)\d+", RegexOptions.Singleline);
        if (proxyBlock.Success) return config.Substring(0, proxyBlock.Index) + proxyBlock.Groups[1].Value + proxyBlock.Groups[2].Value + port + config.Substring(proxyBlock.Index + proxyBlock.Length);
        return config;
    }

    private static void Log(string message)
    {
        try { Directory.CreateDirectory(LogDir); File.AppendAllText(LogFile, DateTime.Now.ToString("s") + " " + message + Environment.NewLine); } catch { }
    }

    private static string ResolveDataDirectory()
    {
        var candidates = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AntigravityProxyLauncher"),
            Path.Combine(Path.GetTempPath(), "AntigravityProxyLauncher"),
            Path.Combine(AppDir, "data")
        };
        foreach (var candidate in candidates)
        {
            try { Directory.CreateDirectory(candidate); return candidate; } catch { }
        }
        return AppDir;
    }
}
