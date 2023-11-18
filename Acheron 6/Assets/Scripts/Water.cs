using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class Water : MonoBehaviour
{
    public Transform particleTransform;
    private float lifetime;
    public ParticleSystem system;
    public ParticleSystem systemSpray;
    public ParticleSystem systemRipple;
    public bool haveParticlesEnded;

    public bool debugWater;
    public bool startAutomatically;
    public static Water water;
    public float playAtTime;
    public MeshRenderer[] renderers;

    public FMOD.Studio.EventInstance flowInstance;
    public FMOD.Studio.EventInstance sprayInstance;

    public FMOD.Studio.EventInstance muffleInstance;
    public FMOD.Studio.EventInstance splashInstance;

    [FMODUnity.EventRef]
    public string audioFlow;
    [FMODUnity.EventRef]
    public string audioSpray;
    [FMODUnity.EventRef]
    public string audioMuffle;
    [FMODUnity.EventRef]
    public string audioSplash;


    private Vector3 startPosition;
    public Vector3 waterTop;
    public Vector3 waterBottom;

    private Vector3 particleStartPosition;
    public Vector3 particleStart;
    public Vector3 particleEnd;

    public float lerp, particleLerp;
    public float timer;
    public float startTimer;


    private void Awake()
    {
        startPosition = transform.localPosition;
        particleStartPosition = particleTransform.position;

        timer = startTimer;
        water = this;
        if (debugWater) transform.position += Vector3.up * 1f;
        StartCoroutine(Timing());
    }

    // Routine which handles the raising water level and change in reverb environment.
    private IEnumerator Timing()
    {
        muffleInstance = FMODUnity.RuntimeManager.CreateInstance(audioMuffle);

        flowInstance = FMODUnity.RuntimeManager.CreateInstance(audioFlow);
        flowInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(system.gameObject));
        flowInstance = FMODUnity.RuntimeManager.CreateInstance(audioSpray);
        flowInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(systemSpray.gameObject));
        var main = system.main;
        var sprayMain = systemSpray.main;
        // Set the Space parameter in FMOD to the maximum value, 1.
        muffleInstance.setParameterByName("Space", 1f);
        if (startAutomatically) GameManager.StartSink();

        while (GameManager.isCalm)
        {

            yield return GameManager.WaitFrame;
        }

        
        // Wait for X number of seconds.
        yield return new WaitForSeconds(playAtTime);

        // Play the water spray sound.
        flowInstance.start();

        // After half a second, start the particle system to synchronize with the water spray.
        yield return GameManager.Wait05;
        system.Play();
        systemSpray.Play();
        systemRipple.Play();

        // Loop which raises the water slowly, and changes reverb progressively as water rises.
        while (timer > 0)
        {
            main.startLifetime = 0.25f * Mathf.Clamp01(timer / startTimer);
            sprayMain.startSpeed = 2 + Mathf.Clamp01(timer / startTimer);
            if (!haveParticlesEnded)
            {
                if (timer < startTimer - 80)
                {
                    system.Stop();
                    haveParticlesEnded = true;
                }
                else if (timer < startTimer - 60)
                {
                    systemSpray.Stop();
                    systemRipple.Stop();
                }
            }
            // Move the water plane upwards with a lerp.
            transform.localPosition = startPosition + Vector3.Lerp(waterBottom, waterTop, lerp);
            particleTransform.position = particleStartPosition + Vector3.Lerp(particleStart, particleEnd, particleLerp);

            // Tick the timer down.
            timer -= Time.deltaTime * GameManager.timeMultiplier;

            // Calculate the lerp for next frame. This is a value that starts at 0 and goes up to 1.
            lerp = Mathf.Abs((timer / startTimer) - 1f);
            particleLerp = Mathf.Abs((Mathf.Clamp(timer +  8f, 0, startTimer) / startTimer) - 1f);
            // Re-use the lerp for the FMOD parameter responsible for reverb.
            muffleInstance.setParameterByName("Space", lerp);

            // Wait one frame before beginning the loop again. This avoids an infinite loop.
            yield return GameManager.WaitFrame;
        }
        yield break;
    }
}
