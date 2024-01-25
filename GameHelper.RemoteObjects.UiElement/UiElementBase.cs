using System;
using System.Numerics;
using GameHelper.Cache;
using GameHelper.Ui;
using GameHelper.Utils;
using GameOffsets.Objects.UiElement;
using ImGuiNET;

namespace GameHelper.RemoteObjects.UiElement;

public class UiElementBase : RemoteObjectBase
{
	private Vector2 positionModifier;

	private bool show;

	private nint[] childrenAddresses;

	private uint flags;

	private float localScaleMultiplier;

	private Vector2 relativePosition;

	private Vector2 unScaledSize;

	private nint parentAddress;

	private readonly UiElementParents parents;

	protected Vector4 backgroundColor;

	private byte scaleIndex;

	public virtual Vector2 Postion
	{
		get
		{
			(float WidthScale, float HeightScale) scaleValue = Core.GameScale.GetScaleValue(scaleIndex, localScaleMultiplier);
			float widthScale = scaleValue.WidthScale;
			float heightScale = scaleValue.HeightScale;
			Vector2 pos = GetUnScaledPosition();
			pos.X *= widthScale;
			pos.Y *= heightScale;
			pos.X += Core.GameCull.Value;
			return pos;
		}
	}

	public virtual Vector2 Size
	{
		get
		{
			(float WidthScale, float HeightScale) scaleValue = Core.GameScale.GetScaleValue(scaleIndex, localScaleMultiplier);
			float widthScale = scaleValue.WidthScale;
			float heightScale = scaleValue.HeightScale;
			Vector2 size = unScaledSize;
			size.X *= widthScale;
			size.Y *= heightScale;
			return size;
		}
	}

	public bool IsVisible
	{
		get
		{
			if (UiElementBaseFuncs.IsVisibleChecker(flags))
			{
				if (parentAddress != IntPtr.Zero)
				{
					return parents.GetParent(parentAddress).IsVisible;
				}
				return true;
			}
			return false;
		}
	}

	public int TotalChildrens => childrenAddresses.Length;

	[SkipImGuiReflection]
	public UiElementBase this[int i]
	{
		get
		{
			if (childrenAddresses.Length <= i)
			{
				return null;
			}
			return new UiElementBase(childrenAddresses[i], parents);
		}
	}

	internal UiElementBase(nint address, UiElementParents parents)
		: base(address, forceUpdate: true, skipFirstUpdate: true)
	{
		CleanUpData();
		this.parents = parents;
		if (address != IntPtr.Zero)
		{
			UpdateData(hasAddressChanged: true);
		}
	}

	public bool TryGetParent(out UiElementBase parent)
	{
		if (parentAddress != IntPtr.Zero)
		{
			parent = parents.GetParent(parentAddress);
			return true;
		}
		parent = null;
		return false;
	}

	internal override void ToImGui()
	{
		ImGui.Checkbox("Show", ref show);
		ImGui.SameLine();
		if (ImGui.Button("Explore"))
		{
			GameUiExplorer.AddUiElement(this);
		}
		base.ToImGui();
		if (show)
		{
			ImGuiHelper.DrawRect(Postion, Size, byte.MaxValue, byte.MaxValue, 0);
		}
		ImGui.Text($"Position  {Postion}");
		ImGui.Text($"Size  {Size}");
		ImGui.Text($"Unscaled Size {unScaledSize}");
		ImGui.Text($"IsVisible  {IsVisible}");
		ImGui.Text($"Total Childrens  {TotalChildrens}");
		ImGui.Text($"Parent  {((IntPtr)parentAddress).ToInt64():X}");
		ImGui.Text($"Position Modifier {positionModifier}");
		ImGui.Text($"Scale Index {scaleIndex}");
		ImGui.Text($"Local Scale Multiplier {localScaleMultiplier}");
		ImGui.Text($"Flags: {flags:X}");
		ImGui.Text("Background Color");
		ImGui.SameLine();
		ImGui.ColorButton("##UiElementBackgroundColor", backgroundColor);
	}

	protected override void CleanUpData()
	{
		positionModifier = Vector2.Zero;
		show = false;
		childrenAddresses = Array.Empty<nint>();
		flags = 0u;
		localScaleMultiplier = 1f;
		relativePosition = Vector2.Zero;
		unScaledSize = Vector2.Zero;
		scaleIndex = 0;
		parentAddress = IntPtr.Zero;
		backgroundColor = Vector4.Zero;
	}

	protected override void UpdateData(bool hasAddressChanged)
	{
		UpdateData(Core.Process.Handle.ReadMemory<UiElementBaseOffset>(base.Address), hasAddressChanged);
	}

	protected void UpdateData(UiElementBaseOffset data, bool hasAddressChanged)
	{
		if (data.Self != IntPtr.Zero && data.Self != base.Address)
		{
			throw new Exception($"This (address: {((IntPtr)base.Address).ToInt64():X})is not a Ui Element. Self Address = {((IntPtr)data.Self).ToInt64():X}");
		}
		parentAddress = data.ParentPtr;
		parents.AddIfNotExists(data.ParentPtr);
		childrenAddresses = Core.Process.Handle.ReadStdVector<nint>(data.ChildrensPtr);
		positionModifier.X = data.PositionModifier.X;
		positionModifier.Y = data.PositionModifier.Y;
		scaleIndex = data.ScaleIndex;
		localScaleMultiplier = data.LocalScaleMultiplier;
		flags = data.Flags;
		relativePosition.X = data.RelativePosition.X;
		relativePosition.Y = data.RelativePosition.Y;
		unScaledSize.X = data.UnscaledSize.X;
		unScaledSize.Y = data.UnscaledSize.Y;
		backgroundColor = ImGuiHelper.Color(data.BackgroundColor);
	}

	private Vector2 GetUnScaledPosition()
	{
		if (parentAddress == IntPtr.Zero)
		{
			return relativePosition;
		}
		UiElementBase myParent = parents.GetParent(parentAddress);
		Vector2 parentPos = myParent.GetUnScaledPosition();
		if (UiElementBaseFuncs.ShouldModifyPos(flags))
		{
			parentPos += myParent.positionModifier;
		}
		if (myParent.scaleIndex == scaleIndex && myParent.localScaleMultiplier == localScaleMultiplier)
		{
			return parentPos + relativePosition;
		}
		(float WidthScale, float HeightScale) scaleValue = Core.GameScale.GetScaleValue(myParent.scaleIndex, myParent.localScaleMultiplier);
		float parentScaleW = scaleValue.WidthScale;
		float parentScaleH = scaleValue.HeightScale;
		(float WidthScale, float HeightScale) scaleValue2 = Core.GameScale.GetScaleValue(scaleIndex, localScaleMultiplier);
		float myScaleW = scaleValue2.WidthScale;
		float myScaleH = scaleValue2.HeightScale;
		Vector2 myPos = default(Vector2);
		myPos.X = parentPos.X * parentScaleW / myScaleW + relativePosition.X;
		myPos.Y = parentPos.Y * parentScaleH / myScaleH + relativePosition.Y;
		return myPos;
	}
}
