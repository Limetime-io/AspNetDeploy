using System.Collections.Generic;
using System.Linq;
using AspNetDeploy.Model;

namespace AspNetDeploy.WebUI.Models.DeploymentSteps
{
    public class DeploymentStepEditModelFactory
    {
        private readonly AspNetDeployEntities entities;

        public DeploymentStepEditModelFactory()
        {
            this.entities = entities;
        }

        public DeploymentStepModel Create(DeploymentStep deploymentStep)
        {
            DeploymentStepModel model;

            switch (deploymentStep.Type)
            {
                case DeploymentStepType.DeployWebSite:
                    model = new WebSiteDeploymentStepModel
                    {
                        OrderIndex = deploymentStep.OrderIndex,
                        DeploymentStepId = deploymentStep.Id,
                        BundleVersionId = deploymentStep.BundleVersionId,
                        SiteName = deploymentStep.GetStringProperty("IIS.SiteName"),
                        ProjectId = deploymentStep.GetIntProperty("ProjectId"),
                        Destination = deploymentStep.GetStringProperty("IIS.DestinationPath"),
                        Roles = string.Join(", ", deploymentStep.MachineRoles.Select(mr => mr.Name)),
                        BindingsJson = deploymentStep.GetStringProperty("IIS.Bindings")
                    };
                    break;
                case DeploymentStepType.CopyFiles:
                    model = new ZipArchiveDeploymentStepModel
                    {
                        OrderIndex = deploymentStep.OrderIndex,
                        DeploymentStepId = deploymentStep.Id,
                        BundleVersionId = deploymentStep.BundleVersionId,
                        StepTitle = deploymentStep.GetStringProperty("Step.Title"),
                        ProjectId = deploymentStep.GetIntProperty("ProjectId"),
                        Destination = deploymentStep.GetStringProperty("DestinationPath"),
                        Roles = string.Join(", ", deploymentStep.MachineRoles.Select(mr => mr.Name)),
                        CustomConfigurationJson = deploymentStep.GetStringProperty("CustomConfiguration")
                    };
                    break;
                case DeploymentStepType.DeploySourceFiles:
                    model = new SourceFilesDeploymentStepModel
                    {
                        OrderIndex = deploymentStep.OrderIndex,
                        DeploymentStepId = deploymentStep.Id,
                        BundleVersionId = deploymentStep.BundleVersionId,
                        StepTitle = deploymentStep.GetStringProperty("Step.Title"),
                        ProjectId = deploymentStep.GetIntProperty("ProjectId"),
                        Destination = deploymentStep.GetStringProperty("DestinationPath"),
                        Roles = string.Join(", ", deploymentStep.MachineRoles.Select(mr => mr.Name)),
                        CustomConfigurationJson = deploymentStep.GetStringProperty("CustomConfiguration")
                    };
                    break;
                case DeploymentStepType.Configuration:
                    model = new ConfigDeploymentStepModel
                    {
                        OrderIndex = deploymentStep.OrderIndex,
                        BundleVersionId = deploymentStep.BundleVersionId,
                        DeploymentStepId = deploymentStep.Id,
                        ConfigJson = deploymentStep.GetStringProperty("SetValues"),
                        StepTitle = deploymentStep.GetStringProperty("Step.Title"),
                        File = deploymentStep.GetStringProperty("File"),
                        Roles = string.Join(", ", deploymentStep.MachineRoles.Select(mr => mr.Name))
                    };
                    break;
                case DeploymentStepType.RunSQLScript:
                    model = new SqlScriptDeploymentStepModel
                    {
                        OrderIndex = deploymentStep.OrderIndex,
                        BundleVersionId = deploymentStep.BundleVersionId,
                        DeploymentStepId = deploymentStep.Id,
                        Roles = string.Join(", ", deploymentStep.MachineRoles.Select(mr => mr.Name)),
                        ConnectionString = deploymentStep.GetStringProperty("ConnectionString"),
                        StepTitle = deploymentStep.GetStringProperty("Step.Title"),
                        Command = deploymentStep.GetStringProperty("Command")
                    };
                    break;
                case DeploymentStepType.UpdateHostsFile:
                    model = new HostsDeploymentStepModel
                    {
                        OrderIndex = deploymentStep.OrderIndex,
                        BundleVersionId = deploymentStep.BundleVersionId,
                        DeploymentStepId = deploymentStep.Id,
                        Roles = string.Join(", ", deploymentStep.MachineRoles.Select(mr => mr.Name)),
                        StepTitle = deploymentStep.GetStringProperty("Step.Title"),
                        ConfigJson = deploymentStep.GetStringProperty("ConfigurationJson")
                    };
                    break;
                case DeploymentStepType.DeployDacpac:
                    model = new DacpacDeploymentStepModel
                    {
                        OrderIndex = deploymentStep.OrderIndex,
                        BundleVersionId = deploymentStep.BundleVersionId,
                        DeploymentStepId = deploymentStep.Id,
                        Roles = string.Join(", ", deploymentStep.MachineRoles.Select(mr => mr.Name)),
                        StepTitle = deploymentStep.GetStringProperty("Step.Title"),
                        ProjectId = deploymentStep.GetIntProperty("ProjectId"),
                        ConnectionString = deploymentStep.GetStringProperty("ConnectionString"),
                        TargetDatabase = deploymentStep.GetStringProperty("TargetDatabase"),
                        CustomConfiguration = deploymentStep.GetStringProperty("CustomConfiguration")
                    };
                    break;
                case DeploymentStepType.RunVsTests:
                    model = new RunVsTestStepModel
                    {
                        OrderIndex = deploymentStep.OrderIndex,
                        BundleVersionId = deploymentStep.BundleVersionId,
                        DeploymentStepId = deploymentStep.Id,
                        Roles = string.Join(", ", deploymentStep.MachineRoles.Select(mr => mr.Name)),
                        StepTitle = deploymentStep.GetStringProperty("Step.Title"),
                        ProjectId = deploymentStep.GetIntProperty("ProjectId"),
                        FiltersJson = deploymentStep.GetStringProperty("FiltersJson"),
                        StopOnFailure = deploymentStep.GetBoolProperty("StopOnFailure")
                    };
                    break;
                case DeploymentStepType.DeployContainer:
                    model = new ContainerDeploymentStepModel
                    {
                        OrderIndex = deploymentStep.OrderIndex,
                        BundleVersionId = deploymentStep.BundleVersionId,
                        DeploymentStepId = deploymentStep.Id,
                        Roles = string.Join(", ", deploymentStep.MachineRoles.Select(mr => mr.Name)),
                        StepTitle = deploymentStep.GetStringProperty("Step.Title"),
                        ContainerName = deploymentStep.GetStringProperty("Container.Name"),
                        Ports = deploymentStep.GetStringProperty("Container.Ports"),
                        EnvironmentVariables = deploymentStep.GetStringProperty("Container.EnvironmentVariables"),
                        Labels = deploymentStep.GetStringProperty("Container.Labels"),
                        Volumes = deploymentStep.GetStringProperty("Container.Volumes"),
                        RestartPolicy = deploymentStep.GetStringProperty("Container.RestartPolicy"),
                        Networks = deploymentStep.GetStringProperty("Container.Networks")
                    };
                    break;
                default:
                    model = new DeploymentStepModel
                    {
                        OrderIndex = deploymentStep.OrderIndex,
                        DeploymentStepId = deploymentStep.Id,
                        BundleVersionId = deploymentStep.BundleVersionId,
                        Roles = string.Join(", ", deploymentStep.MachineRoles.Select(mr => mr.Name))
                    };
                    break;
            }

            return model;
        }
    }
}
