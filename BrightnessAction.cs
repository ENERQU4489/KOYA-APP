using System;
using System.Management;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Threading;

namespace KOYA_APP
{
    public class BrightnessAction : IStreamDeckAction
    {
        public string Name { get; set; } = "Jasność";
        
        [JsonIgnore]
        public string Icon => "\uE706";
        public string Category => "System & Okna";
        public string Description { get; set; } = "Reguluj jasność ekranu";

        public string? MonitorId { get; set; }
        public string? MonitorName { get; set; }

        private int _targetBrightness = -1;
        private int _lastSetBrightness = -1;
        private bool _isWorkerRunning = false;
        private readonly object _lock = new object();

        public void Execute() { }

        public void ExecuteAnalog(bool direction)
        {
            lock (_lock)
            {
                if (_targetBrightness == -1) _targetBrightness = GetWmiBrightness();
                _targetBrightness = direction ? Math.Min(100, _targetBrightness + 5) : Math.Max(0, _targetBrightness - 5);
            }
            EnsureWorkerRunning();
        }

        public void ExecuteAbsolute(int value)
        {
            lock (_lock)
            {
                _targetBrightness = (int)(value * 100.0 / 255.0);
            }
            EnsureWorkerRunning();
        }

        private void EnsureWorkerRunning()
        {
            if (_isWorkerRunning) return;
            _isWorkerRunning = true;
            
            Task.Run(async () => {
                while (true)
                {
                    int currentTarget;
                    lock (_lock)
                    {
                        currentTarget = _targetBrightness;
                    }

                    if (currentTarget != _lastSetBrightness)
                    {
                        SetInternalBrightness(currentTarget);
                        _lastSetBrightness = currentTarget;
                    }

                    await Task.Delay(100); // Max 10 hardware updates per second

                    lock (_lock)
                    {
                        if (_targetBrightness == _lastSetBrightness)
                        {
                            _isWorkerRunning = false;
                            break;
                        }
                    }
                }
            });
        }

        private void SetInternalBrightness(int brightness)
        {
            try
            {
                IntPtr hMonitor = MonitorFromWindow(GetDesktopWindow(), MONITOR_DEFAULTTOPRIMARY);
                if (hMonitor != IntPtr.Zero && GetNumberOfPhysicalMonitorsFromHMONITOR(hMonitor, out uint num))
                {
                    var physicalMonitors = new PHYSICAL_MONITOR[num];
                    if (GetPhysicalMonitorsFromHMONITOR(hMonitor, num, physicalMonitors))
                    {
                        SetMonitorBrightness(physicalMonitors[0].hPhysicalMonitor, (uint)brightness);
                        DestroyPhysicalMonitors(num, physicalMonitors);
                        return;
                    }
                }
            } catch { }

            SetWmiBrightness(brightness);
        }

        private int GetWmiBrightness()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(new ManagementScope(@"root\wmi"), new SelectQuery("WmiMonitorBrightness"));
                using var collection = searcher.Get();
                foreach (ManagementObject obj in collection) return int.Parse(obj["CurrentBrightness"].ToString()!);
            } catch { }
            return 50;
        }

        private void SetWmiBrightness(int brightness)
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(new ManagementScope(@"root\wmi"), new SelectQuery("WmiMonitorBrightnessMethods"));
                using var collection = searcher.Get();
                foreach (ManagementObject obj in collection)
                {
                    obj.InvokeMethod("WmiSetBrightness", new object[] { uint.MaxValue, (byte)brightness });
                }
            } catch { }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct PHYSICAL_MONITOR { public IntPtr hPhysicalMonitor; [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string szPhysicalMonitorDescription; }

        [DllImport("user32.dll")] static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll")] static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);
        const uint MONITOR_DEFAULTTOPRIMARY = 1;
        [DllImport("dxva2.dll", SetLastError = true)] static extern bool GetNumberOfPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, out uint pdwNumberOfPhysicalMonitors);
        [DllImport("dxva2.dll", SetLastError = true)] static extern bool GetPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, uint dwPhysicalMonitorArraySize, [Out] PHYSICAL_MONITOR[] pPhysicalMonitorArray);
        [DllImport("dxva2.dll", SetLastError = true)] static extern bool SetMonitorBrightness(IntPtr hMonitor, uint dwNewBrightness);
        [DllImport("dxva2.dll", SetLastError = true)] static extern bool DestroyPhysicalMonitors(uint dwPhysicalMonitorArraySize, [In] PHYSICAL_MONITOR[] pPhysicalMonitorArray);
    }
}
