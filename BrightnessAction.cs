using System;
using System.Management;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace KOYA_APP
{
    public class BrightnessAction : IStreamDeckAction
    {
        public string Name { get; set; } = "Jasność";
        
        [JsonIgnore]
        public string Icon => "\uE706";
        public string Description { get; set; } = "Reguluj jasność ekranu";

        public string? MonitorId { get; set; }
        public string? MonitorName { get; set; }

        private int _currentBrightness = -1;
        private bool _isProcessing = false;

        public void Execute()
        {
        }

        public async void ExecuteAnalog(bool direction)
        {
            if (_isProcessing) return; // Zapobiegaj nakładaniu się ciężkich operacji WMI/DDC
            
            _isProcessing = true;
            try
            {
                await System.Threading.Tasks.Task.Run(() =>
                {
                    int step = 5;

                    // 1. Próba DDC/CI (Zewnętrzne monitory Desktopowe)
                    if (TrySetDdcBrightness(direction, step))
                    {
                        return;
                    }

                    // 2. Fallback na WMI (Laptopy)
                    if (_currentBrightness == -1)
                    {
                        _currentBrightness = GetWmiBrightness();
                    }

                    _currentBrightness = direction ? 
                        Math.Min(100, _currentBrightness + step) : 
                        Math.Max(0, _currentBrightness - step);

                    SetWmiBrightness(_currentBrightness);
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Brightness Error: {ex.Message}");
            }
            finally
            {
                _isProcessing = false;
            }
        }

        private bool TrySetDdcBrightness(bool direction, int step)
        {
            try
            {
                IntPtr hwnd = GetDesktopWindow();
                IntPtr hMonitor = MonitorFromWindow(hwnd, MONITOR_DEFAULTTOPRIMARY);
                if (hMonitor == IntPtr.Zero) return false;

                if (GetNumberOfPhysicalMonitorsFromHMONITOR(hMonitor, out uint numPhysicalMonitors))
                {
                    var physicalMonitors = new PHYSICAL_MONITOR[numPhysicalMonitors];
                    if (GetPhysicalMonitorsFromHMONITOR(hMonitor, numPhysicalMonitors, physicalMonitors))
                    {
                        bool success = false;
                        IntPtr hPhysicalMonitor = physicalMonitors[0].hPhysicalMonitor;
                        
                        if (GetMonitorBrightness(hPhysicalMonitor, out uint min, out uint current, out uint max))
                        {
                            int nextBrightness = direction ? (int)current + step : (int)current - step;
                            nextBrightness = Math.Max((int)min, Math.Min((int)max, nextBrightness));
                            
                            success = SetMonitorBrightness(hPhysicalMonitor, (uint)nextBrightness);
                        }
                        
                        DestroyPhysicalMonitors(numPhysicalMonitors, physicalMonitors);
                        return success;
                    }
                }
            }
            catch
            {
                // Ignorujemy błędy i pozwalamy zadziałać fallbackowi WMI
            }
            return false;
        }

        private int GetWmiBrightness()
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    new ManagementScope(@"root\wmi"),
                    new SelectQuery("WmiMonitorBrightness"));

                using var collection = searcher.Get();
                foreach (ManagementObject obj in collection)
                {
                    if (!string.IsNullOrEmpty(MonitorId))
                    {
                        var instanceName = obj["InstanceName"]?.ToString();
                        if (instanceName != null && instanceName.Contains(MonitorId, StringComparison.OrdinalIgnoreCase))
                        {
                            return int.Parse(obj["CurrentBrightness"].ToString()!);
                        }
                    }
                    else
                    {
                        return int.Parse(obj["CurrentBrightness"].ToString()!);
                    }
                }
            }
            catch { }
            return 50;
        }

        private void SetWmiBrightness(int brightness)
        {
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    new ManagementScope(@"root\wmi"),
                    new SelectQuery("WmiMonitorBrightnessMethods"));

                using var collection = searcher.Get();
                foreach (ManagementObject obj in collection)
                {
                    if (!string.IsNullOrEmpty(MonitorId))
                    {
                        var instanceName = obj["InstanceName"]?.ToString();
                        if (instanceName != null && instanceName.Contains(MonitorId, StringComparison.OrdinalIgnoreCase))
                        {
                            obj.InvokeMethod("WmiSetBrightness", new object[] { uint.MaxValue, (byte)brightness });
                            return;
                        }
                    }
                    else
                    {
                        obj.InvokeMethod("WmiSetBrightness", new object[] { uint.MaxValue, (byte)brightness });
                        return;
                    }
                }
            }
            catch { }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct PHYSICAL_MONITOR
        {
            public IntPtr hPhysicalMonitor;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szPhysicalMonitorDescription;
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);
        const uint MONITOR_DEFAULTTOPRIMARY = 1;

        [DllImport("dxva2.dll", SetLastError = true)]
        static extern bool GetNumberOfPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, out uint pdwNumberOfPhysicalMonitors);

        [DllImport("dxva2.dll", SetLastError = true)]
        static extern bool GetPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, uint dwPhysicalMonitorArraySize, [Out] PHYSICAL_MONITOR[] pPhysicalMonitorArray);

        [DllImport("dxva2.dll", SetLastError = true)]
        static extern bool GetMonitorBrightness(IntPtr hMonitor, out uint pdwMinimumBrightness, out uint pdwCurrentBrightness, out uint pdwMaximumBrightness);

        [DllImport("dxva2.dll", SetLastError = true)]
        static extern bool SetMonitorBrightness(IntPtr hMonitor, uint dwNewBrightness);

        [DllImport("dxva2.dll", SetLastError = true)]
        static extern bool DestroyPhysicalMonitors(uint dwPhysicalMonitorArraySize, [In] PHYSICAL_MONITOR[] pPhysicalMonitorArray);
    }
}
