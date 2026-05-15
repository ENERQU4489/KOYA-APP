using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace KOYA_APP
{
    public class GeminiService
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        public GeminiService(string apiKey)
        {
            _apiKey = apiKey;
            _httpClient = new HttpClient();
        }

        public async Task<string> GetAIResponse(string prompt, List<string> filePaths, byte[]? screenshotBytes = null)
        {
            try
            {
                string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_apiKey}";

                var parts = new List<object>();

                // 1. Złożony System Prompt (Wiedza o KOYA)
                string systemInstruction = @"Jesteś KOYA AI – oficjalny asystent wirtualnego Stream Decka 'KOYA-APP'. 
TWOJA WIEDZA O APLIKACJI:
- KOYA posiada 15 przycisków w UI. Lewy klik konfiguruje, prawy wykonuje.
- Obsługiwane akcje: Spotify (Like/Open), Soundboard (MP3/WAV), Makra (nagrywanie ruchów myszy i klawiszy), Skróty klawiszowe, PowerShell, Screenshoty, Mixer głośności.
- Hardware: Łączy się przez RawHID (Arduino/Custom). Jeśli hardware jest niepodłączony, wyświetla się overlay 'PLEASE CONNECT', który można kliknąć, aby wejść w tryb Offline.
- Nagrywanie makr: Wymaga naciśnięcia fizycznego przycisku na urządzeniu, aby rozpocząć/zatrzymać zapis.
- Konfiguracja: Wszystkie ustawienia i akcje są zapisywane w pliku 'config.json'.

TWOJE ZADANIA:
- Pomagaj użytkownikowi w konfiguracji akcji.
- Analizuj błędy na podstawie zrzutów ekranu lub logów, które użytkownik załączy.
- Odpowiadaj konkretnie, technicznie i w sposób futurystyczny/cyberpunkowy, ale profesjonalny.
- Jeśli użytkownik pyta o coś niezwiązanego z systemem/pracą, staraj się być zwięzły.";

                parts.Add(new { text = systemInstruction + "\n\nZAPYTANIE UŻYTKOWNIKA: " + prompt });

                // 2. Dodaj pliki jako tekst
                foreach (var path in filePaths)
                {
                    if (File.Exists(path))
                    {
                        string content = File.ReadAllText(path);
                        parts.Add(new { text = $"\n--- PLIK: {Path.GetFileName(path)} ---\n{content}" });
                    }
                }

                // 3. Dodaj screenshot (Inline Data)
                if (screenshotBytes != null)
                {
                    parts.Add(new { 
                        inline_data = new { 
                            mime_type = "image/jpeg", 
                            data = Convert.ToBase64String(screenshotBytes) 
                        } 
                    });
                }

                var requestBody = new
                {
                    contents = new[] { new { parts = parts } }
                };

                string jsonRequest = JsonConvert.SerializeObject(requestBody);
                var response = await _httpClient.PostAsync(url, new StringContent(jsonRequest, Encoding.UTF8, "application/json"));
                string jsonResponse = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    dynamic? result = JsonConvert.DeserializeObject(jsonResponse);
                    return result?.candidates[0].content.parts[0].text ?? "Błąd: Pusta odpowiedź Gemini.";
                }
                else
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        return "BŁĄD KRYTYCZNY: Twój klucz API jest nieprawidłowy lub wygasł. Użyj przycisku resetu w panelu asystenta, aby wpisać nowy klucz.";
                    }
                    if ((int)response.StatusCode == 429)
                    {
                        return "LIMIT PRZEKROCZONY: Wysyłasz zapytania zbyt szybko. Poczekaj chwilę i spróbuj ponownie.";
                    }
                    return $"Błąd Google API (Status: {response.StatusCode}): \n{jsonResponse}";
                }
            }
            catch (Exception ex)
            {
                return "Błąd połączenia: " + ex.Message;
            }
        }
    }
}
