namespace AspNetDeploy.WebUI.Models.DeploymentSteps
{
    public class SourceFilesDeploymentStepModel : ProjectRelatedDeploymentStepModel
    {
        public string StepTitle { get; set; }

        public string Destination { get; set; }

        public string CustomConfigurationJson { get; set; }
    }
}
