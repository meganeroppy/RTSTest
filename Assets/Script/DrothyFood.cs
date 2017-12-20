using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  ドロシーの食べ物
/// </summary>
public class DrothyFood : Photon.MonoBehaviour
{
    public enum Type
    {
        SmallenCake,
        LargenCake,
        Mushroom,
    }

    private Type myType;
}
