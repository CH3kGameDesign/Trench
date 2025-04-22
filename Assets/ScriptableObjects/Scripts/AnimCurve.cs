using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Visual/Anim Curve Object", fileName = "New Anim Curve")]
public class AnimCurve : ScriptableObject
{
    public AnimationCurve curve = new AnimationCurve();

    public float Evaluate(float point)
    {
        return curve.Evaluate(point);
    }
}
