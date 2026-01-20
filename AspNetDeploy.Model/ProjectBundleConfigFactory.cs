using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AspNetDeploy.Model
{
    public class ProjectBundleConfigFactory
    {
        public static ProjectBundleConfig Create(string configurationJson)
        {
            if (string.IsNullOrWhiteSpace(configurationJson))
            {
                return null;
            }

            try
            {
                JObject json = JObject.Parse(configurationJson);

                int version = json["Version"]?.Value<int>() ?? 1;
                ProjectBundleConfigType type = (ProjectBundleConfigType)(json["Type"]?.Value<int>() ?? 0);

                if (version == 1)
                {
                    switch (type)
                    {
                        case ProjectBundleConfigType.NetCore:
                            return JsonConvert.DeserializeObject<NetCoreProjectBundleConfig>(configurationJson);

                        case ProjectBundleConfigType.Undefined:
                        default:
                            return JsonConvert.DeserializeObject<ProjectBundleConfig>(configurationJson);
                    }
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string Serialize(ProjectBundleConfig config)
        {
            if (config == null)
            {
                return null;
            }

            return JsonConvert.SerializeObject(config);
        }
    }
}
