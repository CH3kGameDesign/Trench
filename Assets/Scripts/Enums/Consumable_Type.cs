using System.ComponentModel;
using UnityEngine;
public enum Consumable_Type
{
	[Description ("Health Potion")]	[InspectorName ("Potion/Health")]	Potion_Health,
	[Description ("Revive Spark")]	[InspectorName ("Potion/Revive")]	Potion_Revive,
}
