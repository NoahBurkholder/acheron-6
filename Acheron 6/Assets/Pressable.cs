using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pressable : MonoBehaviour
{
    public bool isOn;
    private float cooldown = 0.3f;
    private float timer;
    public Material onMaterial;
    public Material offMaterial;
    public MeshRenderer renderer;

    private FMOD.Studio.EventInstance radioInstance;
    private FMOD.Studio.EventInstance pressInstance;


    [FMODUnity.EventRef]
    public string audioRadio;

    [FMODUnity.EventRef]
    public string audioPress;
    public Transform SpeakerTransform;

    private void Start()
    {
        radioInstance = FMODUnity.RuntimeManager.CreateInstance(audioRadio);
        radioInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(SpeakerTransform));

    }
    public void Press()
    {
        if (timer <= 0)
        {
            isOn = !isOn;
            timer = cooldown;
            if (isOn)
            {
                Material[] mats = renderer.materials;
                mats[1] = onMaterial;
                renderer.materials = mats;
                radioInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(SpeakerTransform));

                radioInstance.start();

                pressInstance = FMODUnity.RuntimeManager.CreateInstance(audioPress);
                pressInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
                pressInstance.start();
                pressInstance.release();
            } else
            {
                Material[] mats = renderer.materials;
                mats[1] = offMaterial;
                renderer.materials = mats; 
                radioInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(SpeakerTransform));
                radioInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                pressInstance = FMODUnity.RuntimeManager.CreateInstance(audioPress);
                pressInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
                pressInstance.start();
                pressInstance.release();
            }

        }
    }
    private void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }

    }
}
