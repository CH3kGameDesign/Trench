using System.ComponentModel;
using UnityEngine;
public enum Gun_Type
{
	[Description ("Rifle Gun")]	[InspectorName ("gun_Rifle")]	gun_Rifle,
	[Description ("Rocket Gun")]	[InspectorName ("gun_Rocket")]	gun_Rocket,
	[Description ("Rod Gun")]	[InspectorName ("gun_Rod")]	gun_Rod,
	[Description ("Shotgun Gun")]	[InspectorName ("gun_Shotgun")]	gun_Shotgun,
}
