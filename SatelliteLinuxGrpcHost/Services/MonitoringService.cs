using System;
using System.Globalization;
using System.IO;
using System.Linq;
using GrpcSatelliteLinux;

namespace SatelliteLinuxGrpcHost.Services
{
    public class MonitoringService : IMonitoringService
    {
        public ServerSummaryResponse GetServerSummary()
        {
            var response = new ServerSummaryResponse();

            FillMemoryInfo(response);
            FillDrivesInfo(response);

            return response;
        }

        private static void FillMemoryInfo(ServerSummaryResponse response)
        {
            (double totalMb, double availableMb) = GetMemoryInfoMb();

            response.TotalMemoryMb = totalMb;
            response.AvailableMemoryMb = availableMb;
        }

        private static void FillDrivesInfo(ServerSummaryResponse response)
        {
            var drives = System.IO.DriveInfo.GetDrives()
                .Where(drive => drive.IsReady && (drive.DriveType == DriveType.Fixed || drive.DriveType == DriveType.Ram));

            foreach (var drive in drives)
            {
                response.Drives.Add(new DriveInfoResponse
                {
                    Name = drive.Name,
                    Label = drive.VolumeLabel,
                    FreeSpaceMb = Math.Round(drive.AvailableFreeSpace / 1024d / 1024d),
                    TotalSpaceMb = Math.Round(drive.TotalSize / 1024d / 1024d)
                });
            }
        }

        private static (double totalMb, double availableMb) GetMemoryInfoMb()
        {
            if (OperatingSystem.IsLinux() && File.Exists("/proc/meminfo"))
            {
                double? totalKb = null;
                double? availableKb = null;

                foreach (string line in File.ReadLines("/proc/meminfo"))
                {
                    if (line.StartsWith("MemTotal:", StringComparison.OrdinalIgnoreCase))
                    {
                        totalKb = ParseMemInfoValue(line);
                    }
                    else if (line.StartsWith("MemAvailable:", StringComparison.OrdinalIgnoreCase))
                    {
                        availableKb = ParseMemInfoValue(line);
                    }

                    if (totalKb.HasValue && availableKb.HasValue)
                    {
                        break;
                    }
                }

                if (totalKb.HasValue)
                {
                    double totalMb = totalKb.Value / 1024d;
                    double availableMb = (availableKb ?? totalKb.Value) / 1024d;
                    return (totalMb, availableMb);
                }
            }

            var memoryInfo = GC.GetGCMemoryInfo();
            double totalAvailableBytes = memoryInfo.TotalAvailableMemoryBytes;
            double memoryLoadBytes = memoryInfo.MemoryLoadBytes;

            double totalMbFallback = totalAvailableBytes > 0
                ? totalAvailableBytes / 1024d / 1024d
                : 0d;
            double availableMbFallback = totalAvailableBytes > 0
                ? Math.Max(0d, (totalAvailableBytes - memoryLoadBytes) / 1024d / 1024d)
                : 0d;

            return (totalMbFallback, availableMbFallback);
        }

        private static double? ParseMemInfoValue(string line)
        {
            string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2)
            {
                return null;
            }

            if (double.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
            {
                return value;
            }

            return null;
        }
    }
}
