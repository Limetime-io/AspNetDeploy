using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcSatelliteLinux;

namespace SatelliteLinuxGrpcHost.Services
{
    public class DeploymentController : Deployment.DeploymentBase
    {
        private readonly IDeploymentService deploymentService;

        public DeploymentController(IDeploymentService deploymentService)
        {
            this.deploymentService = deploymentService;
        }

        public override Task<IsReadyResponse> IsReady(Empty request, ServerCallContext context)
        {
            Console.WriteLine("IsReady called");
            return Task.FromResult(new IsReadyResponse()
            {
                IsReady = this.deploymentService.IsReady()
            });
        }

        public override Task<GetLastExceptionResponse> GetLastException(Empty request, ServerCallContext context)
        {
            ExceptionInfo exception = this.deploymentService.GetLastException();

            var response = new GetLastExceptionResponse()
            {
                TypeName = exception.TypeName,
                AssemblyQualifiedTypeName = exception.AssemblyQualifiedTypeName,
                Message = exception.Message,
                Source = exception.Source,
                StackTrace = exception.StackTrace
            };

            foreach (var data in exception.ExceptionData)
            {
                response.ExceptionData.Add(new GrpcSatelliteLinux.ExceptionDataInfo()
                {
                    Name = data.Name,
                    Value = data.Value,
                    IsProperty = data.IsProperty
                });
            }

            return Task.FromResult(response);
        }

        public override Task<DeploymentServiceBasicResponse> BeginPublication(BeginPublicationRequest request, ServerCallContext context)
        {
            Console.WriteLine($"Begin publication: {request.PublicationId}");
            return Task.FromResult(new DeploymentServiceBasicResponse()
            {
                IsSuccess = this.deploymentService.BeginPublication(request.PublicationId)
            });
        }

        public override Task<DeploymentServiceBasicResponse> ExecuteNextOperation(Empty request, ServerCallContext context)
        {
            Console.WriteLine("Execute next operation");
            return Task.FromResult(new DeploymentServiceBasicResponse()
            {
                IsSuccess = this.deploymentService.ExecuteNextOperation()
            });
        }

        public override Task<DeploymentServiceBasicResponse> Complete(Empty request, ServerCallContext context)
        {
            Console.WriteLine("Completing");
            return Task.FromResult(new DeploymentServiceBasicResponse()
            {
                IsSuccess = this.deploymentService.Complete()
            });
        }

        public override Task<Empty> Rollback(Empty request, ServerCallContext context)
        {
            Console.WriteLine("Rolling back");
            this.deploymentService.Rollback();
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> ResetPackage(Empty request, ServerCallContext context)
        {
            Console.WriteLine("Resetting package");
            this.deploymentService.ResetPackage();
            return Task.FromResult(new Empty());
        }

        public override async Task<Empty> UploadPackageBuffer(IAsyncStreamReader<UploadPackageBufferRequest> requestStream, ServerCallContext context)
        {
            Console.WriteLine("Uploading package buffer");
            while (await requestStream.MoveNext())
            {
                UploadPackageBufferRequest request = requestStream.Current;

                byte[] bytes = new byte[request.Buffer.Length];
                request.Buffer.CopyTo(bytes, 0);

                this.deploymentService.UploadPackageBuffer(bytes);
            }

            return new Empty();
        }

        public override Task<Empty> DeployContainer(DeployContainerRequest request, ServerCallContext context)
        {
            Console.WriteLine($"Deploying container: {request.ContainerName}");
            this.deploymentService.DeployContainer((dynamic)request);
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> ProcessConfigFile(ProcessConfigFileRequest request, ServerCallContext context)
        {
            Console.WriteLine($"Processing config file: {request.File}");
            this.deploymentService.ProcessConfigFile((dynamic)request);
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> RunPowerShellScript(RunPowerShellScriptRequest request, ServerCallContext context)
        {
            Console.WriteLine($"Running PowerShell script");
            this.deploymentService.RunPowerShellScript((dynamic)request);
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> CopyFiles(CopyFilesRequest request, ServerCallContext context)
        {
            Console.WriteLine($"Copying files to: {request.Destination}");
            this.deploymentService.CopyFiles((dynamic)request);
            return Task.FromResult(new Empty());
        }

        public override Task<Empty> RunSQLScripts(RunSQLScriptsRequest request, ServerCallContext context)
        {
            Console.WriteLine($"Running SQL scripts");
            this.deploymentService.RunSQLScript((dynamic)request);
            return Task.FromResult(new Empty());
        }
    }
}
