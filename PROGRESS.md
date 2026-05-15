# KOYA-APP: Status Report & Backlog (2026-05-16)

## 📋 Status projektu
Aplikacja osiągnęła fazę produkcyjną (Final Release). Została zoptymalizowana pod kątem dystrybucji jako pojedynczy, przenośny plik wykonywalny z ulepszonym systemem feedbacku dla użytkownika oraz potężnym asystentem AI.

### ✅ Co działa (Aktualizacja Final)
- **Dystrybucja**: 
    - Build typu **Single-File EXE** (Self-contained, zawiera .NET 8 runtime).
    - Brak zależności zewnętrznych – aplikacja odpala się na każdym PC z Windowsem.
- **Multi-Provider AI Assistant**:
    - **Obsługa 3 dostawców**: Google Gemini (z fallbackiem v1/v1beta), Groq (Llama 3.3) oraz OpenAI (GPT-4o).
    - **Naprawiono błędy JSON**: Pełna zgodność z camelCase REST API (inlineData/mimeType).
    - **Screenshoty & Pliki**: Możliwość przesyłania zrzutów ekranu i plików do analizy przez AI.
    - **Dynamiczny wybór**: Przełączanie między modelami w czasie rzeczywistym z poziomu UI.
- **UI/UX Improvements**:
    - **File Explorer Integration**: Przycisk 📁 do wyboru dźwięków, aplikacji i skryptów. 
    - **Notification Popups**: Animowane powiadomienia w prawym górnym rogu o wykonanych akcjach.
    - **Notification Toggle**: Ikona dzwonka 🔔 do sterowania popupami.
- **Architektura**: Polimorficzna serializacja JSON (Actions) przy użyciu .NET 8.
- **Obsługa HID (Hardware)**: Zrefaktoryzowany `HidBackend.cs` ze stabilnym odczytem RawHID.

### 🚀 Wydania (Desktop)
- `FINAL x2`: Ostateczna wersja produkcyjna (Single-File).

### ❌ Do zrobienia (Backlog)
1. **Dynamic Icons**: Wyświetlanie okładki albumu ze Spotify na przycisku.
2. **Profiles**: Możliwość przełączania całych zestawów akcji (np. Game/Work).


---
*Zaktualizowane przez vault-gpt 🤍*
