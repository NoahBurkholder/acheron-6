using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
public class Book : Pickupable
{
    public override void OnPickup(Hand h)
    {
        base.OnPickup(h);

    }

    private void Start()
    {
        StartCoroutine(BookRoutine());
    }
    private float rotation, oldRotation;
    private bool hasClosed = true;
    private FMOD.Studio.EventInstance closeInstance;
    [FMODUnity.EventRef]
    public string audioClose;
    public float bookClosePitch;
    private IEnumerator BookRoutine()
    {
        while (true)
        {
            //if (isReporting) Debug.Log(rotation);

            rotation = Mathf.Abs(thisTransform.localEulerAngles.x - 180);
            if (rotation > 175f)
            {
                if (rotation > oldRotation)
                {
                    if (!hasClosed)
                    {
                        hasClosed = true;
                        if (GameManager.time > 1f)
                        {
                            closeInstance = FMODUnity.RuntimeManager.CreateInstance(audioClose);
                            closeInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(thisTransform));
                            closeInstance.setParameterByName("Volume", Mathf.Clamp01(Mathf.Abs((rotation - oldRotation) * 0.1f)));
                            closeInstance.setParameterByName("Pitch", bookClosePitch);

                            closeInstance.start();
                            closeInstance.release();
                        }
                    }
                }
            } else
            {
                if (hasClosed)
                {
                    if (isReporting) Debug.Log(rotation);
                    hasClosed = false;
                }

            }

            oldRotation = rotation;
            yield return GameManager.WaitFrame;
        }
    }
}
