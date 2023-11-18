using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
public class Microphone : Pickupable
{
    public Transform SpeakerTransform;
    private FMOD.Studio.EventInstance feedbackInstance;


    [FMODUnity.EventRef]
    public string audioFeedback;
    private void Start()
    {
        StartCoroutine(FeedbackRoutine());
    }


    private float feedbackTarget;
    private float feedback;

    private IEnumerator FeedbackRoutine()
    {
        feedbackInstance = FMODUnity.RuntimeManager.CreateInstance(audioFeedback);
        feedbackInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(SpeakerTransform));
        feedbackInstance.start();
        feedbackInstance.release();
        while (true)
        {
            feedbackTarget = 1 - Mathf.Clamp((SpeakerTransform.position - thisTransform.position).magnitude * 3, 0, 1f);



            feedback = Mathf.Lerp(feedback, feedbackTarget, 0.1f);
            feedbackInstance.setParameterByName("Volume", feedback);
            yield return GameManager.WaitFrame;
        }
        yield break;
    }
}
