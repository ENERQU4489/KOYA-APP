import hid
import time
import sys

# Arduino Pro Micro HID details
VENDOR_ID = 0x2341
PRODUCT_ID = 0x8037

def monitor_infinite():
    try:
        device = hid.device()
        device.open(VENDOR_ID, PRODUCT_ID)
        device.set_nonblocking(True)
        print("\n=== KOYA: MONITOR HARDWARE'U (TRYB CIĄGŁY) ===")
        print("Skrypt będzie wyświetlać każdy sygnał z Twojego Stream Decka.")
        print("Aby zakończyć, naciśnij Ctrl+C.\n")
        print("Czekam na sygnały...")
        
        while True:
            report = device.read(64)
            if report:
                # Windows report ID handling
                offset = 1 if report[0] == 0 else 0
                msg_type = report[offset]
                index = report[offset + 1]
                val = report[offset + 2]
                
                if msg_type == 1: # Przycisk
                    state = "WCIŚNIĘTY" if val == 1 else "PUSZCZONY"
                    print(f"[BUTTON] Indeks: {index} | Stan: {state}")
                elif msg_type == 2: # Potencjometr
                    print(f"[POTENTIOMETER] Indeks: {index} | Wartość: {val}")
            
            time.sleep(0.005) # Szybkie odświeżanie
            
    except KeyboardInterrupt:
        print("\nZatrzymano przez użytkownika.")
        device.close()
    except Exception as e:
        print(f"\nBŁĄD: {e}")
        print("Upewnij się, że Stream Deck jest podłączony i żadna inna aplikacja go nie blokuje.")

if __name__ == "__main__":
    monitor_infinite()
