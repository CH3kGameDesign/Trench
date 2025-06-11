using System.ComponentModel;
using UnityEngine;
public enum Consumable_Type
{
	[Description ("Health Potion")]	[InspectorName ("Item/HealthPotion")]	Item_HealthPotion,
	[Description ("Revive Potion")]	[InspectorName ("Item/RevivePotion")]	Item_RevivePotion,
}
