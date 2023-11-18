using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using BeautifyEffect;
using UnityEngine.XR;
using Colorful;
public class Player : MonoBehaviour
{
    public static Transform PlayerTransform;
    private FMOD.Studio.EventInstance footstepInstance;
    private FMOD.Studio.EventInstance vocalInstance;

    [FMODUnity.EventRef]
    public string audioFootstep;

    [FMODUnity.EventRef]
    public string audioGasp;

    [FMODUnity.EventRef]
    public string audioGurgle;

    public CharacterController character;
    public Transform headTracker;
    public Beautify beautify;
    public bool isUnderwater;
    public Color waterTint;
    public static InputDevice Headset;
    public static InputDevice RightController;
    public static InputDevice LeftController;
    private Vector2 moveAxis;
    private Vector2 rotateAxis;
    private bool isWalking;
    private IEnumerator DrownRoutine;
    private void Start()
    {
        PlayerTransform = transform;
        DrownRoutine = Drowning();
        SetupControls();
    }
    private void SetupControls()
    {
        List<InputDevice> leftHandDevices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.LeftHand, leftHandDevices);

        if (leftHandDevices.Count > 0)
        {
            LeftController = leftHandDevices[0];
            Debug.Log(string.Format("Device name '{0}' with role '{1}'", LeftController.name, LeftController.role.ToString()));
        }

        List<InputDevice> rightHandDevices = new List<InputDevice>();
        InputDevices.GetDevicesAtXRNode(XRNode.RightHand, rightHandDevices);

        if (rightHandDevices.Count > 0)
        {
            RightController = rightHandDevices[0];
            Debug.Log(string.Format("Device name '{0}' with role '{1}'", RightController.name, RightController.role.ToString()));
        }
        

    }
    private void Update()
    {
        CheckUnderwater();
        HandleMovement();
        CheckReset();
    }

    public static void Reset()
    {


    }
    public bool shouldReset;
    public void CheckReset()
    {
        LeftController.TryGetFeatureValue(CommonUsages.menuButton, out shouldReset);
        if (shouldReset)
        {
            shouldReset = false;
            GameManager.Reset();
        }

    }
    private float steptimer;
    public void HandleMovement()
    {
        LeftController.TryGetFeatureValue(CommonUsages.primary2DAxis, out moveAxis);
        RightController.TryGetFeatureValue(CommonUsages.primary2DAxis, out rotateAxis);

        steptimer += (moveAxis.magnitude + Mathf.Abs(rotateAxis.x)) * Time.deltaTime;

        if (steptimer > 0.4f)
        {
            steptimer = 0;
            footstepInstance = FMODUnity.RuntimeManager.CreateInstance(audioFootstep);

            footstepInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
            footstepInstance.setParameterByName("Volume", moveAxis.magnitude * 4f);

            footstepInstance.start();
            footstepInstance.release();

        }

        if (moveAxis.magnitude > 0.01f)
        {
            character.Move((transform.forward * moveAxis.y) * 0.01f);
            character.Move((transform.right * moveAxis.x) * 0.01f);
        }

        if (rotateAxis.magnitude > 0.01f)
        {
            transform.Rotate(Vector3.up * (rotateAxis.x * 1f), Space.Self);
        }
    }
    private float drown;
    private float randomTimer;
    private float randomMax;
    public FastVignette vignette;
    private IEnumerator Drowning()
    {
        randomMax = 8f;
        randomTimer = Random.Range(randomMax / 2, randomMax);
        while (isUnderwater)
        {
            drown += Time.deltaTime;
            randomTimer -= Time.deltaTime;

            if (drown > 15)
            {
                vignette.Darkness = ((drown - 15f) * 10f);
            }
            if (randomTimer <= 0)
            {
                vocalInstance = FMODUnity.RuntimeManager.CreateInstance(audioGurgle);

                if (drown > 8f) vocalInstance.setParameterByName("Switch", 2);
                else if (drown > 4f) vocalInstance.setParameterByName("Switch", 1);
                else vocalInstance.setParameterByName("Switch", 0);

                vocalInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(headTracker));
                vocalInstance.start();
                vocalInstance.release();

                if (drown >= 25f) randomMax = 2f;
                else if (drown >= 20f) randomMax = 4f;
                else if (drown >= 15f) randomMax = 6f;
                else randomMax = 8f;
                randomTimer = Random.Range(randomMax / 2, randomMax);
            }

            yield return GameManager.WaitFrame;
        }

        drown = 0;

        vocalInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        vocalInstance = FMODUnity.RuntimeManager.CreateInstance(audioGasp);


        if (drown > 8f) vocalInstance.setParameterByName("Switch", 2);
        else if (drown > 4f) vocalInstance.setParameterByName("Switch", 1);
        else vocalInstance.setParameterByName("Switch", 0);
        vocalInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(headTracker));
        vocalInstance.start();
        vocalInstance.release();

        yield break;
    }
    public void CheckUnderwater()
    {
        if (headTracker.position.y < Water.water.transform.position.y)
        {

            if (!isUnderwater)
            {
                isUnderwater = true;

                StartCoroutine(Drowning());
                Water.water.renderers[0].enabled = false;
                Water.water.renderers[1].enabled = true;

                Water.water.splashInstance = FMODUnity.RuntimeManager.CreateInstance(Water.water.audioSplash);

                Water.water.splashInstance.setParameterByName("Switch", 2);
                
                Water.water.splashInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(headTracker));
                Water.water.splashInstance.start();
                Water.water.splashInstance.release();

                Water.water.muffleInstance = FMODUnity.RuntimeManager.CreateInstance(Water.water.audioMuffle);
                Water.water.muffleInstance.start();
                Water.water.muffleInstance.release();

                beautify.depthOfField = true;
                waterTint = new Color(waterTint.r, waterTint.g, waterTint.b, 1f);
                beautify.tintColor = waterTint;
            }
        }
        else
        {
            if (isUnderwater)
            {
                isUnderwater = false;

                Water.water.renderers[0].enabled = true;
                Water.water.renderers[1].enabled = false;


                Water.water.splashInstance = FMODUnity.RuntimeManager.CreateInstance(Water.water.audioSplash);

                Water.water.splashInstance.setParameterByName("Switch", 0);

                Water.water.splashInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(headTracker));
                Water.water.splashInstance.start();
                Water.water.splashInstance.release();

                Water.water.muffleInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                beautify.depthOfField = false;
                waterTint = new Color(waterTint.r, waterTint.g, waterTint.b, 0);
                beautify.tintColor = waterTint;
            }
        }
    }
}
