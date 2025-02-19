using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace VisionHelper.Services
{
    public class TextToSpeechService
    {
        public void Speak(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                Console.WriteLine("Nenhum texto para falar.");
                return;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                SpeakWindows(text);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                SpeakMacOS(text);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                SpeakLinux(text);
            }
            else
            {
                Console.WriteLine("Sistema operacional não suportado.");
            }
        }

        private void SpeakWindows(string text)
        {
            Console.WriteLine("Falando no Windows...");
            try
            {
                string escapedText = text.Replace("\"", "\\\"");
                string command = $"Add-Type –AssemblyName System.speech; " +
                                 $"$synth = New-Object System.Speech.Synthesis.SpeechSynthesizer; " +
                                 $"$synth.Speak(\"{escapedText}\")";
                
                Process.Start(new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = $"-Command \"{command}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao falar no Windows: {ex.Message}");
            }
        }

        private void SpeakMacOS(string text)
        {
            Console.WriteLine("Falando no macOS...");
            try
            {
                Process.Start("say", text);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao falar no macOS: {ex.Message}");
            }
        }

        private void SpeakLinux(string text)
        {
            Console.WriteLine("Falando no Linux...");
            try
            {
                Process.Start("espeak", $"\"{text}\"");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao falar no Linux: {ex.Message}");
            }
        }
    }
}