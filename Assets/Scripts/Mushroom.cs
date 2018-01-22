using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Mushroom : NetworkBehaviour
{
    public static List<Mushroom> list;

    [ServerCallback]
    private void Awake()
    {
        if( list == null )
        {
            list = new List<Mushroom>();
        }

        list.Add(this);
    }


    [ServerCallback]
    private void OnDestroy()
    {
        if( list.Contains(this))
        {
            list.Remove(this);
        }
    }
}
