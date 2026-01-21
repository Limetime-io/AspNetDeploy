namespace SatelliteLinuxGrpcHost.Services
{
    public class InformationService : IInformationService
    {
        private const int ServiceVersion = 20240417;

        public int GetVersion()
        {
            return ServiceVersion;
        }
    }
}
