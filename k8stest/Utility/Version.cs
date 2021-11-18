using System.Reflection;

namespace k8stest.Utility
{
    public static class Version
    {
        public static string GetCurrent()
        {
            var fileVersion = Assembly
             .GetEntryAssembly()?
             .GetCustomAttribute<AssemblyFileVersionAttribute>()?
             .Version;

            return !string.IsNullOrEmpty(fileVersion)
                ? fileVersion
                : Assembly.GetEntryAssembly()?
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                    .InformationalVersion ?? "n/a";
        }
    }
}