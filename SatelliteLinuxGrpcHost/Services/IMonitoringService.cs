using GrpcSatelliteLinux;

namespace SatelliteLinuxGrpcHost.Services
{
    public interface IMonitoringService
    {
        ServerSummaryResponse GetServerSummary();
    }
}
