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
        private const int VendorId = 0x2341; 
        private const int ProductId = 0x8037; 
        
        private HidDevice? _device;
        private HidStream? _stream;
        private bool _isRunning;
        private Thread? _readThread;

        public event Action<int>? ButtonPressed;
        public event Action<int, bool>? KnobTurned;
        public event Action<int, int>? KnobAbsoluteChanged;
        public event Action<bool>? ConnectionStatusChanged;

        public bool IsConnected => _stream != null;

        public void Start()
        {
            if (_isRunning) return;
            _isRunning = true;
            _readThread = new Thread(MonitorDevice) { IsBackground = true, Name = "HID_Monitor_Thread" };
            _readThread.Start();
        }

        public void Stop()
        {
            _isRunning = false;
            _stream?.Dispose();
            _stream = null;
        }

        private bool _firstConnectAttempt = true;

        private void MonitorDevice()
        {
            while (_isRunning)
            {
                if (_stream == null)
                {
                    TryConnect();
                    if (_stream == null) 
                    { 
                        if (_firstConnectAttempt)
                        {
                            _firstConnectAttempt = false;
                            ConnectionStatusChanged?.Invoke(false);
                        }
                        Thread.Sleep(2000); 
                        continue; 
                    }
                }

                try
                {
                    int reportSize = _device?.MaxInputReportLength ?? 64;
                    byte[] buffer = new byte[reportSize];
                    int count = _stream.Read(buffer, 0, buffer.Length);
                    if (count > 0) ParseInput(buffer);
                }
                catch { Disconnect(); Thread.Sleep(100); }
            }
        }

        private void TryConnect()
        {
            var devices = DeviceList.Local.GetHidDevices(VendorId, ProductId).ToList();
            
            // Próbujemy znaleźć interfejs RawHID (zazwyczaj 64 lub 65 bajtów)
            var targetDevice = devices.FirstOrDefault(d => d.MaxInputReportLength >= 63 && d.MaxInputReportLength <= 65);
            
            // Jeśli nie ma idealnego dopasowania, bierzemy pierwszy lepszy z tej listy
            if (targetDevice == null) targetDevice = devices.FirstOrDefault();

            if (targetDevice != null && targetDevice.TryOpen(out _stream))
            {
                _device = targetDevice;
                _stream.ReadTimeout = 1000;
                _firstConnectAttempt = false;
                ConnectionStatusChanged?.Invoke(true);
            }
        }

        private void Disconnect()
        {
            _stream?.Dispose();
            _stream = null;
            _device = null;
            ConnectionStatusChanged?.Invoke(false);
        }

        private void ParseInput(byte[] data)
        {
            int offset = (data[0] == 0) ? 1 : 0;
            byte type = data[offset];     
            byte index = data[offset + 1]; 
            byte val = data[offset + 2];   

            if (index < 0 || index >= 14) return;

            if (type == 1 && val == 1) // Button Pressed
            {
                ButtonPressed?.Invoke(index);
            }
            else if (type == 2) // Absolute Pot
            {
                KnobAbsoluteChanged?.Invoke(index, val);
            }
        }
    }
}
