using System.ComponentModel;
using UnityEngine;
public enum Armor_Type
{
	[Description ("Empty")]	[InspectorName ("Helmet/Empty")]	Helmet_Empty,
	[Description ("Basic")]	[InspectorName ("Helmet/Basic")]	Helmet_Basic,
	[Description ("Conscript")]	[InspectorName ("Helmet/Conscript")]	Helmet_Conscript,
	[Description ("DayDreamer")]	[InspectorName ("Helmet/DayDreamer")]	Helmet_DayDreamer,
	[Description ("ion")]	[InspectorName ("Helmet/ion")]	Helmet_ion,
	[Description ("RangeFinder")]	[InspectorName ("Helmet/RangeFinder")]	Helmet_RangeFinder,
	[Description ("Recruit")]	[InspectorName ("Helmet/Recruit")]	Helmet_Recruit,
	[Description ("Shaved")]	[InspectorName ("Helmet/Shaved")]	Helmet_Shaved,
	[Description ("Empty")]	[InspectorName ("Chest/Empty")]	Chest_Empty,
	[Description ("Basic")]	[InspectorName ("Chest/Basic")]	Chest_Basic,
	[Description ("Ion")]	[InspectorName ("Chest/Ion")]	Chest_Ion,
	[Description ("Recruit")]	[InspectorName ("Chest/Recruit")]	Chest_Recruit,
	[Description ("Empty")]	[InspectorName ("Arm/Empty")]	Arm_Empty,
	[Description ("Basic")]	[InspectorName ("Arm/Basic")]	Arm_Basic,
	[Description ("Ion")]	[InspectorName ("Arm/Ion")]	Arm_Ion,
	[Description ("Empty")]	[InspectorName ("Leg/Empty")]	Leg_Empty,
	[Description ("Basic")]	[InspectorName ("Leg/Basic")]	Leg_Basic,
	[Description ("Ion")]	[InspectorName ("Leg/Ion")]	Leg_Ion,
	[Description ("Black")]	[InspectorName ("Material/Black")]	Material_Black,
	[Description ("Green")]	[InspectorName ("Material/Green")]	Material_Green,
	[Description ("Orange")]	[InspectorName ("Material/Orange")]	Material_Orange,
	[Description ("Red")]	[InspectorName ("Material/Red")]	Material_Red,
}
