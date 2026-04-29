# KOYA-APP

Wirtualny makropad / Stream Deck na Windows. Aplikacja WPF (.NET 8) z 15 konfigurowalnymi przyciskami, z których każdy może wykonywać wybraną akcję systemową — bez fizycznego urządzenia.

---

## Wymagania

| Wymaganie | Wersja |
|-----------|--------|
| .NET SDK | 8.0+ (net8.0-windows) |
| System operacyjny | Windows 10 / 11 (x64) |
| NAudio | 2.2.0+ (NuGet) |
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
3. Wybierz akcję, skonfiguruj opcjonalne parametry (ścieżka do `.exe`, skrót klawiszowy, urządzenie audio), kliknij **Zatwierdź**.
4. Przycisk zostanie oznaczony nazwą wybranej akcji. Każde kolejne kliknięcie **wykonuje** przypisaną akcję.
5. Pasek tytułu jest przeciągalny; przyciski **−** i **✕** minimalizują / zamykają okno.

---

## Dostępne akcje

| Akcja | Opis | Konfiguracja |
|-------|------|--------------|
| `PlayPauseAction` | Play / Pause odtwarzania | — |
| `NextTrackAction` | Następny utwór | — |
| `PreviousTrackAction` | Poprzedni utwór | — |
| `VolumeAction` | Zmiana głośności systemu | — |
| `MuteSpeakerAction` | Wycisz / odcisz głośniki | — |
| `MuteMicrophoneAction` | Wycisz / odcisz mikrofon | — |
| `SelectMicAction` | Ustaw domyślne urządzenie wejściowe | Wybór z listy aktywnych mikrofonów |
| `CopyAction` | Ctrl+C (kopiuj) | — |
| `PasteAction` | Ctrl+V (wklej) | — |
| `ScreenshotAction` | Zrzut ekranu (PrintScreen) | — |
| `TaskManagerAction` | Otwórz Menedżer zadań | — |
| `CloseWindowAction` | Zamknij aktywne okno (Alt+F4) | — |
| `FullscreenAction` | Przełącz pełny ekran (F11) | — |
| `AltTabAction` | Przełącz okno (Alt+Tab) | — |
| `CustomShortcutAction` | Dowolna kombinacja klawiszy | Nagrywanie klawiszy w UI |
| `OpenAppAction` | Uruchom aplikację z dysku | Ścieżka do `.exe` przez OpenFileDialog |

---

## Architektura

```
KOYA-APP/
├── App.xaml / App.xaml.cs          # Punkt wejścia aplikacji WPF
├── MainWindow.xaml / .cs           # Główny panel 15 przycisków + Volume
├── ActionPicker.xaml / .cs         # Dialog wyboru i konfiguracji akcji
├── IStreamDeckAction.cs            # Interfejs bazowy wszystkich akcji
├── VolumeAction.cs                 # NAudio – zmiana głośności
├── MuteMicrophoneAction.cs         # NAudio – wyciszenie mikrofonu
├── MuteSpeakerAction.cs            # NAudio – wyciszenie głośników
├── SelectMicAction.cs              # NAudio – wybór urządzenia wejściowego
├── PlayPauseAction.cs              # SendKeys – media play/pause
├── NextTrackAction.cs              # SendKeys – następny utwór
├── PreviousTrackAction.cs          # SendKeys – poprzedni utwór
├── CopyAction.cs                   # SendKeys – Ctrl+C
├── PasteAction.cs                  # SendKeys – Ctrl+V
├── ScreenshotAction.cs             # SendKeys – PrintScreen
├── TaskManagerAction.cs            # Process.Start – taskmgr
├── CloseWindowAction.cs            # SendKeys – Alt+F4
├── FullscreenAction.cs             # SendKeys – F11
├── AltTabAction.cs                 # SendKeys – Alt+Tab
├── CustomShortcutAction.cs         # Win32 SendInput – dowolny skrót
├── OpenAppAction.cs                # Process.Start – dowolna aplikacja
└── KOYA-APP.csproj                 # Definicja projektu .NET 8 WPF
```

### Interfejs `IStreamDeckAction`

Każda akcja implementuje `IStreamDeckAction`:

```csharp
public interface IStreamDeckAction
{
    string Name { get; }
    void Execute();
}
```

Dodanie nowej akcji wymaga stworzenia klasy implementującej ten interfejs i zarejestrowania jej w liście w `ActionPicker.xaml.cs`.

---

## Zależności NuGet

```xml
<PackageReference Include="NAudio" Version="2.3.0" />
<PackageReference Include="System.Windows.Extensions" Version="6.0.0" />
```

- **NAudio** — obsługa urządzeń audio Windows (CoreAudio API): zmiana głośności, wyciszanie, enumeracja mikrofonów.
- **System.Windows.Extensions** — rozszerzenia WPF / Windows Forms używane przez niektóre akcje IO.

---

## Rozszerzanie

Aby dodać własną akcję:

1. Utwórz plik `MojaAkcja.cs` w katalogu projektu.
2. Zaimplementuj `IStreamDeckAction`:

```csharp
using KOYA_APP;

public class MojaAkcja : IStreamDeckAction
{
    public string Name => "Moja Akcja";

    public void Execute()
    {
        // logika akcji
    }
}
```

3. Zarejestruj w `ActionPicker.xaml.cs` w konstruktorze, wewnątrz `new List<IStreamDeckAction> { ... }`:

```csharp
new MojaAkcja(),
```

4. Opcjonalnie dodaj panel konfiguracji w `ActionsListBox_SelectionChanged` i `Select_Click`, wzorując się na `SelectMicAction` lub `CustomShortcutAction`.

---

## Licencja

Brak zdefiniowanej licencji. Wszelkie prawa zastrzeżone przez autora repozytorium.
