using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using VisionHelper.Services;

namespace VisionHelperGUI
{
    public partial class MainWindow : Window
    {
        private readonly ScreenCaptureService _screenCaptureService;
        private readonly OpenAiService _openAiService;
        private readonly TextToSpeechService _textToSpeechService;

        public MainWindow()
        {
            InitializeComponent();

            
            EnvService.LoadEnvVariables();
            string apiKey = EnvService.GetApiKey();

            
            _screenCaptureService = new ScreenCaptureService();
            _openAiService = new OpenAiService(apiKey);
            _textToSpeechService = new TextToSpeechService();

            
            CaptureButton.Click += OnCaptureClick;
            ClearButton.Click += OnClearClick;

            
            this.Deactivated += OnWindowBlur;
        }

        private async void OnWindowBlur(object? sender, EventArgs e)
        {
            Console.WriteLine("Janela perdeu o foco. Capturando a tela do monitor ativo...");
            ResultText.Text = "Capturando tela do monitor ativo...";

            string imagePath = _screenCaptureService.CaptureScreen();
            if (!string.IsNullOrEmpty(imagePath))
            {
                ResultText.Text = "Enviando para análise...";
                string description = await _openAiService.DescribeImageAsync(imagePath);
                ResultText.Text = $"Descrição: {description}";

                
                ClearButton.IsVisible = true;

                
                ResizeWindowToContent();
            }
            else
            {
                ResultText.Text = "Erro ao capturar.";
            }
        }

        private async void OnCaptureClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            ResultText.Text = "Capturando tela...";

            string imagePath = _screenCaptureService.CaptureScreen();
            if (!string.IsNullOrEmpty(imagePath))
            {
                ResultText.Text = "Enviando para análise...";
                string description = await _openAiService.DescribeImageAsync(imagePath);
                ResultText.Text = $"Descrição: {description}";

                
                _textToSpeechService.Speak(description);

                
                ClearButton.IsVisible = true;

                
                ResizeWindowToContent();
            }
            else
            {
                ResultText.Text = "Erro ao capturar.";
            }
        }

        private void OnClearClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            ResultText.Text = "Clique para capturar";
            ClearButton.IsVisible = false;

            this.Width = 100;
            this.Height = 120;
        }

        private void ResizeWindowToContent()
        {
            this.Width = Math.Max(ResultText.DesiredSize.Width + 20, 150);
            this.Height = Math.Max(ResultText.DesiredSize.Height + 100, 150);
        }
    }
}