using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CKUtil
{
    public static string ToString_Currency(this int _value)
    {
        string _temp = "$" + _value.ToString();
        return _temp;
    }
}
