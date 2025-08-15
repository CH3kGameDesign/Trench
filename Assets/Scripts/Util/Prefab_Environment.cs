using UnityEngine;

public class Prefab_Environment : MonoBehaviour
{
    public string _name = "";
    public TypeEnum _type;
    public enum TypeEnum { floor, wall, ceiling, door, fence}

    [HideInInspector] public SurfaceUpdater SU_surface = null;

    public string GetName()
    {
        if (_name == "") return gameObject.name;
        else return _name;
    }
}
