using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using AspNetDeploy.Contracts;
using AspNetDeploy.Model;

namespace AspNetDeploy.BuildServices.DotnetCore
{
    public class DotnetCoreDockerBuildService : IBuildService
    {
        public BuildSolutionResult Build(string sourcesFolder, ProjectVersion projectVersion, Action<string> projectBuildStarted, Action<string, bool, string> projectBuildComplete, Action<string, Exception> errorLogger)
        {
            var fullPath = Path.Combine(sourcesFolder, projectVersion.ProjectFile);
            projectBuildStarted(fullPath);

            // Get configuration from ProjectVersionToBundleVersion
            var projectVersionToBundleVersion = projectVersion.ProjectVersionToBundleVersion.FirstOrDefault();

            if (projectVersionToBundleVersion == null)
            {
                string errorMessage = "ProjectVersionToBundleVersion not found for project";
                errorLogger(fullPath, new DotnetCoreDockerBuildServiceException(errorMessage));
                projectBuildComplete(fullPath, false, errorMessage);
                return new BuildSolutionResult { IsSuccess = false };
            }

            NetCoreProjectBundleConfig config = null;

            if (!string.IsNullOrWhiteSpace(projectVersionToBundleVersion.ConfigurationJson))
            {
                var bundleConfig = ProjectBundleConfigFactory.Create(projectVersionToBundleVersion.ConfigurationJson);
                config = bundleConfig as NetCoreProjectBundleConfig;
            }

            if (config == null)
            {
                string errorMessage = "NetCoreProjectBundleConfig not found or invalid in ConfigurationJson";
                errorLogger(fullPath, new DotnetCoreDockerBuildServiceException(errorMessage));
                projectBuildComplete(fullPath, false, errorMessage);
                return new BuildSolutionResult { IsSuccess = false };
            }

            string output;
            string workingDirectory = Path.GetDirectoryName(fullPath);

            // Restore
            if (DoDotnet(workingDirectory, "restore", out output) != 0)
            {
                errorLogger(fullPath, new DotnetCoreDockerBuildServiceException(output));
                projectBuildComplete(fullPath, false, output);
                return new BuildSolutionResult { IsSuccess = false };
            }

            // Build publish command
            string publishCommand = BuildPublishCommand(config, fullPath, projectVersion);

            if (DoDotnet(workingDirectory, publishCommand, out output) != 0)
            {
                errorLogger(fullPath, new DotnetCoreDockerBuildServiceException(output));
                projectBuildComplete(fullPath, false, output);
                return new BuildSolutionResult { IsSuccess = false };
            }

            projectBuildComplete(fullPath, true, null);
            return new BuildSolutionResult { IsSuccess = true };
        }

        private string BuildPublishCommand(NetCoreProjectBundleConfig config, string projectFile, ProjectVersion projectVersion)
        {
            string os = GetOsParameter(config.Platform);
            string arch = GetArchParameter(config.Architecture);
            string outputType = GetOutputTypeParameter(config.OutputType);

            string command = $"publish \"{projectFile}\" -c Release";

            if (!string.IsNullOrEmpty(os))
            {
                command += $" --os {os}";
            }

            if (!string.IsNullOrEmpty(arch))
            {
                command += $" --arch {arch}";
            }

            command += " /t:PublishContainer";
            command += " -p:ContainerRepository=Project";
            command += $" -p:ContainerImageTag={projectVersion.Id}";

            // Build output path: bin/{os}/{arch}/{output-type}
            string projectDirectory = Path.GetDirectoryName(projectFile);
            string outputPath = Path.Combine(projectDirectory, "bin", os ?? "default", arch ?? "default", outputType ?? "default", "container.tar.gz");
            command += $" -p:ContainerArchiveOutputPath=\"{outputPath}\"";

            return command;
        }

        private string GetOsParameter(NetCorePlatform platform)
        {
            switch (platform)
            {
                case NetCorePlatform.Windows:
                    return "windows";
                case NetCorePlatform.Linux:
                    return "linux";
                case NetCorePlatform.MacOS:
                    return "osx";
                case NetCorePlatform.Undefined:
                default:
                    return null;
            }
        }

        private string GetArchParameter(NetCoreArchitecture architecture)
        {
            switch (architecture)
            {
                case NetCoreArchitecture.x86:
                    return "x86";
                case NetCoreArchitecture.x64:
                    return "x64";
                case NetCoreArchitecture.arm:
                    return "arm";
                case NetCoreArchitecture.arm64:
                    return "arm64";
                case NetCoreArchitecture.Undefined:
                default:
                    return null;
            }
        }

        private string GetOutputTypeParameter(NetCoreOutputType outputType)
        {
            switch (outputType)
            {
                case NetCoreOutputType.Exe:
                    return "exe";
                case NetCoreOutputType.DockerContainer:
                    return "docker";
                case NetCoreOutputType.Undefined:
                default:
                    return null;
            }
        }

        private static int DoDotnet(string workingDirectory, string arguments, out string output)
        {
            Process process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.WorkingDirectory = workingDirectory;
            process.StartInfo.FileName = "dotnet";
            process.StartInfo.Arguments = arguments;

            process.Start();

            output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (!string.IsNullOrEmpty(error))
            {
                output += System.Environment.NewLine + error;
            }

            return process.ExitCode;
        }
    }
}
