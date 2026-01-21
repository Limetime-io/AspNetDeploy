using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcSatelliteLinux;

namespace SatelliteLinuxGrpcHost.Services
{
    public class InformationController : Information.InformationBase
    {
        private readonly IInformationService informationService;

        public InformationController(IInformationService informationService)
        {
            this.informationService = informationService;
        }

        public override Task<InformationVersionResponse> GetVersion(Empty request, ServerCallContext context)
        {
            return Task.FromResult(new InformationVersionResponse
            {
                Version = this.informationService.GetVersion()
            });
        }
    }
}
