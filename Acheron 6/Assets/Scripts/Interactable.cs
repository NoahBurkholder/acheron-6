/// Bird by Example baseline interactable object. 
/// A GameObject which birds react neurologically to (based on what it 'evokes').
/// These evokes afford birds to transfer skills and behaviours based on shared attributes on disparate objects.
/// Birds can only focus on one Interactable type object at a time. Birds are also considered an Interactable.
/// Also holds information about its physical properties.
/// Noah James Burkholder 2020

using System.Collections;
using UnityEngine;
using FMODUnity;
/// <summary>
/// The baseline type of gameplay object.
/// All interactables 'evoke' concepts, which let birds transfer skills and behaviour between objects.
/// </summary>
public class Interactable : MonoBehaviour
{
    [Header("General")]
    [HideInInspector] protected bool isAlive = true;
    public Transform thisTransform;
    public bool isCarried;
    public bool isUnderwater;
    public bool isBuoyant;
    public float buoyancyForce;
    public Vector3 buoyancyOffset;
    private Vector3 relativeOffset;
    [Header("Physics")]
    public Rigidbody rigidBody;

    private float initialDrag;
    private float initialAngularDrag;
    


    // For playing audio.
    [Header("Audio")]
    private FMOD.Studio.EventInstance collisionInstance;

    [FMODUnity.EventRef]
    public string audioCollision;
    [FMODUnity.EventRef]
    public string audioDrag;

    [Header("Debug")]
    public bool isReporting;


    // Some attributes of an instanced interactable.
    public bool canBePickedUp = false;

    // Used for giving birds dynamic nicknames based on the types of Interactables they interact most with.

    private void Awake()
    {
        thisTransform = this.transform;
        if (rigidBody == null) rigidBody = gameObject.GetComponent<Rigidbody>();
        isAlive = true;
        initialDrag = rigidBody.drag;
        initialAngularDrag = rigidBody.angularDrag;
    }
    public virtual void Update()
    {
        relativeOffset = thisTransform.position + (thisTransform.localToWorldMatrix.MultiplyVector(buoyancyOffset));
        Debug.DrawRay(relativeOffset, Vector3.up, Color.red);

        if (relativeOffset.y < Water.water.transform.position.y)
        {
            if (isBuoyant)
            {
                rigidBody.AddForceAtPosition((Vector3.up * (buoyancyForce * Mathf.Clamp01((Water.water.transform.position.y - relativeOffset.y) * 10f))), relativeOffset);
            }
        }

        if (transform.position.y < Water.water.transform.position.y)
        {


            if (!isUnderwater)
            {
                rigidBody.drag = 2f;
                rigidBody.angularDrag = 10f;
                OnUnderwater(rigidBody.velocity.magnitude);
            }
        }
        else
        {
            if (isUnderwater)
            {
                rigidBody.drag = initialDrag;
                rigidBody.angularDrag = initialAngularDrag;
                OnResurface(rigidBody.velocity.magnitude);
            }
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        float volume = Mathf.Clamp(collision.relativeVelocity.magnitude / 5, 0f, 1f);
        float pitch = volume;
        if (collision.relativeVelocity.magnitude > 0.0001f) {
            if ((audioCollision != null) && (GameManager.time > 1f))
            {
                collisionInstance = FMODUnity.RuntimeManager.CreateInstance(audioCollision);
                collisionInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(collision.contacts[0].point));
                collisionInstance.setParameterByName("Pitch", pitch);
                collisionInstance.setParameterByName("Volume", volume);
                if (isReporting) Debug.Log("Vol = " + volume);

                
                collisionInstance.start();
                collisionInstance.release();
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        /*float volume = Mathf.Clamp(collision.relativeVelocity.magnitude / 1000, 0f, 1f);
        float pitch = volume;
        if (volume > 0.05f)
        {
            if ((audioDrag != null) && (GameManager.time > 1f))
            {
                float intensity = Mathf.Clamp(collision.impulse.magnitude / 1000, 0f, 1f);
                if (!audioDrag.IsPlaying())
                {
                    audioInstance.setParameterByName("Pitch", pitch);
                    audioInstance.setParameterByName("Volume", volume);

                    //audioInstance.start();
                    //audioInstance.release();
                }
            }
        }*/

    }
    private void OnCollisionExit(Collision collision)
    {
        /*if (audioInstance != null)
        {
            if (audioInstance.())
            {
                audioInstance.release();
            }
        }*/
    }


    public virtual void Hover(Hand h)
    {

    }

    public virtual void Unhover(Hand h)
    {

    }

    public virtual void OnReset()
    {
        // TODO: Populate this.
    }
    public virtual void OnUnderwater(float velocity)
    {
        isUnderwater = true;
        if (velocity < 0.05f)
        {
            Water.water.splashInstance = FMODUnity.RuntimeManager.CreateInstance(Water.water.audioSplash);


            if (velocity < 0.3f)
            {
                Water.water.splashInstance.setParameterByName("Switch", 0);
            }
            else
            {
                Water.water.splashInstance.setParameterByName("Switch", 2);
            }


            Water.water.splashInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(thisTransform));
            Water.water.splashInstance.start();
            Water.water.splashInstance.release();
        }

    }

    public virtual void OnResurface(float velocity)
    {
        isUnderwater = false;
    }
}

