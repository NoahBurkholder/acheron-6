using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
public class SequenceEnable : MonoBehaviour
{
    public float playAtTime;
    private FMOD.Studio.EventInstance soundInstance;
    [FMODUnity.EventRef]
    public string audioSound;
    public bool isDuringSink;

    public GameObject enabledObject;
    public GameObject disabledObject;

    public bool shouldDisable;
    private void Start()
    {
        StartCoroutine(Timing());
    }
    private IEnumerator Timing()
    {
        if (isDuringSink)
        {
            while (GameManager.isCalm)
            {
                yield return GameManager.WaitFrame;
            }

        }
        yield return new WaitForSeconds(playAtTime / GameManager.timeMultiplier);
        if (shouldDisable) disabledObject.SetActive(false);
        enabledObject.SetActive(true);
        soundInstance = FMODUnity.RuntimeManager.CreateInstance(audioSound);
        soundInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(enabledObject.transform));
        soundInstance.start();
        soundInstance.release();
        yield break;
    }
}
