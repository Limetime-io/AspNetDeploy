using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcSatelliteLinux;

namespace SatelliteLinuxGrpcHost.Services
{
    public class MonitoringController : Monitoring.MonitoringBase
    {
        private readonly IMonitoringService monitoringService;

        public MonitoringController(IMonitoringService monitoringService)
        {
            this.monitoringService = monitoringService;
        }

        public override Task<ServerSummaryResponse> GetServerSummary(Empty request, ServerCallContext context)
        {
            return Task.FromResult(this.monitoringService.GetServerSummary());
        }
    }
}
