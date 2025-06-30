using System.ComponentModel;
using UnityEngine;
public enum Gun_Type
{
	[Description ("Rifle Gun")]	[InspectorName ("gun/Rifle")]	gun_Rifle,
	[Description ("Rocket Gun")]	[InspectorName ("gun/Rocket")]	gun_Rocket,
	[Description ("Rod Gun")]	[InspectorName ("gun/Rod")]	gun_Rod,
	[Description ("Shotgun Gun")]	[InspectorName ("gun/Shotgun")]	gun_Shotgun,
}
