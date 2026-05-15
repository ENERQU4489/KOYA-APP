using HidSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace KOYA_APP
{
    public class HidBackend
    {
        private const int VendorId = 0x2341; // Arduino
        private const int ProductId = 0x8037; // Pro Micro / Leonardo (Wykryty: 0x8037)
        
        private HidDevice? _device;
        private HidStream? _stream;
        private bool _isRunning;
        private Thread? _readThread;

        public event Action<int>? ButtonPressed;
        public event Action<int, bool>? KnobTurned; // index, direction
        public event Action<bool>? ConnectionStatusChanged;

        public bool IsConnected => _stream != null;

        public void Start()
        {
            if (_isRunning) return;
            _isRunning = true;
            _readThread = new Thread(MonitorDevice) { IsBackground = true, Name = "HID_Monitor_Thread" };
            _readThread.Start();
            Console.WriteLine("[HID] Monitor startujący...");
        }

        public void Stop()
        {
            _isRunning = false;
            _stream?.Dispose();
            _stream = null;
            Console.WriteLine("[HID] Monitor zatrzymany.");
        }

        private void MonitorDevice()
        {
            int errorCount = 0;
            const int maxErrors = 3;

            while (_isRunning)
            {
                if (_stream == null)
                {
                    TryConnect();
                    if (_stream == null)
                    {
                        Thread.Sleep(2000);
                        continue;
                    }
                    errorCount = 0;
                }

                try
                {
                    byte[] buffer = new byte[64];
                    int count = _stream.Read(buffer, 0, buffer.Length);
                    
                    if (count > 0)
                    {
                        errorCount = 0; // Reset błędów po udanym odczycie
                        ParseInput(buffer);
                        continue; // Natychmiast próbuj czytać kolejny pakiet
                    }
                }
                catch (TimeoutException)
                {
                    // Timeout jest normalny, jeśli urządzenie nic nie wysyła
                    continue; 
                }
                catch (Exception ex)
                {
                    errorCount++;
                    Debug.WriteLine($"[HID] Próba {errorCount}/{maxErrors} nieudana: {ex.Message}");
                    
                    if (errorCount >= maxErrors)
                    {
                        Disconnect();
                    }
                    Thread.Sleep(100);
                    continue;
                }
                
                Thread.Sleep(1);
            }
        }

        private void TryConnect()
        {
            // Pobieramy wszystkie urządzenia HID o danym VID/PID
            var devices = DeviceList.Local.GetHidDevices(VendorId, ProductId).ToList();
            
            if (!devices.Any()) return;

            // Szukamy interfejsu RawHID. 
            // W Arduino Leonardo/Pro Micro (HID-Project) RawHID to zazwyczaj interface 2 (MI_02).
            // Możemy też sprawdzić MaxInputReportLength, który dla RawHID wynosi 64.
            var rawDevices = devices.Where(d => 
                d.DevicePath.Contains("mi_02") || 
                d.DevicePath.Contains("&col02") || 
                d.MaxInputReportLength == 64).ToList();

            // Jeśli nie znaleźliśmy po ścieżce, bierzemy pierwsze lepsze z raportem 64
            var targetDevice = rawDevices.FirstOrDefault() ?? devices.FirstOrDefault(d => d.MaxInputReportLength == 64);

            if (targetDevice != null)
            {
                try
                {
                    if (targetDevice.TryOpen(out _stream))
                    {
                        _device = targetDevice;
                        _stream.ReadTimeout = 1000; // Ustawiamy timeout, aby Read nie wisiał wiecznie
                        
                        ConnectionStatusChanged?.Invoke(true);
                        string name = targetDevice.GetProductName() ?? "Arduino KOYA";
                        Console.WriteLine($"[HID] POŁĄCZONO: {name}");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[HID] Błąd otwarcia: {ex.Message}");
                }
            }
        }

        private void Disconnect()
        {
            _stream?.Dispose();
            _stream = null;
            _device = null;
            ConnectionStatusChanged?.Invoke(false);
            Console.WriteLine("[HID] Rozłączono.");
        }

        private void ParseInput(byte[] data)
        {
            // Windows Report ID handling: jeśli pierwszy bajt to 0, dane zaczynają się od drugiego.
            int offset = (data[0] == 0) ? 1 : 0;

            byte type = data[offset];     // 1: Przycisk, 2: Enkoder
            byte index = data[offset + 1]; // Indeks (0-13)
            byte val = data[offset + 2];   // Wartość/Kierunek

            if (type == 1) // Przycisk
            {
                ButtonPressed?.Invoke(index);
            }
            else if (type == 2) // Enkoder
            {
                bool isRight = (val == 1);
                KnobTurned?.Invoke(index, isRight);
            }
        }
    }
}
