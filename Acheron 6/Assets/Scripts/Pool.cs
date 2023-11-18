using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pool : MonoBehaviour
{

    public static List<Interactable> objects;
    public static void Reset()
    {
        if (objects != null)
        {
            foreach (Interactable i in objects)
            {
                i.OnReset();
            }
        }

    }
}
