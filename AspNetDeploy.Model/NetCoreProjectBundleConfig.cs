namespace AspNetDeploy.Model
{
    public enum NetCorePlatform
    {
        Undefined = 0,
        Windows = 1,
        Linux = 2,
        MacOS = 3
    }

    public enum NetCoreArchitecture
    {
        Undefined = 0,
        x86 = 1,
        x64 = 2,
        arm = 3,
        arm64 = 4
    }

    public enum NetCoreOutputType
    {
        Undefined = 0,
        Exe = 1,
        DockerContainer = 2
    }

    public class NetCoreProjectBundleConfig : ProjectBundleConfig
    {
        public NetCorePlatform Platform { get; set; }
        public NetCoreArchitecture Architecture { get; set; }
        public NetCoreOutputType OutputType { get; set; }
    }
}
