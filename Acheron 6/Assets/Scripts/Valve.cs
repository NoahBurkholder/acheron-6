using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
public class Valve : Pickupable
{
    private FMOD.Studio.EventInstance squeakInstance;
    private FMOD.Studio.EventInstance breakInstance;


    [FMODUnity.EventRef]
    public string audioSqueak;
    [FMODUnity.EventRef]
    public string audioBreak;

    private float magnitudeTarget;
    private float magnitudeSmoothed;
    public bool isAttached = true;
    public bool isSinkValve;
    public HingeJoint valveJoint;
    private void Start()
    {
        squeakInstance = FMODUnity.RuntimeManager.CreateInstance(audioSqueak);
        squeakInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(thisTransform));

    }
    private float strain;
    private FMOD.Studio.PLAYBACK_STATE state;
    private void Update()
    {
        // By smoothing changes of velocity values, we avoid strange audio distortion artifacts.
        magnitudeTarget = rigidBody.angularVelocity.magnitude / 10;
        magnitudeSmoothed += (magnitudeTarget - magnitudeSmoothed) * 0.01f;

        // Modify FMOD parameters on instance.
        squeakInstance.setParameterByName("Pitch", magnitudeSmoothed);
        squeakInstance.setParameterByName("Volume", magnitudeSmoothed);
        // If the valve is attached to its axis...
        if ((isAttached) && (rigidBody.angularVelocity.magnitude > 0.01f))
        {
            // And if the squeak audio isn't already playing...
            squeakInstance.getPlaybackState(out state);
            if (state != FMOD.Studio.PLAYBACK_STATE.PLAYING)
            {
                // Then begin the squeaking loop.
                squeakInstance.start();
            }

            // If this valve is the valve that sinks the submarine.
            if ((isSinkValve) && (isAttached))
            {
                // If you strain this valve too much...
                strain += rigidBody.angularVelocity.magnitude;
                if (strain >= 6000f)
                {
                    // Cause the submarine to sink.
                    GameManager.StartSink();

                    // Destroy this valve, causing the breaking SFX.
                    Destroy(valveJoint);

                    // Pop the valve off.
                    rigidBody.AddForce(thisTransform.right * -500f);
                    isAttached = false;
                }
            }
        } 
        else
        {
            // Otherwise stop the squeaking to save computational resources!
            squeakInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        }
    }

    private void OnJointBreak(float breakForce)
    {
        if (isSinkValve) GameManager.StartSink();
        breakInstance = FMODUnity.RuntimeManager.CreateInstance(audioBreak);
        breakInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(thisTransform));
        breakInstance.start();
        breakInstance.release();

        squeakInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        rigidBody.angularDrag = 0.05f;
        isAttached = false;
    }
    


    public override void OnPickup(Hand h)
    {
        base.OnPickup(h);
       
    }
    public override void OnDrop()
    {
        base.OnDrop();
    }
}
