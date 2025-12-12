using Newtonsoft.Json;

namespace VisitorRegistry.Web.Infrastructure
{
    public static class JsonSerializer
    {
        public static string ToJsonCamelCase(object obj)
        {
            return JsonConvert.SerializeObject(obj, new JsonSerializerSettings { ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver() });
        }
    }
}
