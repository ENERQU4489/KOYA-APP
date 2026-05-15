# KOYA - Achromatic Control Hub ⌨️🌑

**KOYA** to zaawansowana aplikacja sterująca typu Stream Deck, zaprojektowana z myślą o minimalistycznej estetyce (Achromatic Cyberpunk) i maksymalnej funkcjonalności. Integruje fizyczny hardware (Arduino/Custom HID) z wirtualnym panelem sterowania i asystentem AI.

## 🚀 Kluczowe Funkcje

- **Multi-Action Hub**: 15 konfigurowalnych przycisków w UI + wsparcie dla gałek analogowych (Enkodery).
- **Potężny Asystent AI**: Zintegrowana obsługa trzech gigantów:
  - **Google Gemini**: Inteligentny fallback między v1 i v1beta dla maksymalnej dostępności.
  - **Groq**: Błyskawiczne modele Llama 3.3 (idealne jako darmowa alternatywa).
  - **OpenAI**: Pełna moc GPT-4o z analizą obrazu (zrzuty ekranu).
- **Automatyzacja & Akcje**:
  - Sterowanie Spotify (Like/Open).
  - Soundboard (MP3/WAV) z wbudowaną przeglądarką plików 📁.
  - Zaawansowany Rejestrator Makr (klawiatura + mysz).
  - Skróty klawiszowe, PowerShell, Mikser Głośności, Jasność Monitora.
- **Produkcyjna Jakość**:
  - **Single-File EXE**: Przenośna aplikacja, która odpali się na każdym PC bez instalacji .NET.
  - **Animated Feedback**: Płynne animacje przycisków i system powiadomień popup w rogu ekranu.
  - **Tray Support**: Działa w tle, z szybkim dostępem z paska zadań.

## 🛠️ Instalacja & Build

Aplikacja jest dystrybuowana jako **self-contained executable**.

1. Pobierz `KOYA-APP.exe` z folderu wydań.
2. Uruchom – wszystkie zależności są zawarte w pliku.
3. (Opcjonalnie) Skonfiguruj klucze AI w panelu asystenta, aby odblokować wsparcie techniczne na żywo.

### Budowanie ze źródeł:
Wymagany .NET 8 SDK.
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o "./Publish"
```

## 🔌 Hardware (Opcjonalnie)
KOYA łączy się z urządzeniami RawHID. Domyślnie współpracuje ze skryptem `arduino_koya.ino` zawartym w repozytorium.
- **Auto-Picker**: Naciśnięcie fizycznego przycisku bez przypisanej akcji automatycznie otwiera menu konfiguracji.

## 🛡️ Bezpieczeństwo
Wszystkie konfiguracje (w tym klucze API) są przechowywane lokalnie w pliku `config.json` w folderze aplikacji. Repozytorium zawiera `.gitignore` chroniący Twoje prywatne dane.

---
*Created with 🖤 for the Cyberpunk Era.*
