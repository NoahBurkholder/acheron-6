using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

// Plays at a certain time either during the sinking or during game-time.
public class SequenceSound : MonoBehaviour
{
    // Time to play.
    public float playAtTime;

    // Data instance.
    private FMOD.Studio.EventInstance soundInstance;

    // Name of sound in FMOD.
    [FMODUnity.EventRef]
    public string audioSound;

    // Does the timer start during the sinking, or when you hit play?
    public bool isDuringSink;

    private void Start()
    {
        StartCoroutine(SoundTiming());
    }
    private IEnumerator SoundTiming()
    {
        if (isDuringSink) // If this is during a sink, wait...
        {
            while (GameManager.isCalm)
            {
                // While the submarine isn't sinking, just wait.
                yield return GameManager.WaitFrame;
            }
        }

        // Begin timer.
        yield return new WaitForSeconds(playAtTime / GameManager.timeMultiplier);

        // Play sound.
        soundInstance = FMODUnity.RuntimeManager.CreateInstance(audioSound);
        soundInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
        soundInstance.start();
        soundInstance.release();
        yield break;
    }
}
