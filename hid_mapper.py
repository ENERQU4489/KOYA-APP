import hid
import time

# Arduino Pro Micro HID details
VENDOR_ID = 0x2341
PRODUCT_ID = 0x8037

def map_buttons():
    try:
        device = hid.device()
        device.open(VENDOR_ID, PRODUCT_ID)
        device.set_nonblocking(True)
        print("\n=== KOYA HUB: HARDWARE MAPPER ===")
        print("Połączono ze Stream Deckiem. Rozpoczynamy mapowanie 12 przycisków.")
        print("Skrypt zapyta Cię o kliknięcie każdego przycisku po kolei (od góry do dołu, od lewej do prawej).\n")
    except Exception as e:
        print(f"BŁĄD: Nie znaleziono urządzenia KOYA! Sprawdź czy jest podłączone. ({e})")
        return

    mapping = []
    
    # 3 rows, 4 columns
    for row in range(3):
        for col in range(4):
            btn_num = row * 4 + col + 1
            print(f"--- KROK {btn_num}/12 ---")
            print(f"KLIKNIJ FIZYCZNY PRZYCISK: [ Rząd {row+1}, Kolumna {col+1} ]")
            print("Czekam na sygnał...")
            
            captured = False
            while not captured:
                report = device.read(64)
                if report:
                    # Windows report ID handling (shift if first byte is 0)
                    offset = 1 if report[0] == 0 else 0
                    msg_type = report[offset]
                    btn_index = report[offset + 1]
                    val = report[offset + 2]
                    
                    if msg_type == 1 and val == 1: # Button Pressed
                        print(f"Odebrano sygnał! (Wewnętrzny Indeks: {btn_index})")
                        mapping.append(btn_index)
                        captured = True
                        # Small delay to avoid capturing the same press multiple times
                        time.sleep(0.5)
                time.sleep(0.01)
    
    device.close()
    print("\n=== MAPOWANIE ZAKOŃCZONE ===")
    print("Oto Twoja nowa kolejność indeksów:")
    print(mapping)
    print("\nPrześlij mi tę listę, a ja wstawię ją do kodu Arduino!")

if __name__ == "__main__":
    map_buttons()
