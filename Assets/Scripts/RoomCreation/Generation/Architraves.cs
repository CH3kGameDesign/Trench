using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Trench/AssetLists/Architraves", fileName = "New Architrave List")]
public class Architraves : ScriptableObject
{
    public List<LineRenderer> skirting = new List<LineRenderer>();
    public List<LineRenderer> cornices = new List<LineRenderer>();

    public Material defaultMaterial;
}
