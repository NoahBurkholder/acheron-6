using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
public class Agitator : MonoBehaviour
{
    private FMOD.Studio.EventInstance agitationInstance;

    [FMODUnity.EventRef]
    public string audioAgitation;
    
    public Rigidbody rigidBody;
    private float magnitudeTarget;
    private float magnitudePrevious;
    private float magnitudeSmoothed;
    public float pitchMultiplier;
    public float volumeMultipier;
    public float threshold;
    public float speed;

    public float cooldown;
    private float timer;

    public bool onRotation;
    public bool onTranslation;
    public bool isReporting;
    void Update()
    {
        if (onRotation)
        {
            magnitudeTarget = Mathf.Clamp(rigidBody.angularVelocity.sqrMagnitude * 0.1f, 0, 1);
            magnitudeSmoothed += (magnitudeTarget - magnitudeSmoothed) * speed;

            if (rigidBody.velocity.sqrMagnitude * rigidBody.angularVelocity.sqrMagnitude > threshold)
            {
                if (timer <= 0)
                {
                    agitationInstance = FMODUnity.RuntimeManager.CreateInstance(audioAgitation);

                    agitationInstance.setParameterByName("Pitch", magnitudeSmoothed * pitchMultiplier);
                    agitationInstance.setParameterByName("Volume", magnitudeSmoothed * volumeMultipier);

                    agitationInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
                    agitationInstance.start();
                    agitationInstance.release();
                    timer = cooldown;
                }
            }
            

            magnitudePrevious = magnitudeTarget;
        }
        timer -= Time.deltaTime;
    }
}
