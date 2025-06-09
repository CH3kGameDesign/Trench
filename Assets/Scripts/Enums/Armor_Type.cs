using System.ComponentModel;
using UnityEngine;
public enum Armor_Type
{
	[Description ("Empty")]	[InspectorName ("Helmet/Empty")]	Helmet_Empty,
	[Description ("Basic")]	[InspectorName ("Helmet/Basic")]	Helmet_Basic,
	[Description ("DayDreamer")]	[InspectorName ("Helmet/DayDreamer")]	Helmet_DayDreamer,
	[Description ("RangeFinder")]	[InspectorName ("Helmet/RangeFinder")]	Helmet_RangeFinder,
	[Description ("Shaved")]	[InspectorName ("Helmet/Shaved")]	Helmet_Shaved,
	[Description ("Empty")]	[InspectorName ("Chest/Empty")]	Chest_Empty,
	[Description ("Basic")]	[InspectorName ("Chest/Basic")]	Chest_Basic,
	[Description ("Empty")]	[InspectorName ("Arm/Empty")]	Arm_Empty,
	[Description ("Basic")]	[InspectorName ("Arm/Basic")]	Arm_Basic,
	[Description ("Empty")]	[InspectorName ("Leg/Empty")]	Leg_Empty,
	[Description ("Basic")]	[InspectorName ("Leg/Basic")]	Leg_Basic,
	[Description ("Black")]	[InspectorName ("Material/Black")]	Material_Black,
	[Description ("Green")]	[InspectorName ("Material/Green")]	Material_Green,
	[Description ("Orange")]	[InspectorName ("Material/Orange")]	Material_Orange,
	[Description ("Red")]	[InspectorName ("Material/Red")]	Material_Red,
}
