# KOYA-APP: Status Report & Backlog (2026-04-30)

## 📋 Status projektu
Aplikacja została w pełni przebudowana wizualnie na styl achromatyczny (cybernetyczny minimalizm) i zaktualizowana o zaawansowane funkcje audio oraz poprawione animacje mechaniczne.

### ✅ Co działa
- **Architektura**: Polimorficzna serializacja JSON (Actions) przy użyciu .NET 8 (JsonDerivedType).
- **Obsługa HID (Hardware)**: 
    - Zrefaktoryzowany `HidBackend.cs` z automatyczną rekonfektacją i stabilnym odczytem RawHID.
    - **Auto-Picker**: Fizyczne naciśnięcie nieprzypisanego przycisku automatycznie otwiera okno konfiguracji akcji.
    - Zoptymalizowane wyszukiwanie kontrolek (Button Cache) dla natychmiastowej reakcji na hardware.
- **Design (Achromatic Cyber)**: 
    - Całkowita rezygnacja z kolorów na rzecz czerni, bieli i szarości.
    - Naprawione ładowanie ikon (Segoe MDL2 Assets).
- **Intro & UI**: Intro "KOYA" z sekwencyjnym pojawianiem się liter.
- **System**: Stabilny build, działający Tray Icon i system samouczka.
- **Nowości (v1.4.0)**:
    - **Spotify Integration**: Dedykowane akcje "Polub" (UI Automation) oraz "Otwórz Spotify".
    - **Visual & Audio Feedback**: Subtelne kliknięcia dźwiękowe (NAudio) oraz animacje przy każdym naciśnięciu.
    - **Macro Recorder**: Zaawansowany rejestrator sekwencji klawiszy z zachowaniem opóźnień.
    - **Virtual Pad Mode**: Kliknięcie w UI teraz wykonuje akcję, a prawy przycisk służy do edycji.

### ❌ Do zrobienia (Backlog)
1. **Dynamic Icons**: Wyświetlanie okładki albumu ze Spotify na przycisku.
2. **Profiles**: Możliwość przełączania całych zestawów akcji (np. Game/Work).


---
*Zaktualizowane przez vault-gpt 🤍*
