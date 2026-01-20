using AspNetDeploy.BuildServices.DotnetCore;
using AspNetDeploy.BuildServices.MSBuild;
using AspNetDeploy.Contracts;
using AspNetDeploy.Contracts.Exceptions;
using AspNetDeploy.Model;
using BuildServices.Gulp;

namespace AspNetDeploy.BuildServices
{
    public class BuildServiceFactory : IBuildServiceFactory
    {
        private readonly IPathServices pathServices;

        public BuildServiceFactory(IPathServices pathServices)
        {
            this.pathServices = pathServices;
        }

        public IBuildService Create(ProjectType projectType)
        {
            if (projectType == ProjectType.GulpFile)
            {
                return new GulpBuildService(this.pathServices);
            }

            if (projectType.HasFlag(ProjectType.NetCore))
            {
                return new DotnetCoreBuildService();
            }

            return new MSBuildBuildService(this.pathServices);
        }

        public IBuildService Create(ProjectBundleConfig config)
        {
            if (config is NetCoreProjectBundleConfig netCoreProjectBundle)
            {
                if (netCoreProjectBundle.OutputType == NetCoreOutputType.DockerContainer)
                {
                    return new DotnetCoreDockerBuildService();
                }

                return new DotnetCoreBuildService();
            }

            throw new AspNetDeployException("Project bundle config is not supported");
        }
    }
}
