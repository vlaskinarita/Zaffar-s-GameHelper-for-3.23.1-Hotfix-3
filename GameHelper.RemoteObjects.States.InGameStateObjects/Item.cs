using GameHelper.RemoteEnums.Entity;
using GameOffsets.Objects.States.InGameState;

namespace GameHelper.RemoteObjects.States.InGameStateObjects;

public class Item : Entity
{
	internal Item(nint address)
		: base(address)
	{
	}

	protected override void UpdateData(bool hasAddressChanged)
	{
		ItemStruct itemData = Core.Process.Handle.ReadMemory<ItemStruct>(base.Address);
		base.IsValid = true;
		base.EntityType = EntityTypes.Item;
		base.EntitySubtype = EntitySubtypes.InventoryItem;
		if (!UpdateComponentData(itemData, hasAddressChanged))
		{
			UpdateComponentData(itemData, hasAddressChanged: true);
		}
	}
}
