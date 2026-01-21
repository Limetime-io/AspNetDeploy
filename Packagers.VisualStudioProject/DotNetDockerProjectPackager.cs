using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using AspNetDeploy.Model;
using Ionic.Zip;

namespace Packagers.VisualStudioProject
{
    public class DotNetDockerProjectPackager : VisualStudioProjectPackager
    {
        public NetCoreProjectBundleConfig NetCoreProjectBundleConfig { get; }

        public DotNetDockerProjectPackager()
        {
            NetCoreProjectBundleConfig = new NetCoreProjectBundleConfig
            {
                Platform = NetCorePlatform.Linux,
                Architecture = NetCoreArchitecture.x64,
                OutputType = NetCoreOutputType.DockerContainer
            };
        }

        public DotNetDockerProjectPackager(NetCoreProjectBundleConfig netCoreProjectBundleConfig)
        {
            NetCoreProjectBundleConfig = netCoreProjectBundleConfig ?? throw new ArgumentNullException(nameof(netCoreProjectBundleConfig));
        }


        protected override void PackageProjectContents(ZipFile zipFile, XDocument xDocument, XNamespace vsNamespace, string projectRootFolder)
        {
            // Build path based on configuration: bin/{os}/{arch}/{output-type}/container.tar.gz
            string os = GetOsParameter(NetCoreProjectBundleConfig.Platform);
            string arch = GetArchParameter(NetCoreProjectBundleConfig.Architecture);
            string outputType = GetOutputTypeParameter(NetCoreProjectBundleConfig.OutputType);

            string containerPath = Path.Combine(
                projectRootFolder,
                "bin",
                os ?? "default",
                arch ?? "default",
                outputType ?? "default",
                "container.tar.gz"
            );

            if (!File.Exists(containerPath))
            {
                throw new VisualStudioPackagerException($"Docker container not found at: {containerPath}");
            }

            // Add the container.tar.gz file to the package
            zipFile.AddFile(containerPath, "\\");
        }

        private string GetOsParameter(NetCorePlatform platform)
        {
            switch (platform)
            {
                case NetCorePlatform.Windows:
                    return "windows";
                case NetCorePlatform.Linux:
                    return "linux";
                case NetCorePlatform.MacOS:
                    return "osx";
                case NetCorePlatform.Undefined:
                default:
                    return null;
            }
        }

        private string GetArchParameter(NetCoreArchitecture architecture)
        {
            switch (architecture)
            {
                case NetCoreArchitecture.x86:
                    return "x86";
                case NetCoreArchitecture.x64:
                    return "x64";
                case NetCoreArchitecture.arm:
                    return "arm";
                case NetCoreArchitecture.arm64:
                    return "arm64";
                case NetCoreArchitecture.Undefined:
                default:
                    return null;
            }
        }

        private string GetOutputTypeParameter(NetCoreOutputType outputType)
        {
            switch (outputType)
            {
                case NetCoreOutputType.Exe:
                    return "exe";
                case NetCoreOutputType.DockerContainer:
                    return "docker";
                case NetCoreOutputType.Undefined:
                default:
                    return null;
            }
        }
    }
}
