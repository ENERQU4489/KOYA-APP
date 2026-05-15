# KOYA-APP

Wirtualny makropad / Stream Deck na Windows. Aplikacja WPF (.NET 8) z 15 konfigurowalnymi przyciskami, z których każdy może wykonywać wybraną akcję systemową — bez fizycznego urządzenia.

🚀 **Główne Funkcje**
Aplikacja wspiera szeroki wachlarz akcji, które można przypisać do przycisków lub skrótów:
- **Zarządzanie Multimediami**: Play/Pause, następny/poprzedni utwór, regulacja głośności.
- **Integracja Spotify**: Dedykowane akcje "Polub" oraz "Otwórz Spotify".
- **Macro Recorder**: Nagrywanie sekwencji klawiszy z zachowaniem opóźnień.
- **Visual & Audio Feedback**: Kliknięcia dźwiękowe oraz animacje przycisków.
- **Narzędzia Systemowe**: Screenshoty, Task Manager, Alt-Tab, wyciszanie mikrofonu.

---

## Wymagania

| Wymaganie | Wersja |
|-----------|--------|
| .NET SDK | 8.0+ (net8.0-windows) |
| System operacyjny | Windows 10 / 11 (x64) |
| NAudio | 2.3.0+ (NuGet) |
| System.Windows.Extensions | 6.0.0+ (NuGet) |

---

## Instalacja

```bash
git clone https://github.com/ENERQU4489/KOYA-APP.git
cd KOYA-APP
dotnet restore
dotnet build
dotnet run
```

Lub otwórz `KOYA-APP.csproj` w Visual Studio 2022+ i uruchom przez F5.

---

## Użycie

1. Uruchom aplikację — pojawi się panel z 15 przyciskami oraz przyciskami Volume Up / Volume Down.
2. Kliknij dowolny **pusty przycisk** — otworzy się `ActionPicker` z listą dostępnych akcji.
3. Wybierz akcję, skonfiguruj opcjonalne parametry (ścieżka do `.exe`, skrót klawiszowy, urządzenie audio), kliknij **Zastosuj**.
4. Przycisk zostanie oznaczony nazwą wybranej akcji. Każde kolejne kliknięcie **wykonuje** przypisaną akcję.
5. **Prawy przycisk myszy** na przypisanym przycisku pozwala na ponowną edycję akcji.
6. Pasek tytułu jest przeciągalny; przyciski **−** i **✕** minimalizują / zamykają okno.

---

## Architektura

```
KOYA-APP/
├── App.xaml / App.xaml.cs          # Punkt wejścia aplikacji WPF
├── MainWindow.xaml / .cs           # Główny panel 15 przycisków + Volume
├── ActionPicker.xaml / .cs         # Dialog wyboru i konfiguracji akcji
├── IStreamDeckAction.cs            # Interfejs bazowy wszystkich akcji
├── MacroAction.cs                  # Obsługa makr (sekwencji klawiszy)
├── SpotifyLikeAction.cs            # Integracja ze Spotify (UI Automation)
└── KOYA-APP.csproj                 # Definicja projektu .NET 8 WPF
```

### Interfejs `IStreamDeckAction`

Każda akcja implementuje `IStreamDeckAction`:

```csharp
public interface IStreamDeckAction
{
    string Name { get; }
    string Description { get; }
    string Icon { get; }
    void Execute();
    void ExecuteAnalog(bool direction);
}
```

---

## Zależności NuGet

```xml
<PackageReference Include="NAudio" Version="2.3.0" />
<PackageReference Include="HidSharp" Version="2.6.4" />
<PackageReference Include="System.Windows.Extensions" Version="6.0.0" />
```

---

## Licencja

Projekt rozwijany przez ENERQU4489. Wszelkie prawa zastrzeżone.
