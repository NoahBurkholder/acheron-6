using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;
using UnityEngine.XR;
public enum Layer
{
    Environment = 8,
    Interactive = 9,
    Hands = 10,
    Obscura = 11
}

public class Hand : MonoBehaviour
{

    [SerializeField]
    public Rigidbody rigidBody;
    public static Hand RightHand;
    public static Hand LeftHand;
    public InputDevice Device;

    public bool isLeftHand;

    public FixedJoint fixedGrab;
    public Pickupable stickied; // Object in palm, but not grabbed.
    public Pickupable hovered; // Object grabbed in palm.
    public Pressable pressed;

    public bool isFetching = false;
    public bool isBlocked = false;
    public bool isSticky = false;
    public bool isUnderwater;

    [SerializeField]
    public Material missMaterial;
    [SerializeField]
    public Material hitMaterial;
    [SerializeField]
    private Material blockMaterial;
    public SkinnedMeshRenderer handRenderer;
    public Animator handAnimator;

    private static readonly int POSE_HASH = Animator.StringToHash("IsPosing");
    private static readonly int FIST_HASH = Animator.StringToHash("IsFist");
    private float gripAxis; // 0-1 float for animation blend. Relaxed vs. posing.
    private float triggerAxis; // 0-1 float for animation blend. If posing, Point vs. Fist.
    private readonly float GRIP_THRESHOLD = 0.1f; // Weight of the trigger press.


    [SerializeField]
    private bool inTrigger;
    [SerializeField]
    public Rigidbody handRB;
    private int floorMask = 1 << (int)Layer.Environment;
    private int objectMask = 1 << (int)Layer.Interactive;

    private void Start()
    {
        SetupHands();
        //StartCoroutine(HandAnimation());
    }

    private void Update()
    {
        Device.TryGetFeatureValue(CommonUsages.trigger, out triggerAxis);
        Device.TryGetFeatureValue(CommonUsages.grip, out gripAxis);

        isSticky = (gripAxis > GRIP_THRESHOLD);

        CheckUnderwater();

        CheckGrab();
        CheckDrop();

        handAnimator.SetFloat(POSE_HASH, gripAxis);
        handAnimator.SetFloat(FIST_HASH, triggerAxis);
    }

    public void CheckGrab()
    {

        if (isSticky && (hovered != null) && (stickied == null))
        {
            stickied = hovered;
            stickied.OnPickup(this);
            if (fixedGrab == null) fixedGrab = gameObject.AddComponent<FixedJoint>();

            fixedGrab.connectedBody = stickied.rigidBody;
            fixedGrab.enableCollision = false;

            fixedGrab.breakForce = isLeftHand ? 8000f : 9000f;
            fixedGrab.breakTorque = 500f;
        }
    }

    public void CheckDrop()
    {
        if (!isSticky && (stickied != null))
        {
            stickied.OnDrop();
            stickied.rigidBody.angularVelocity = rigidBody.angularVelocity;
            stickied = null;
            if (fixedGrab != null) Destroy(fixedGrab);
        }
    }

    void OnTriggerEnter(Collider collide)
    {
        // Check for blocking volumes.
        if (collide.gameObject.layer == (int)Layer.Obscura)
        {
            isBlocked = true;
            handRenderer.material = blockMaterial;

        }
        // Check for blocking volumes.
        if (collide.gameObject.layer == (int)Layer.Interactive)
        {
            if (pressed == null)
            {
                if (pressed = collide.GetComponent<Pressable>())
                {
                    //interactiveProp.StartedLookingAt(emptyHit);
                    pressed.Press();
                    handRenderer.material = hitMaterial;

                    /*if (fistAmount == 0 && poseAmount > 0.85f && (interactiveProp is Station.Interaction.AR.ButtonInteractive))
                    {
                        interactiveProp.LeftClickDown(EMPTY_HIT);
                    }*/
                }
            }

        }

        // If not blocked...
        if (!isBlocked)
        {
            if (hovered == null)
            {
                if (hovered = collide.GetComponent<Pickupable>())
                {
                    //interactiveProp.StartedLookingAt(emptyHit);
                    hovered.Hover(this);
                    handRenderer.material = hitMaterial;

                    /*if (fistAmount == 0 && poseAmount > 0.85f && (interactiveProp is Station.Interaction.AR.ButtonInteractive))
                    {
                        interactiveProp.LeftClickDown(EMPTY_HIT);
                    }*/
                }
            }
        }
    }

    void OnTriggerExit(Collider collide)
    {
        if (collide.gameObject.layer == (int)Layer.Obscura)
        {
            isBlocked = false;
            handRenderer.material = missMaterial;
        }
        if (collide.GetComponent<Pressable>())
        {
            if (pressed != null)
            {
                //interactiveProp.StoppedLookingAt(emptyHit);
                handRenderer.material = missMaterial;

                if (collide.GetComponent<Pressable>() == pressed)
                {
                    pressed = null;
                }
            }
        }
        if (collide.GetComponent<Interactable>())
        {
            if (hovered != null)
            {
                //interactiveProp.StoppedLookingAt(emptyHit);
                hovered.Unhover(this);
                handRenderer.material = missMaterial;

                if (collide.GetComponent<Interactable>() == hovered)
                {
                    hovered = null;
                }
            }
            //UnblockCollisions(collide); 
        }

        if (collide.gameObject.layer == (int)Layer.Interactive)
            inTrigger = false;

    }


    void OnTriggerStay(Collider collide)
    {
        if (!isBlocked)
        {
            if (collide.gameObject.layer == (int)Layer.Interactive)
            {
                inTrigger = true;
            }
        }

    }

    private IEnumerator HandAnimation()
    {
        while (true)
        {
            /*triggerWeight = keepHandOpen ? 0 : OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRController); // Get weight of trigger press.
            handWeight = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRController);
            poseAmount = triggerWeight > handWeight ? triggerWeight : handWeight;
            fistAmount = triggerWeight;*/

            yield return GameManager.WaitFrame;
        }
        yield break;
    }
    public static void Drop(Hand specificHand)
    {

    }

    public void SetMissMaterial()
    {
        handRenderer.material = missMaterial;
    }

    public void SetHitMaterial()
    {
        handRenderer.material = hitMaterial;
    }

    private void SetupHands()
    {
        if (isLeftHand)
        {
            Device = Player.LeftController;
            LeftHand = this;
        }
        else
        {
            Device = Player.RightController;

            RightHand = this;
        }
    }
    private void CheckUnderwater()
    {
        if (transform.position.y < Water.water.transform.position.y)
        {
            if (!isUnderwater)
            {
                isUnderwater = true;
                Water.water.splashInstance = FMODUnity.RuntimeManager.CreateInstance(Water.water.audioSplash);

                Water.water.splashInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform));
                Water.water.splashInstance.start();
                Water.water.splashInstance.release();
            }
        }
        else
        {
            if (isUnderwater)
            {
                isUnderwater = false;
            }
        }
    }
}
