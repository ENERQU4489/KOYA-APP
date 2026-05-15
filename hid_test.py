import hid
import time

# Arduino Pro Micro VID/PID
VENDOR_ID = 0x2341
PRODUCT_ID = 0x8037

def test_hid():
    print(f"Szukanie urządzenia VID: {hex(VENDOR_ID)}, PID: {hex(PRODUCT_ID)}...")
    
    # Listowanie urządzeń dla pewności
    for device in hid.enumerate():
        if device['vendor_id'] == VENDOR_ID and device['product_id'] == PRODUCT_ID:
            print(f"Znaleziono: {device['product_string']} (Path: {device['path']})")

    try:
        h = hid.device()
        h.open(VENDOR_ID, PRODUCT_ID)
        print("Urządzenie OTWARTE pomyślnie. Czekam na dane (zewrzyj piny na Arduino)...")
        h.set_nonblocking(True)

        while True:
            d = h.read(64)
            if d:
                print(f"Odebrano dane: {d}")
            time.sleep(0.01)
            
    except Exception as e:
        print(f"BŁĄD: {e}")
    finally:
        print("Zamykanie...")

if __name__ == "__main__":
    test_hid()
