using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace VisionHelper.Services
{
    public class OpenAiService
    {
        private readonly string _apiKey;
        private readonly string _openAiEndpoint = "https://api.openai.com/v1/chat/completions";

        public OpenAiService(string apiKey)
        {
            _apiKey = apiKey;
        }

        public async Task<string> DescribeImageAsync(string imagePath)
        {
            if (!File.Exists(imagePath))
                return "Erro: Arquivo da imagem não encontrado.";

            try
            {
                string base64Image = ConvertImageToBase64(imagePath);

                var requestBody = new
                {
                    model = "gpt-4o-mini",
                    messages = new[]
                    {
                        new
                        {
                            role = "user",
                            content = new object[]
                            {
                                new { type = "image_url", image_url = new { url = $"data:image/png;base64,{base64Image}" } },
                                new { type = "text", text = "Descreva a imagem em detalhes" }
                            }
                        }
                    },
                    max_tokens = 200,
                };

                string jsonRequest = JsonSerializer.Serialize(requestBody);

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync(_openAiEndpoint, content);

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        var jsonDoc = JsonDocument.Parse(jsonResponse);
                        var message = jsonDoc.RootElement
                                             .GetProperty("choices")[0]
                                             .GetProperty("message")
                                             .GetProperty("content")
                                             .GetString();
                        return message;
                    }
                    else
                    {
                        return $"Erro na requisição: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}";
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Erro ao processar a imagem: {ex.Message}";
            }
        }

        private string ConvertImageToBase64(string imagePath)
        {
            byte[] imageBytes = File.ReadAllBytes(imagePath);
            return Convert.ToBase64String(imageBytes);
        }
    }
}