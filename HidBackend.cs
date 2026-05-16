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

        private void MonitorDevice()
        {
            while (_isRunning)
            {
                if (_stream == null)
                {
                    TryConnect();
                    if (_stream == null) { Thread.Sleep(2000); continue; }
                }

                try
                {
                    byte[] buffer = new byte[64];
                    int count = _stream.Read(buffer, 0, buffer.Length);
                    if (count > 0) ParseInput(buffer);
                }
                catch { Disconnect(); Thread.Sleep(100); }
                Thread.Sleep(1);
            }
        }

        private void TryConnect()
        {
            var devices = DeviceList.Local.GetHidDevices(VendorId, ProductId).ToList();
            var targetDevice = devices.FirstOrDefault(d => d.MaxInputReportLength == 64);

            if (targetDevice != null && targetDevice.TryOpen(out _stream))
            {
                _device = targetDevice;
                _stream.ReadTimeout = 1000;
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

        private bool[] _lastButtonStates = new bool[14];

        private void ParseInput(byte[] data)
        {
            int offset = (data[0] == 0) ? 1 : 0;
            byte type = data[offset];     
            byte index = data[offset + 1]; 
            byte val = data[offset + 2];   

            if (index < 0 || index >= 14) return;

            if (type == 1) // Button
            {
                bool isPressed = (val == 1);
                if (isPressed && !_lastButtonStates[index]) ButtonPressed?.Invoke(index);
                _lastButtonStates[index] = isPressed;
            }
            else if (type == 2) // Absolute Pot
            {
                KnobAbsoluteChanged?.Invoke(index, val);
            }
        }
    }
}
