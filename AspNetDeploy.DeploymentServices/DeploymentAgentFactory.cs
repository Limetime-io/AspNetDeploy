using System;
using AspNetDeploy.Contracts;
using AspNetDeploy.Contracts.Exceptions;
using AspNetDeploy.DeploymentServices.WCFSatellite;
using AspNetDeploy.Model;
using DeploymentServices.Grpc;

namespace AspNetDeploy.DeploymentServices
{
    public class DeploymentAgentFactory : IDeploymentAgentFactory
    {
        private readonly IVariableProcessorFactory variableProcessorFactory;
        private readonly IPathServices pathServices;

        public DeploymentAgentFactory(IVariableProcessorFactory variableProcessorFactory, IPathServices pathServices)
        {
            this.variableProcessorFactory = variableProcessorFactory;
            this.pathServices = pathServices;
        }

        public IDeploymentAgent Create(Machine machine, Package package)
        {
            IVariableProcessor variableProcessor = this.variableProcessorFactory.Create(package.Id, machine.Id);

            // Select agent based on machine platform
            NetCorePlatform platform = (NetCorePlatform)machine.PlatformId;

            switch (platform)
            {
                case NetCorePlatform.Windows:
                    return new WCFSatelliteDeploymentAgent(variableProcessor, machine.URL, machine.Login, machine.Password);

                case NetCorePlatform.Linux:
                    return new GrpcDeploymentAgent(variableProcessor, this.pathServices, machine.URL, machine.Login, machine.Password);

                case NetCorePlatform.MacOS:
                    return new GrpcDeploymentAgent(variableProcessor, this.pathServices, machine.URL, machine.Login, machine.Password);

                case NetCorePlatform.Undefined:
                default:
                    throw new AspNetDeployException($"Unsupported platform for machine '{machine.Name}': {platform}");
            }
        }
    }
}