using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class Pickupable : Interactable
{
    [Header("Audio")]
    private FMOD.Studio.EventInstance pickupInstance;

    [FMODUnity.EventRef]
    public string audioPickup;


    public virtual void OnPickup(Hand h)
    {
        pickupInstance = FMODUnity.RuntimeManager.CreateInstance(audioPickup);
        pickupInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(thisTransform));
        pickupInstance.start();
        pickupInstance.release();
    }

    public virtual void OnDrop()
    {
        pickupInstance = FMODUnity.RuntimeManager.CreateInstance(audioPickup);
        pickupInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(thisTransform));
        pickupInstance.start();
        pickupInstance.release();
    }
}
