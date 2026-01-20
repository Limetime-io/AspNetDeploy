using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using AspNetDeploy.Contracts;
using AspNetDeploy.Model;
using Ionic.Zip;

namespace AspNetDeploy.ContinuousIntegration
{
    public class PackageManager
    {
        private readonly IPathServices pathServices;
        private readonly IProjectPackagerFactory projectPackagerFactory;

        public PackageManager(IPathServices pathServices, IProjectPackagerFactory projectPackagerFactory)
        {
            this.pathServices = pathServices;
            this.projectPackagerFactory = projectPackagerFactory;
        }

        public void PackageBundle(int bundleVersionId)
        {
            AspNetDeployEntities entities = new AspNetDeployEntities();

            BundleVersion bundleVersion = entities.BundleVersion
                .Include("Bundle")
                .Include("Packages")
                .Include("ProjectVersionTobundleVersion.ProjectVersion.Project")
                .Include("ProjectVersionTobundleVersion.ProjectVersion.SourceControlVersion.SourceControl")
                .First(bv => bv.Id == bundleVersionId);

            Package package = new Package
            {
                BundleVersion = bundleVersion,
                CreatedDate = DateTime.UtcNow
            };

            entities.Package.Add(package);

            IList<string> artifacts = new List<string>();

            using (ZipFile zipFile = new ZipFile(Encoding.UTF8))
            {
                zipFile.AlternateEncoding = Encoding.UTF8;
                zipFile.AlternateEncodingUsage = ZipOption.Always;

                foreach (ProjectVersionToBundleVersion projectVersionLink in bundleVersion.ProjectVersionToBundleVersion)
                {
                    ProjectVersion projectVersion = projectVersionLink.ProjectVersion;

                    ProjectBundleConfig config = ProjectBundleConfigFactory.Create(projectVersionLink.ConfigurationJson);

                    IProjectPackager projectPackager = config == null
                        ? projectPackagerFactory.Create(projectVersion.ProjectType)
                        : projectPackagerFactory.Create(config);

                    if (projectPackager == null) // no need to package
                    {
                        projectVersion.SetStringProperty("LastPackageDuration", "0");
                        entities.SaveChanges();
                        continue;
                    }

                    string sourcesFolder = this.pathServices.GetSourceControlVersionPath(projectVersion.SourceControlVersion.SourceControl.Id, projectVersion.SourceControlVersion.Id);
                    string projectPackagePath = this.pathServices.GetProjectPackagePath(projectVersionLink.ProjectVersionId, projectVersion.SourceControlVersion.GetStringProperty("Revision"), config);
                    string projectPath = Path.Combine(sourcesFolder, projectVersion.ProjectFile);

                    if (!File.Exists(projectPackagePath))
                    {
                        DateTime packageStartDate = DateTime.UtcNow;
                        projectPackager.Package(projectPath, projectPackagePath);
                        projectVersion.SetStringProperty("LastPackageDuration", (DateTime.UtcNow - packageStartDate).TotalSeconds.ToString(CultureInfo.InvariantCulture));
                        entities.SaveChanges();
                    }

                    zipFile.AddFile(projectPackagePath, "/");
                    artifacts.Add(projectPackagePath);

                    PackageEntry packageEntry = new PackageEntry
                    {
                        Package = package,
                        ProjectVersion = projectVersion,
                        Revision = projectVersion.SourceControlVersion.GetStringProperty("Revision")
                    };

                    entities.PackageEntry.Add(packageEntry);
                }

                zipFile.Save(this.pathServices.GetBundlePackagePath(bundleVersionId, package.Id));
            }

            package.PackageDate = DateTime.UtcNow;
            entities.SaveChanges();

            foreach (string artifact in artifacts)
            {
                if (File.Exists(artifact))
                {
                    File.Delete(artifact);
                }
            }
        }
    }
}