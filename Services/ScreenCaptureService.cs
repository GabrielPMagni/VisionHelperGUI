using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace VisionHelper.Services
{
    public class ScreenCaptureService
    {
        public string CaptureScreen()
        {
            Console.WriteLine("Clique na tela que deseja capturar...");
            WaitForUserClick(); 

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return CaptureMonitorWindows();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return CaptureMonitorMacOS();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return CaptureMonitorLinux();
            }
            else
            {
                Console.WriteLine("Sistema operacional não suportado para captura de tela.");
                return null;
            }
        }

        
        private void WaitForUserClick()
        {
            Console.WriteLine("Aguardando clique...");
            Thread.Sleep(1000); 
        }

        
        [System.Runtime.Versioning.SupportedOSPlatform("windows")]
        private string CaptureMonitorWindows()
        {
            try
            {
                IntPtr hwnd = GetForegroundWindow();
                if (hwnd == IntPtr.Zero)
                    throw new Exception("Nenhuma janela ativa detectada.");

                RECT rect;
                GetWindowRect(hwnd, out rect);
                int x = rect.Left;
                int y = rect.Top;

                
                IntPtr hMonitor = MonitorFromPoint(new POINT { x = x, y = y }, 2);
                MONITORINFO monitorInfo = new MONITORINFO();
                monitorInfo.cbSize = Marshal.SizeOf(typeof(MONITORINFO));
                GetMonitorInfo(hMonitor, ref monitorInfo);

                
                int width = monitorInfo.rcMonitor.Right - monitorInfo.rcMonitor.Left;
                int height = monitorInfo.rcMonitor.Bottom - monitorInfo.rcMonitor.Top;

                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "screenshot.png");

                using (Bitmap screenshot = new Bitmap(width, height))
                using (Graphics graphics = Graphics.FromImage(screenshot))
                {
                    graphics.CopyFromScreen(monitorInfo.rcMonitor.Left, monitorInfo.rcMonitor.Top, 0, 0, new System.Drawing.Size(width, height));
                    screenshot.Save(path, System.Drawing.Imaging.ImageFormat.Png);
                }

                return path;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao capturar o monitor no Windows: {ex.Message}");
                return null;
            }
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hwnd, out RECT rect);

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromPoint(POINT pt, uint flags);

        [DllImport("user32.dll")]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        private struct RECT { public int Left, Top, Right, Bottom; }
        private struct POINT { public int x, y; }
        private struct MONITORINFO
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
        }

        
        private string CaptureMonitorMacOS()
        {
            try
            {
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "screenshot.png");

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "/usr/sbin/screencapture",
                    Arguments = $"-i {path}", 
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = new Process { StartInfo = psi })
                {
                    process.Start();
                    process.WaitForExit();
                }

                if (File.Exists(path))
                    return path;
                else
                    throw new Exception("Erro ao capturar a tela no macOS.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro na captura de tela no macOS: {ex.Message}");
                return null;
            }
        }

        
        private string CaptureMonitorLinux()
        {
            try
            {
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "screenshot.png");

                
                ProcessStartInfo psiGetMonitor = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = "-c \"xdotool click 1; xrandr | grep ' connected' | awk '{print $1}'\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                string monitorName;
                using (Process process = new Process { StartInfo = psiGetMonitor })
                {
                    process.Start();
                    monitorName = process.StandardOutput.ReadToEnd().Trim();
                    process.WaitForExit();
                }

                if (string.IsNullOrEmpty(monitorName))
                    throw new Exception("Não foi possível identificar o monitor ativo.");

                
                string command = $"import -window root -crop $(xrandr --query | grep {monitorName} | awk '{{print $3}}') {path}";

                ProcessStartInfo psiCapture = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{command}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = new Process { StartInfo = psiCapture })
                {
                    process.Start();
                    process.WaitForExit();
                }

                if (File.Exists(path))
                    return path;
                else
                    throw new Exception("Erro ao capturar o monitor no Linux.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro na captura de tela no Linux: {ex.Message}");
                return null;
            }
        }
    }
}