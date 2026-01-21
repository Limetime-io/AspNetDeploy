using System.Diagnostics;

namespace SatelliteLinuxGrpcHost.Services
{
    public class DeploymentService : IDeploymentService
    {
        private bool isReady = true;
        private Exception? lastException;
        private MemoryStream? packageStream;

        public bool IsReady()
        {
            return isReady;
        }

        public bool BeginPublication(int publicationId)
        {
            try
            {
                Console.WriteLine($"Beginning publication: {publicationId}");
                return true;
            }
            catch (Exception ex)
            {
                lastException = ex;
                return false;
            }
        }

        public bool ExecuteNextOperation()
        {
            try
            {
                Console.WriteLine("Executing next operation");
                return true;
            }
            catch (Exception ex)
            {
                lastException = ex;
                return false;
            }
        }

        public bool Complete()
        {
            try
            {
                Console.WriteLine("Completing publication");
                return true;
            }
            catch (Exception ex)
            {
                lastException = ex;
                return false;
            }
        }

        public void Rollback()
        {
            Console.WriteLine("Rolling back");
        }

        public void ResetPackage()
        {
            packageStream?.Dispose();
            packageStream = new MemoryStream();
            Console.WriteLine("Package reset");
        }

        public void UploadPackageBuffer(byte[] buffer)
        {
            packageStream ??= new MemoryStream();
            packageStream.Write(buffer, 0, buffer.Length);
            Console.WriteLine($"Uploaded buffer: {buffer.Length} bytes");
        }

        public void DeployContainer(dynamic request)
        {
            try
            {
                Console.WriteLine($"Deploying container: {request.ContainerName}");

                // Extract container.tar.gz from package
                // Load image: docker load -i container.tar.gz
                // Run container with parameters

                string containerName = request.ContainerName;
                string ports = request.Ports ?? "";
                string envVars = request.EnvironmentVariables ?? "";
                string volumes = request.Volumes ?? "";
                string restartPolicy = request.RestartPolicy ?? "unless-stopped";
                string networks = request.Networks ?? "";

                // Build docker run command
                var dockerCommand = $"run -d --name {containerName}";

                if (!string.IsNullOrEmpty(restartPolicy))
                {
                    dockerCommand += $" --restart={restartPolicy}";
                }

                // Add ports
                if (!string.IsNullOrEmpty(ports))
                {
                    foreach (var port in ports.Split(','))
                    {
                        dockerCommand += $" -p {port.Trim()}";
                    }
                }

                // Add environment variables
                if (!string.IsNullOrEmpty(envVars))
                {
                    foreach (var env in envVars.Split('\n'))
                    {
                        if (!string.IsNullOrWhiteSpace(env))
                        {
                            dockerCommand += $" -e {env.Trim()}";
                        }
                    }
                }

                // Add volumes
                if (!string.IsNullOrEmpty(volumes))
                {
                    foreach (var volume in volumes.Split('\n'))
                    {
                        if (!string.IsNullOrWhiteSpace(volume))
                        {
                            dockerCommand += $" -v {volume.Trim()}";
                        }
                    }
                }

                // Add networks
                if (!string.IsNullOrEmpty(networks))
                {
                    foreach (var network in networks.Split('\n'))
                    {
                        if (!string.IsNullOrWhiteSpace(network))
                        {
                            dockerCommand += $" --network {network.Trim()}";
                        }
                    }
                }

                dockerCommand += " PROJECT:latest";

                Console.WriteLine($"Docker command: docker {dockerCommand}");

                // TODO: Execute docker command
                // ExecuteCommand("docker", dockerCommand);
            }
            catch (Exception ex)
            {
                lastException = ex;
                throw;
            }
        }

        public void ProcessConfigFile(dynamic request)
        {
            try
            {
                string file = request.File;
                string content = request.Content;

                Console.WriteLine($"Processing config file: {file}");
                File.WriteAllText(file, content);
            }
            catch (Exception ex)
            {
                lastException = ex;
                throw;
            }
        }

        public void RunPowerShellScript(dynamic request)
        {
            try
            {
                string script = request.Script;
                string workingDirectory = request.WorkingDirectory ?? Environment.CurrentDirectory;

                Console.WriteLine($"Running PowerShell script in: {workingDirectory}");

                var processInfo = new ProcessStartInfo
                {
                    FileName = "pwsh",
                    Arguments = $"-Command \"{script}\"",
                    WorkingDirectory = workingDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(processInfo);
                if (process != null)
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    process.WaitForExit();

                    if (process.ExitCode != 0)
                    {
                        throw new Exception($"PowerShell script failed: {error}");
                    }

                    Console.WriteLine($"Script output: {output}");
                }
            }
            catch (Exception ex)
            {
                lastException = ex;
                throw;
            }
        }

        public void CopyFiles(dynamic request)
        {
            try
            {
                string destination = request.Destination;
                int projectId = request.ProjectId;
                string mode = request.Mode ?? "overwrite";

                Console.WriteLine($"Copying files to: {destination}, Mode: {mode}");

                // TODO: Extract files from package and copy to destination
            }
            catch (Exception ex)
            {
                lastException = ex;
                throw;
            }
        }

        public void RunSQLScript(dynamic request)
        {
            try
            {
                string connectionString = request.ConnectionString;
                string command = request.Command;

                Console.WriteLine($"Running SQL script");

                // TODO: Execute SQL command
            }
            catch (Exception ex)
            {
                lastException = ex;
                throw;
            }
        }

        public ExceptionInfo GetLastException()
        {
            if (lastException == null)
            {
                return new ExceptionInfo();
            }

            return new ExceptionInfo
            {
                TypeName = lastException.GetType().Name,
                AssemblyQualifiedTypeName = lastException.GetType().AssemblyQualifiedName ?? "",
                Message = lastException.Message,
                Source = lastException.Source ?? "",
                StackTrace = lastException.StackTrace ?? "",
                ExceptionData = new List<ExceptionDataInfo>()
            };
        }
    }
}
