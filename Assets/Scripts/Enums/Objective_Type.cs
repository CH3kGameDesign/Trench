using System.ComponentModel;
using UnityEngine;
public enum Objective_Type
{
	[Description ("Defeat Enemies")]	[InspectorName ("Kill/Any")]	Kill_Any,
	[Description ("Collect Money via Treasure")]	[InspectorName ("Collect/Value")]	Collect_Value,
	[Description ("Collect ")]	[InspectorName ("Collect/Resource")]	Collect_Resource,
	[Description ("Collect Enemy Bodies")]	[InspectorName ("Collect/Enemy")]	Collect_Enemy,
	[Description ("Damage Enemies with Rifle")]	[InspectorName ("Damage/Rifle")]	Damage_Rifle,
	[Description ("Damage Enemies with Rod Gun")]	[InspectorName ("Damage/Rod")]	Damage_Rod,
	[Description ("Damage Enemies with RPG")]	[InspectorName ("Damage/RPG")]	Damage_RPG,
	[Description ("Damage Enemies with Explosions")]	[InspectorName ("Damage/Explosions")]	Damage_Explosions,
}
