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
            NetCoreProjectBundleConfig = new NetCoreProjectBundleConfig();
        }

        public DotNetDockerProjectPackager(NetCoreProjectBundleConfig netCoreProjectBundleConfig)
        {
            NetCoreProjectBundleConfig = netCoreProjectBundleConfig;
        }


        protected override void PackageProjectContents(ZipFile zipFile, XDocument xDocument, XNamespace vsNamespace, string projectRootFolder)
        {
            XElement targetFramework = xDocument.Descendants("TargetFramework").FirstOrDefault();

            if (targetFramework == null)
            {
                throw new VisualStudioPackagerException("targetFramework not set");
            }

            if (!this.IsFrameworkSupported(targetFramework.Value))
            {
                throw new VisualStudioPackagerException("targetFramework not supported: " + targetFramework.Value);
            }

            // TODO: CREATE DOCKER IMAGE

            AddProjectDirectory(
                zipFile,
                projectRootFolder,
                Path.Combine("bin", "Release", targetFramework.Value, "publish"),
                "\\");
        }

        private bool IsFrameworkSupported(string targetFramework)
        {
            if (targetFramework.Equals("net8.0", StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            if (targetFramework.Equals("net9.0", StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            if (targetFramework.Equals("net10.0", StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            return false;
        }
    }
}
