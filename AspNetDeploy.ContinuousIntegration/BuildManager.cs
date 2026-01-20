using System;
using System.IO;
using System.Linq;
using AspNetDeploy.Contracts;
using AspNetDeploy.Contracts.Exceptions;
using AspNetDeploy.Model;

namespace AspNetDeploy.ContinuousIntegration
{
    public class BuildManager
    {
        private readonly IBuildServiceFactory buildServiceFactory;
        private readonly IPathServices pathServices;
        private readonly ILoggingService loggingService;

        public BuildManager(IBuildServiceFactory buildServiceFactory, IPathServices pathServices, ILoggingService loggingService)
        {
            this.buildServiceFactory = buildServiceFactory;
            this.pathServices = pathServices;
            this.loggingService = loggingService;
        }

        public void Build(int sourceControlVersionId, int projectVersionId, Action<int> projectBuildStarted, Action<int, bool> projectBuildComplete)
        {
            AspNetDeployEntities entities = new AspNetDeployEntities();

            SourceControlVersion sourceControlVersion = entities.SourceControlVersion.Include("SourceControl").First(scv => scv.Id == sourceControlVersionId);
            ProjectVersion projectVersion = entities.ProjectVersion
                .Include("Properties")
                .Include("ProjectVersionToBundleVersion")
                .First(pv => pv.Id == projectVersionId);

            foreach (ProjectVersionToBundleVersion projectVersionLink in projectVersion.ProjectVersionToBundleVersion)
            {
                ProjectBundleConfig config = ProjectBundleConfigFactory.Create(projectVersionLink.ConfigurationJson);

                IBuildService buildService = config == null 
                    ? buildServiceFactory.Create(projectVersion.ProjectType)
                    : buildServiceFactory.Create(config);

                string sourcesFolder = this.pathServices.GetSourceControlVersionPath(sourceControlVersion.SourceControl.Id, sourceControlVersion.Id);
                
                BuildSolutionResult buildSolutionResult = buildService.Build(
                    sourcesFolder,
                    projectVersion,
                    projectFileName =>
                    {
                        ProjectVersion projectVersionBuild = entities.ProjectVersion
                            .Where(p => p.SourceControlVersionId == sourceControlVersionId)
                            .ToList()
                            .FirstOrDefault(p => !p.IsDeleted && Path.Combine(sourcesFolder, p.ProjectFile).ToLowerInvariant() == projectFileName.ToLowerInvariant());

                        if (projectVersionBuild != null)
                        {
                            projectBuildStarted(projectVersionBuild.Id);
                        }
                    },
                    (projectFileName, success, message) =>
                    {
                        ProjectVersion projectVersionBuild = entities.ProjectVersion
                            .Where(p => p.SourceControlVersionId == sourceControlVersionId)
                            .ToList()
                            .FirstOrDefault(p => !p.IsDeleted && Path.Combine(sourcesFolder, p.ProjectFile).ToLowerInvariant() == projectFileName.ToLowerInvariant());

                        if (projectVersionBuild != null)
                        {
                            projectBuildComplete(projectVersionBuild.Id, success);
                        }
                    },
                    (projectFile, exception) =>
                    {
                        this.loggingService.Log(new AspNetDeployException("Project build failed: " + projectFile, exception), null);
                    });
            }

        }
    }
}