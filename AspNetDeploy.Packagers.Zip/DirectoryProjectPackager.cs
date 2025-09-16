using System.IO;
using System.Text;
using AspNetDeploy.Contracts;
using AspNetDeploy.Contracts.Exceptions;
using Ionic.Zip;
using Ionic.Zlib;

namespace AspNetDeploy.Packagers.Zip
{
    public class DirectoryProjectPackager : IProjectPackager
    {
        public void Package(string projectPath, string packageFile)
        {
            if (!Directory.Exists(projectPath))
            {
                throw new AspNetDeployException("Directory does not exist: " + projectPath);
            }

            using (ZipFile zipFile = new ZipFile(Encoding.UTF8))
            {
                zipFile.AlternateEncoding = Encoding.UTF8;
                zipFile.AlternateEncodingUsage = ZipOption.Always;
                zipFile.CompressionLevel = CompressionLevel.BestCompression;

                zipFile.AddDirectory(projectPath, string.Empty);

                zipFile.Save(packageFile);
            }
        }
    }
}
