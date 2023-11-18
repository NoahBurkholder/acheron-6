using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using FMODUnity;
using UnityEngine.XR;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    public static GameManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = (GameManager)FindObjectOfType(typeof(GameManager));

                if (_instance == null)
                {
                    Debug.LogError("An instance of " + typeof(GameManager) +
                        " is needed in the scene, but there is none.");
                }
            }

            return _instance;
        }
    }

    public static float timeMultiplier = 1f;
    public static WaitForSeconds Wait1 = new WaitForSeconds(1f);
    public static WaitForSeconds Wait05 = new WaitForSeconds(0.5f);
    public static WaitForSeconds Wait01 = new WaitForSeconds(0.1f);
    public static WaitForFixedUpdate WaitFixed = new WaitForFixedUpdate();
    public static WaitForEndOfFrame WaitFrame = new WaitForEndOfFrame();
    public static bool isCalm = true;
    static FMOD.Studio.System fmodCore;

    public static bool IsXRPresent()
    {
        var xrDisplaySubsystems = new List<XRDisplaySubsystem>();
        SubsystemManager.GetInstances<XRDisplaySubsystem>(xrDisplaySubsystems);
        foreach (var xrDisplay in xrDisplaySubsystems)
        {
            if (xrDisplay.running)
            {
                return true;
            }
        }
        return false;
    }

    private void Start()
    {
        time = 0;
    }

    public static void StartSink()
    {
        isCalm = false;
        SinkRoutine = Sinking();
        instance.StartCoroutine(SinkRoutine);
    }

    public static IEnumerator SinkRoutine;

    public static IEnumerator Sinking()
    {
        FMOD.Studio.System.create(out fmodCore);
        while (true)
        {

            yield return new WaitForSeconds(220);
            Reset();

            Debug.Log("Loading new level");
        }

        yield break;
    }

    public static void Reset()
    {
        SceneManager.LoadScene(0);
        Water.water.muffleInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        Player.Reset();
        Pool.Reset();
    }

    public static float time;
    private void Update()
    {
        time += Time.deltaTime;
    }
}
