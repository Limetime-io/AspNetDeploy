namespace AspNetDeploy.WebUI.Models.DeploymentSteps
{
    public class ContainerDeploymentStepModel : DeploymentStepModel
    {
        public string StepTitle { get; set; }
        public string ContainerName { get; set; }
        public string Ports { get; set; }
        public string EnvironmentVariables { get; set; }
        public string Labels { get; set; }
        public string Volumes { get; set; }
        public string RestartPolicy { get; set; }
        public string Networks { get; set; }
    }
}
