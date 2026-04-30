# KOYA-APP: Status Report & Backlog (2026-04-30)

## 📋 Status projektu
Aplikacja została w pełni przebudowana wizualnie na styl achromatyczny (cybernetyczny minimalizm) i zaktualizowana o zaawansowane funkcje audio oraz poprawione animacje mechaniczne.

### ✅ Co działa
- **Architektura**: Polimorficzna serializacja JSON (Actions) przy użyciu .NET 8 (JsonDerivedType).
- **Design (Achromatic Cyber)**: 
    - Całkowita rezygnacja z kolorów na rzecz czerni, bieli i szarości.
    - Ostre kąty (CornerRadius = 0) dla wszystkich elementów UI.
    - Intro "KOYA" z sekwencyjnym pojawianiem się liter i cyfrowym skanowaniem.
- **Interakcja (Potencjometry)**: 
    - Niezależna animacja pierścienia orbitalnego wokół gałek.
    - Obsługa zdarzeń MouseWheel dla precyzyjnej kontroli analogowej.
- **GitHub Sync**: 
    - Dedytkowany przycisk synchronizacji konfiguracji (`config.json`) z repozytorium GitHub.
    - Automatyczne wykrywanie głównego folderu repozytorium i obsługa ścieżek relatywnych.
- **Audio & Akcje**: 
    - `MuteMicrophoneAction` z możliwością wyboru konkretnego urządzenia.
    - Dynamiczne filtrowanie akcji w `ActionPicker` (analogowe vs cyfrowe).
    - `ConfigurationManager` automatycznie zapisuje i wczytuje stan przycisków.
- **System**: Pełna integracja z Git, stabilny build i działający Tray Icon.

### ❌ Do zrobienia (Backlog)
1. **Spotify Integration**: Implementacja akcji dedykowanych dla Spotify.
2. **Visual Feedback**: Dodanie subtelnych efektów dźwiękowych przy kliknięciach.
3. **Macro Recorder**: Możliwość nagrywania ciągów klawiszy jako jednej akcji.

---
*Zaktualizowane przez vault-gpt 🤍*
