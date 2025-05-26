using System.ComponentModel;
using UnityEngine;
public enum Armor_Type
{
	[Description ("None")]	[InspectorName ("Helmet/Empty")]	Helmet_Empty,
	[Description ("Basic")]	[InspectorName ("Helmet/Basic")]	Helmet_Basic,
	[Description ("Day Dreamer")]	[InspectorName ("Helmet/DayDreamer")]	Helmet_DayDreamer,
	[Description ("Range Finder")]	[InspectorName ("Helmet/RangeFinder")]	Helmet_RangeFinder,
	[Description ("Shaved")]	[InspectorName ("Helmet/Shaved")]	Helmet_Shaved,
	[Description ("None")]	[InspectorName ("Chest/Empty")]	Chest_Empty,
	[Description ("Basic")]	[InspectorName ("Chest/Basic")]	Chest_Basic,
	[Description ("None")]	[InspectorName ("Arm/Empty")]	Arm_Empty,
	[Description ("Basic")]	[InspectorName ("Arm/Basic")]	Arm_Basic,
	[Description ("None")]	[InspectorName ("Leg/Empty")]	Leg_Empty,
	[Description ("Basic")]	[InspectorName ("Leg/Basic")]	Leg_Basic,
}
