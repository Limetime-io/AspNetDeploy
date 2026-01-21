namespace SatelliteLinuxGrpcHost.Services
{
    public interface IDeploymentService
    {
        bool IsReady();
        bool BeginPublication(int publicationId);
        bool ExecuteNextOperation();
        bool Complete();
        void Rollback();
        void ResetPackage();
        void UploadPackageBuffer(byte[] buffer);
        void DeployContainer(dynamic request);
        void ProcessConfigFile(dynamic request);
        void RunPowerShellScript(dynamic request);
        void CopyFiles(dynamic request);
        void RunSQLScript(dynamic request);
        ExceptionInfo GetLastException();
    }

    public class ExceptionInfo
    {
        public string TypeName { get; set; } = string.Empty;
        public string AssemblyQualifiedTypeName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string StackTrace { get; set; } = string.Empty;
        public List<ExceptionDataInfo> ExceptionData { get; set; } = new();
    }

    public class ExceptionDataInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public bool IsProperty { get; set; }
    }
}
