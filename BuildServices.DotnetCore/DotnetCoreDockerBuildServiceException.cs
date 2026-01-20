using System;
using System.Runtime.Serialization;
using AspNetDeploy.Contracts.Exceptions;

namespace AspNetDeploy.BuildServices.DotnetCore
{
    public class DotnetCoreDockerBuildServiceException : AspNetDeployException
    {
        public DotnetCoreDockerBuildServiceException()
        {
        }

        public DotnetCoreDockerBuildServiceException(string message) : base(message)
        {
        }

        public DotnetCoreDockerBuildServiceException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DotnetCoreDockerBuildServiceException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
