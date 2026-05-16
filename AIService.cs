using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace KOYA_APP
{
    public enum AIProvider { Google, Groq, OpenAI }

    public class AIService
    {
        private readonly string? _geminiKey;
        private readonly string? _groqKey;
        private readonly string? _openAIKey;
        private static readonly HttpClient _httpClient = new HttpClient();

        public AIService(string? geminiKey, string? groqKey, string? openAIKey)
        {
            _geminiKey = geminiKey;
            _groqKey = groqKey;
            _openAIKey = openAIKey;
        }

        public async Task<string> GetAIResponse(string prompt, List<string> filePaths, byte[]? screenshotBytes = null, AIProvider preferredProvider = AIProvider.Google)
        {
            if (string.IsNullOrWhiteSpace(prompt)) prompt = "Witaj, opowiedz krótko o sobie.";

            if (preferredProvider == AIProvider.Google && !string.IsNullOrEmpty(_geminiKey))
                return await GetGeminiResponse(prompt, filePaths, screenshotBytes);
            
            if (preferredProvider == AIProvider.Groq && !string.IsNullOrEmpty(_groqKey))
                return await GetGroqResponse(prompt, filePaths);

            if (preferredProvider == AIProvider.OpenAI && !string.IsNullOrEmpty(_openAIKey))
                return await GetOpenAIResponse(prompt, filePaths, screenshotBytes);

            return "Błąd: Nie skonfigurowano klucza API dla wybranego dostawcy.";
        }

        private async Task<string> GetGeminiResponse(string prompt, List<string> filePaths, byte[]? screenshotBytes)
        {
            var attempts = new[] {
                new { Ver = "v1beta", Model = "gemini-2.5-flash" },
                new { Ver = "v1beta", Model = "gemini-2.0-flash" },
                new { Ver = "v1beta", Model = "gemini-1.5-flash" }
            };

            string lastError = "";
            foreach (var attempt in attempts)
            {
                try
                {
                    string url = $"https://generativelanguage.googleapis.com/{attempt.Ver}/models/{attempt.Model}:generateContent?key={_geminiKey}";
                    
                    var parts = new List<object> { 
                        new { text = "Jesteś KOYA AI - asystentem technicznym. Odpowiadaj krótko i konkretnie.\n" },
                        new { text = "ZAPYTANIE UŻYTKOWNIKA: " + prompt } 
                    };

                    foreach (var path in filePaths)
                    {
                        if (File.Exists(path))
                            parts.Add(new { text = $"\n--- PLIK: {Path.GetFileName(path)} ---\n{File.ReadAllText(path)}" });
                    }
                    if (screenshotBytes != null)
                        parts.Add(new { inlineData = new { mimeType = "image/jpeg", data = Convert.ToBase64String(screenshotBytes) } });

                    var body = new { contents = new[] { new { role = "user", parts = parts } } };

                    using (var request = new HttpRequestMessage(HttpMethod.Post, url))
                    {
                        request.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
                        var response = await _httpClient.SendAsync(request);
                        string json = await response.Content.ReadAsStringAsync();

                        if (response.IsSuccessStatusCode)
                        {
                            dynamic? result = JsonConvert.DeserializeObject(json);
                            return result?.candidates[0].content.parts[0].text ?? "Pusta odpowiedź Google.";
                        }
                        lastError = $"[{attempt.Model} {attempt.Ver}] {json}";
                    }
                }
                catch (Exception ex) { lastError = ex.Message; }
            }
            return "Błąd Google: " + lastError;
        }

        private async Task<string> GetGroqResponse(string prompt, List<string> filePaths)
        {
            var models = new[] { "llama-3.3-70b-specdec", "llama-3.1-8b-instant", "mixtral-8x7b-32768" };
            string lastError = "";

            foreach (var model in models)
            {
                try
                {
                    string url = "https://api.groq.com/openai/v1/chat/completions";
                    var contentBuilder = new StringBuilder();
                    contentBuilder.AppendLine(prompt);
                    foreach (var path in filePaths)
                    {
                        if (File.Exists(path))
                            contentBuilder.AppendLine($"\n--- PLIK: {Path.GetFileName(path)} ---\n{File.ReadAllText(path)}");
                    }

                    var body = new {
                        model = model,
                        messages = new[] {
                            new { role = "system", content = "Jesteś KOYA AI - asystentem technicznym." },
                            new { role = "user", content = contentBuilder.ToString() }
                        }
                    };

                    using (var request = new HttpRequestMessage(HttpMethod.Post, url))
                    {
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _groqKey);
                        request.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
                        var response = await _httpClient.SendAsync(request);
                        string json = await response.Content.ReadAsStringAsync();

                        if (response.IsSuccessStatusCode)
                        {
                            dynamic? result = JsonConvert.DeserializeObject(json);
                            return result?.choices[0].message.content ?? "Pusta odpowiedź Groq.";
                        }
                        lastError = $"[{model}] {json}";
                    }
                }
                catch (Exception ex) { lastError = ex.Message; }
            }
            return "Błąd Groq: " + lastError;
        }

        private async Task<string> GetOpenAIResponse(string prompt, List<string> filePaths, byte[]? screenshotBytes)
        {
            try
            {
                string url = "https://api.openai.com/v1/chat/completions";
                var userContent = new List<object> { new { type = "text", text = prompt } };

                foreach (var path in filePaths)
                {
                    if (File.Exists(path))
                        userContent.Add(new { type = "text", text = $"\n--- PLIK: {Path.GetFileName(path)} ---\n{File.ReadAllText(path)}" });
                }

                if (screenshotBytes != null)
                {
                    userContent.Add(new { 
                        type = "image_url", 
                        image_url = new { url = $"data:image/jpeg;base64,{Convert.ToBase64String(screenshotBytes)}" } 
                    });
                }

                var body = new {
                    model = "gpt-4o",
                    messages = new object[] {
                        new { role = "system", content = (object)"Jesteś KOYA AI - asystentem technicznym." },
                        new { role = "user", content = (object)userContent }
                    }
                };

                using (var request = new HttpRequestMessage(HttpMethod.Post, url))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _openAIKey);
                    request.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
                    var response = await _httpClient.SendAsync(request);
                    string json = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        dynamic? result = JsonConvert.DeserializeObject(json);
                        return result?.choices[0].message.content ?? "Pusta odpowiedź OpenAI.";
                    }
                    return "Błąd OpenAI: " + json;
                }
            }
            catch (Exception ex) { return "Błąd połączenia OpenAI: " + ex.Message; }
        }
    }
}
