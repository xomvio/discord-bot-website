using Microsoft.Extensions.Localization;
using System.Reflection;

namespace Bitguard.Services
{
    public class SharedResource
    {

    }
    public class LanguageService
    {
        private readonly IStringLocalizer localizer;

        public LanguageService(IStringLocalizerFactory factory)
        {
            var type = typeof(SharedResource);
            var assemblyName = new AssemblyName(type.GetTypeInfo().Assembly.FullName ?? "noAssemblyName");
            localizer = factory.Create(nameof(SharedResource), assemblyName.Name ?? "noAssemblyName");
        }
        public string Get(string key)
        {
            return localizer[key].Value;
        }
    }
}
