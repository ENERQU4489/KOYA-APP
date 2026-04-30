KOYA-APP

KOYA-APP to zaawansowane narzędzie desktopowe oparte na systemie Windows (WPF), umożliwiające szybkie zarządzanie systemem, multimediami i aplikacjami za pomocą konfigurowalnych akcji. Projekt został zaprojektowany z myślą o automatyzacji pracy i szybkim dostępie do kluczowych funkcji komputera.  
🚀 Główne Funkcje

Aplikacja wspiera szeroki wachlarz akcji, które można przypisać do przycisków lub skrótów:

    Zarządzanie Multimediami: Odtwarzaj/Pauzuj, następny/poprzedni utwór, regulacja głośności oraz wyciszanie głośników.  

    Kontrola Mikrofonu: Szybkie wyciszanie (Mute) oraz wybór aktywnego mikrofonu.  

    Narzędzia Systemowe: Wykonywanie zrzutów ekranu, otwieranie Menedżera Zadań, przełączanie okien (Alt-Tab) oraz zamykanie aktywnych okien.  

    Praca z Tekstem: Szybkie kopiowanie i wklejanie.  

    Automatyzacja Aplikacji: Otwieranie wybranych programów, kontrola powiększenia w przeglądarkach (Web Zoom) oraz obsługa pełnego ekranu.  

    Własne Skróty: Możliwość definiowania niestandardowych kombinacji klawiszy (Custom Shortcuts).  

🛠 Technologie

    Język: C#  

    Framework: WPF (Windows Presentation Foundation) / .NET  

    UI: XAML (z wykorzystaniem czcionki Audiowide dla nowoczesnego wyglądu).  

    Architektura: Modularny system akcji oparty na interfejsie IStreamDeckAction.  

📂 Struktura Projektu

    ActionPicker.xaml: Interfejs wyboru dostępnych akcji.  

    ConfigurationManager.cs: Odpowiada za zapisywanie i odczytywanie ustawień użytkownika.  

    TutorialManager.cs: System onboardingu dla nowych użytkowników.  

    Assets/: Zawiera zasoby graficzne (logo) oraz typografię.  

📦 Instalacja i Uruchomienie

    Wymagania: Zainstalowane środowisko .NET SDK oraz Visual Studio 2022 (lub nowsze).

    Klonowanie:
    Bash

    git clone https://github.com/ENERQU4489/KOYA-APP.git

    Kompilacja: Otwórz plik KOYA-APP.csproj w Visual Studio i wybierz opcję Build Solution.  

    Uruchomienie: F5 lub uruchom wygenerowany plik .exe z folderu bin/Debug.

⚠️ Logowanie błędów

Aplikacja posiada wbudowany system logowania awarii, który zapisuje szczegóły w plikach:

    crash_log.txt

    crash_init_log.txt

      

📝 Licencja

Projekt rozwijany przez ENERQU4489. Szczegóły licencji znajdują się w pliku LICENSE (jeśli został dołączony).
