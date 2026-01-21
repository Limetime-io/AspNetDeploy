using AspNetDeploy.Model;

namespace AspNetDeploy.WebUI.Models.DeploymentSteps
{
    public class ContainerDeploymentStepModel : ProjectRelatedDeploymentStepModel
    {
        public string StepTitle { get; set; }
        public string ContainerName { get; set; }
        public string Ports { get; set; }
        public string EnvironmentVariables { get; set; }
        public string Labels { get; set; }
        public string Volumes { get; set; }
        public string RestartPolicy { get; set; }
        public string Networks { get; set; }
        public NetCorePlatform Platform { get; set; }
        public NetCoreArchitecture Architecture { get; set; }
    }
}
