using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GameHelper.Utils;
using ImGuiNET;

namespace GameHelper.RemoteObjects;

public abstract class RemoteObjectBase
{
	[AttributeUsage(AttributeTargets.Property)]
	protected class SkipImGuiReflection : Attribute
	{
	}

	private readonly bool forceUpdate;

	private nint address;

	public nint Address
	{
		get
		{
			return address;
		}
		set
		{
			bool hasAddressChanged = address != value;
			if (hasAddressChanged || forceUpdate)
			{
				address = value;
				if (value == IntPtr.Zero)
				{
					CleanUpData();
				}
				else
				{
					UpdateData(hasAddressChanged);
				}
			}
		}
	}

	internal RemoteObjectBase()
	{
		throw new NotImplementedException();
	}

	internal RemoteObjectBase(nint address, bool forceUpdate = false, bool skipFirstUpdate = false)
	{
		this.forceUpdate = forceUpdate;
		if (skipFirstUpdate)
		{
			this.address = address;
		}
		else
		{
			Address = address;
		}
	}

	internal virtual void ToImGui()
	{
		BindingFlags propFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		IEnumerable<RemoteObjectPropertyDetail> toImGuiMethods = GetToImGuiMethods(GetType(), propFlags, this);
		ImGuiHelper.IntPtrToImGui("Address", address);
		foreach (RemoteObjectPropertyDetail property in toImGuiMethods)
		{
			if (ImGui.TreeNode(property.Name))
			{
				property.ToImGui.Invoke(property.Value, null);
				ImGui.TreePop();
			}
		}
	}

	protected abstract void UpdateData(bool hasAddressChanged);

	protected abstract void CleanUpData();

	internal static IEnumerable<RemoteObjectPropertyDetail> GetToImGuiMethods(Type classType, BindingFlags propertyFlags, object classObject)
	{
		BindingFlags methodFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		List<PropertyInfo> properties = classType.GetProperties(propertyFlags).ToList();
		for (int i = 0; i < properties.Count; i++)
		{
			PropertyInfo property = properties[i];
			if (Attribute.IsDefined(property, typeof(SkipImGuiReflection)))
			{
				continue;
			}
			object propertyValue = property.GetValue(classObject);
			if (propertyValue != null)
			{
				Type propertyType = propertyValue.GetType();
				if (typeof(RemoteObjectBase).IsAssignableFrom(propertyType))
				{
					yield return new RemoteObjectPropertyDetail
					{
						Name = property.Name,
						Value = propertyValue,
						ToImGui = propertyType.GetMethod("ToImGui", methodFlags)
					};
				}
			}
		}
	}
}
