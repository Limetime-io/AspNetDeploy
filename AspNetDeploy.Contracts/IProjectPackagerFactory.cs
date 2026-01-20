using AspNetDeploy.Model;

namespace AspNetDeploy.Contracts
{
    public enum PackagerType
    {
        Undefined = 0,
        Directory = 1,
        Zip = 2,
        Gulp = 3,
        Database = 4,
        DotnetCore = 5,
        VisualStudio = 6,
        IisWebSite = 7,
        Docker = 8,
    }

    public interface IProjectPackagerFactory
    {
        IProjectPackager Create(ProjectType projectType);
        IProjectPackager Create(ProjectBundleConfig config);
    }
}