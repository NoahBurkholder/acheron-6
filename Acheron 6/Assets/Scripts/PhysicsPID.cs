using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsPID : MonoBehaviour
{

    [SerializeField]
    private VectorPID pointController = new VectorPID(0.6f, 0, 0.1f); // Influence hand finger direction.
    [SerializeField]
    private VectorPID palmController = new VectorPID(0.6f, 0, 0.1f); // Influence hand palm direction.

    [SerializeField]
    private VectorPID orientateDamp = new VectorPID(40f, 0, 0.001f); // Dampen orientation of hand.

    [SerializeField]
    private VectorPID followController = new VectorPID(0.6f, 0, 0.001f); // Responsiveness
    [SerializeField]
    private VectorPID followDamp = new VectorPID(0.6f, 0, 0.03f); // Counter-dampening.

    [SerializeField]
    private Hand thisHand;

    [SerializeField]
    private Transform tracking;
    
    [SerializeField]
    private new Rigidbody rigidbody;
    [SerializeField]
    private Collider placementTrigger;
    private bool freeSpace;

    [HideInInspector] public new Transform transform; 

    public void FixedUpdate()
    {
        Follow();
        Orientate();

        freeSpace = true;
        
    }
    Vector3 errorOrientateDamp;
    Vector3 correctionOrientateDamp;
    Vector3 desiredPoint;
    Vector3 currentPoint;
    Vector3 errorPoint;
    Vector3 correctionPoint;

    Vector3 desiredPalm;
    Vector3 currentPalm;
    Vector3 errorPalm;
    Vector3 correctionPalm;

    public Vector3 desiredPosition;
    public Vector3 currentPosition;

    private void Awake()
    {
        transform = gameObject.transform; 
    }

    private void Start()
    {
        rigidbody.maxAngularVelocity = 100;
    }

    private float sustainedError, lastError;

    private void Orientate()
    {
        if (thisHand.isFetching)
            return; 

        desiredPoint = tracking.forward; // Fingers point forward.
        desiredPalm = tracking.up; // Palm points down.

        // Get current orientations to compare with desired orientations.
        currentPoint = transform.forward;
        currentPalm = transform.up;

        // Degrees of error.
        errorPoint = Vector3.Cross(currentPoint, desiredPoint);
        errorPalm = Vector3.Cross(currentPalm, desiredPalm);

        // Tick up timer by errors of both palm and finger.
        sustainedError += errorPoint.magnitude * 0.0045f;
        sustainedError += errorPoint.magnitude * 0.0045f;

        sustainedError -= Time.smoothDeltaTime; // Tick down timer 0.005 per frame.

        // Clamp it.
        sustainedError = Mathf.Clamp(sustainedError, 0, 0.3f);

        // If the error sustains (likely because of a rotational glitch) for more than 0.3 seconds, drop the object.
        if (sustainedError >= 0.3f)
        {
            Hand.Drop(thisHand);
        }

        // Use PID controllers to correct.
        correctionPoint = pointController.Tick(errorPoint, Time.fixedDeltaTime);
        correctionPalm = palmController.Tick(errorPalm, Time.fixedDeltaTime);

        // Add result.
        rigidbody.AddTorque(1000*(correctionPoint + correctionPalm), ForceMode.Acceleration);

        // Get damping error.
        errorOrientateDamp = rigidbody.angularVelocity * -0.001f;

        // Use PID for dampening.
        correctionOrientateDamp = orientateDamp.Tick(errorOrientateDamp, Time.fixedDeltaTime);

    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == (int)Layer.Interactive)
        {
            freeSpace = false;
        }

    }
    private float timer = 2f;

    Vector3 positionError;
    Vector3 positionCorrection;
    Vector3 positionDamping;
    private void Follow()
    {
        if (!thisHand.isFetching)
        {
            if ((tracking.position - Player.PlayerTransform.position).magnitude < 2f)
            {
                desiredPosition = tracking.position;
            }
        }
        currentPosition = transform.position;
        if ((desiredPosition - currentPosition).magnitude > 0.1f)
        {
            if (freeSpace)
            {
                if (timer <= 0)
                {
                    transform.position = desiredPosition;
                    timer = 2f;
                } else
                {
                    timer -= Time.fixedDeltaTime;
                }
            }
        }
        positionError = desiredPosition - currentPosition;
        positionCorrection = followController.Tick(positionError, Time.fixedDeltaTime);
        positionDamping = followDamp.Tick(positionError, Time.fixedDeltaTime);

        rigidbody.AddForce(1000 * (positionCorrection + positionDamping), ForceMode.Acceleration);

    }

    public void Reset()
    {
        timer = 2;
        Vector3 trackingPosition = tracking.position; 
        desiredPosition = trackingPosition;
        transform.position = trackingPosition;
        transform.rotation = tracking.rotation; 
        freeSpace = true; 
        pointController.Reset();
        palmController.Reset();
        orientateDamp.Reset();
        followController.Reset();
        followDamp.Reset();
    }
}