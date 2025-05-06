using System.ComponentModel;
using UnityEngine;
public enum Objective_Type
{
	[Description ("Defeat Enemies")]	[InspectorName ("Kill/Any")]	Kill_Any,
	[Description ("Collect Money via Treasure")]	[InspectorName ("Collect/Value")]	Collect_Value,
}
