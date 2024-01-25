using System.Reflection;

namespace GameHelper.Utils;

internal struct RemoteObjectPropertyDetail
{
	public string Name;

	public object Value;

	public MethodInfo ToImGui;
}
