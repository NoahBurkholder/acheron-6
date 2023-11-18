using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
public class Lantern : Pickupable
{
    public Light light;
    public MeshRenderer[] renderers;
    public Material[] glassMats;
    private FMOD.Studio.EventInstance gasInstance;
    private FMOD.Studio.EventInstance fizzInstance;
    private FMOD.Studio.EventInstance attachInstance;

    [FMODUnity.EventRef]
    public string audioGas;
    [FMODUnity.EventRef]
    public string audioFizz;
    [FMODUnity.EventRef]
    public string audioAttach;


    public SphereCollider attachPoint;
    public CharacterJoint attachJoint;
    public Hookable hook;
    public Vector3 hookOffset;
    public bool isHooked;
    public bool isOn;

    private void Start()
    {
        gasInstance = FMODUnity.RuntimeManager.CreateInstance(audioGas);
    }
    public override void Update()
    {
        base.Update();
        gasInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(thisTransform));
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!isHooked)
        {
            if (hook = other.GetComponent<Hookable>())
            {
                isHooked = true;
                hook.isHooking = true;

                if (attachJoint == null) attachJoint = gameObject.AddComponent<CharacterJoint>();

                attachJoint.connectedBody = hook.rigidBody;
                attachJoint.autoConfigureConnectedAnchor = false;
                attachJoint.anchor = hookOffset;
                attachJoint.connectedAnchor = hook.hookOffset;

                attachJoint.lowTwistLimit = new SoftJointLimit { limit = -10f };
                attachJoint.highTwistLimit = new SoftJointLimit { limit = 10f };
                attachJoint.swing1Limit = new SoftJointLimit { limit = 40f };
                attachJoint.swing2Limit = new SoftJointLimit { limit = 40f };
                attachJoint.axis = new Vector3(0, 1f, 0);
                attachJoint.breakForce =  400f;
                attachJoint.breakTorque = 1000f;

                attachInstance = FMODUnity.RuntimeManager.CreateInstance(audioAttach);
                attachInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(other.transform));

                attachInstance.start();
                attachInstance.release();

            }
        }
    }
    private void OnJointBreak(float breakForce)
    {
        attachInstance = FMODUnity.RuntimeManager.CreateInstance(audioAttach);
        attachInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(thisTransform));
        attachInstance.start();
        attachInstance.release(); isHooked = false;

        hook.isHooking = false;
        hook = null;
    }


    public override void OnPickup(Hand h)
    {
        base.OnPickup(h);
        if (!isOn)
        {

            gasInstance.start();
            isHooked = false; renderers[0].material = glassMats[1];
            renderers[1].enabled = true;
            light.enabled = true;
            isOn = true;
        }
    }
    public override void OnUnderwater(float velocity)
    {
        base.OnUnderwater(velocity);
        if (isOn)
        {
            gasInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            fizzInstance = FMODUnity.RuntimeManager.CreateInstance(audioFizz);
            fizzInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(thisTransform));
            fizzInstance.start();
            fizzInstance.release(); 
            renderers[0].material = glassMats[0];
            renderers[1].enabled = false;
            light.enabled = false;
            isOn = false;
        }

    }

    public override void OnResurface(float velocity)
    {
        base.OnResurface(velocity);
    }
}
