import hid
import time

# Ustawienia urządzenia
VID = 0x2341
PID = 0x8037

def start_test():
    try:
        device = hid.device()
        device.open(VID, PID)
        device.set_nonblocking(True)
        print("\n=== KOYA: PROSTY TESTER MAPOWANIA ===")
        print("Nasłuchuję hardware... (Naciśnij Ctrl+C aby wyjść)\n")
        
        while True:
            report = device.read(64)
            if report:
                # Obsługa Report ID w Windows
                offset = 1 if report[0] == 0 else 0
                msg_type = report[offset]
                index = report[offset + 1]
                value = report[offset + 2]
                
                if msg_type == 1:
                    state = "WCIŚNIĘTY" if value == 1 else "PUSZCZONY"
                    print(f"[PRZYCISK] Indeks: {index:2} | Stan: {state}")
                elif msg_type == 2:
                    print(f"[GAŁKA]    Indeks: {index:2} | Pozycja: {value}/255")
            
            time.sleep(0.001)
            
    except KeyboardInterrupt:
        print("\nTest zatrzymany.")
        device.close()
    except Exception as e:
        print(f"BŁĄD: {e}")

if __name__ == "__main__":
    start_test()
