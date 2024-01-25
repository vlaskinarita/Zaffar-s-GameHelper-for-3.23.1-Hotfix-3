using System.Reflection;
using System.Runtime.Loader;

namespace GameHelper.Plugin;

internal class PluginAssemblyLoadContext : AssemblyLoadContext
{
	private readonly AssemblyDependencyResolver resolver;

	public PluginAssemblyLoadContext(string assemblyLocation)
	{
		resolver = new AssemblyDependencyResolver(assemblyLocation);
	}

	protected override Assembly Load(AssemblyName assemblyName)
	{
		string path = resolver.ResolveAssemblyToPath(assemblyName);
		if (path != null)
		{
			return LoadFromAssemblyPath(path);
		}
		return null;
	}
}
