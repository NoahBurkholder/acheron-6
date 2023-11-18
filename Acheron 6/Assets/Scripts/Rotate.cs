using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    public Transform thisTransform;
    public Vector3 delta;
    private void Start()
    {
        thisTransform = transform;
        StartCoroutine(Rotating());
    }
    private IEnumerator Rotating()
    {
        while (true)
        {
            thisTransform.localEulerAngles += delta;
            yield return GameManager.WaitFrame;
        }
    }
}
