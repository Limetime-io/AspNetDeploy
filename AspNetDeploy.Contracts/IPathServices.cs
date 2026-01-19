namespace AspNetDeploy.Contracts
{
    public interface IPathServices
    {
        string GetSourceControlVersionPath(int sourceControlId, int sourceControlVersionId);
        string GetBundlePackagePath(int bundleId, int packageId);
        string GetProjectPackagePath(int projectPackageId, string revisionId, int packagerId);
        string GetNugetPath();
        string GetNpmPath();
        string GetMSBuildPath();
        string GetClientCertificatePath();
        string GetRootCertificatePath(bool isPfx = true);
        string GetMachineCertificatePath(bool isRoot = false);

    }
}