using System.Collections;
using UnityEngine;

public class ProzeduralAnimationLogic : MonoBehaviour
{
    [SerializeField] private bool showDebugGizmos = true;

    [Header("Movement Animation")]
    [Tooltip("Time in wich the Leg Moves from old to new Position")]
    [SerializeField] private float legMovementTime;
    [Tooltip("Curve in wich the leg Moves up")]
    [SerializeField] private AnimationCurve legMovementCurve;
    [Tooltip("Maximum Range of that the leg can be before moveing to new Position")]
    [SerializeField] private float maxLegRange;

    [Header("Leg Movement Raycasts")]
    [Tooltip("Number of Rays to CHeck the Leg Position")]
    [SerializeField] private int legRayNumber;
    [Tooltip("Degree in wich the Rays are Tilted")]
    [SerializeField] private float legRayTiltDegree;
    [Tooltip("Radius in wich the Leg Rays are distributed around the Origin")]
    [SerializeField] private float legRayPositionradius;
    [Tooltip("Length of Leg Rays")]
    [SerializeField] private float legRaylength;
    [Tooltip("Length wich the Leg Rays can hit")]
    [SerializeField] private LayerMask raycastHitLayers;

    //[Header(" First all Left Legs than all Right Legs --> Front to Back")]
    [Header("Leg Targets and Ray Origins")] //--> First all Left Legs than right Legs
    [Tooltip("IK Targets of the Different Legs --> front to back --> first all left legs than all right legs")]
    [SerializeField] private Transform[] ikTargets;
    [Tooltip("Animation Raycast Targets of the Different Legs --> front to back --> first all left legs than all right legs")]
    [SerializeField] private Transform[] animationRaycastOrigins;

    //The Current Animation Target Position --> always updated
    private Vector3[] currentAnimationTargetPosition;
    //The Next Animation Target Position --> updated when the leg is supposed to be moved
    private Vector3[] nextAnimationTargetPosition;
    //Up Vector to be Set for the Leg IK Target
    private Vector3[] targetUps;
    //Current Range from legs current Position to Calculated Position
    private float ranges;
    //Bool if wich legs are currently moving
    private bool[] moveingLegs;

    //Raycast hit for Legs Raycasts
    private RaycastHit hit;
    //bodynormal to calcluate the up Vector of the Body
    private Vector3 bodyNormal;

    private float deltaDeg;
    private float curDeg;
    private Vector3 curPoint;
    private Vector3 closestPoint;
    private Vector3 tiltedVector;
    private Vector3 rayDir;

    private void Start()
    {
        //Check ik targets and Raycast Origins are the same Length
        if (ikTargets.Length != animationRaycastOrigins.Length)
        {
            Debug.LogError("IK Targets Array and Animation Raycast Origins Array isnt the Same Size");
            Debug.Break();
        }

        //Initialize Arrays with the correct Length
        currentAnimationTargetPosition = new Vector3[ikTargets.Length];
        nextAnimationTargetPosition = new Vector3[ikTargets.Length];
        targetUps = new Vector3[ikTargets.Length];
        moveingLegs = new bool[ikTargets.Length];

        //Set the Initial Leg target Position Data
        for (int i = 0; i < ikTargets.Length; i++)
        {
            nextAnimationTargetPosition[i] = ikTargets[i].position;
        }
    }

    private void Update()
    {
        CalculateTargetPosition();
        CheckRange();
        AdjustBody();
    }

    private void CalculateTargetPosition()
    {
        //Check all Legs
        for (int i = 0; i < animationRaycastOrigins.Length; i++)
        {
            //Set the delta Degree -> degree per point
            deltaDeg = 360f / legRayNumber;
            //Reset Current Degree
            curDeg = 0;

            //Reset CurrentPoint
            curPoint = Vector3.zero;
            //Reset ClosestPoint
            closestPoint = Vector3.zero;

            //Create Ray for each legnum
            for (int j = 0; j < legRayNumber; j++)
            {
                //Set the CurrentPoint with the Rotated right Vector around the up Vector with the current Angle
                curPoint = Quaternion.AngleAxis(curDeg, animationRaycastOrigins[i].up) * animationRaycastOrigins[i].right;
                //Set the leg Ray Radius
                curPoint = curPoint * legRayPositionradius;

                //Tilt the Vector by the Tilt Degree
                tiltedVector = -animationRaycastOrigins[i].up * Mathf.Tan(legRayTiltDegree * Mathf.Deg2Rad) * curPoint.magnitude;
                //Set Ray Direction
                rayDir = (tiltedVector - curPoint).normalized;

                Debug.DrawLine(animationRaycastOrigins[i].position + curPoint, animationRaycastOrigins[i].position + curPoint + rayDir * legRaylength);


                //Check the íf the Ray hit anything
                if (Physics.Raycast(animationRaycastOrigins[i].position + curPoint, rayDir, out hit, legRaylength, raycastHitLayers))
                {
                    if (closestPoint == Vector3.zero)
                    {
                        closestPoint = hit.point;
                        currentAnimationTargetPosition[i] = hit.point;
                        targetUps[i] = hit.normal;
                    }
                    else if ((hit.point - transform.position).sqrMagnitude <= (closestPoint - transform.position).sqrMagnitude)
                    {
                        closestPoint = hit.point;
                        currentAnimationTargetPosition[i] = hit.point;
                        targetUps[i] = hit.normal;
                    }
                }

                curDeg += deltaDeg;
            }
        }
    }

    private void SetNewTargetPosition(int _legtomove)
    {
        ikTargets[_legtomove].up = targetUps[_legtomove];
        nextAnimationTargetPosition[_legtomove] = currentAnimationTargetPosition[_legtomove];

    }

    private void CheckRange()
    {
        for (int i = 0; i < ikTargets.Length; i++)
        {
            ranges = (currentAnimationTargetPosition[i] - nextAnimationTargetPosition[i]).sqrMagnitude;

            if (!moveingLegs[i])
            {
                ikTargets[i].position = nextAnimationTargetPosition[i];

                if (ranges >= maxLegRange * maxLegRange)
                {
                    //Check Edge Cases with at Position 0 or half
                    if (i == 0 || i == moveingLegs.Length / 2)
                    {
                        if (i == 0)
                        {
                            //Check that the Previous Leg or the Leg on the Other Side is not Moving
                            if (moveingLegs[moveingLegs.Length / 2])
                            {
                                ikTargets[i].position = nextAnimationTargetPosition[i];
                                continue;
                            }
                        }
                        else
                        {
                            if (moveingLegs[0])
                            {
                                ikTargets[i].position = nextAnimationTargetPosition[i];
                                continue;
                            }
                        }
                    }
                    else
                    {
                        //Check if Left Side Leg or Right Side Leg
                        if (i < moveingLegs.Length / 2)
                        {
                            //Check that the Previous Leg or the Leg on the Other Side is not Moving
                            if (moveingLegs[i - 1] || moveingLegs[i + moveingLegs.Length / 2 - 1])
                            {
                                ikTargets[i].position = nextAnimationTargetPosition[i];
                                continue;
                            }
                        }
                        else
                        {
                            //Check that the Previous Leg or the Leg on the Other Side is not Moving
                            if (moveingLegs[i - 1] || moveingLegs[i - moveingLegs.Length / 2])
                            {
                                ikTargets[i].position = nextAnimationTargetPosition[i];
                                continue;
                            }
                        }
                    }

                    MoveLeg(i);

                }
            }
        }
    }

    private void MoveLeg(int _leg)
    {
        moveingLegs[_leg] = true;
        SetNewTargetPosition(_leg);
        StartCoroutine(C_MoveLegCoroutine(_leg));
    }

    private IEnumerator C_MoveLegCoroutine(int _leg)
    {
        float passedTime = 0f;

        while (passedTime <= legMovementTime)
        {
            ikTargets[_leg].position = Vector3.Lerp(ikTargets[_leg].position, nextAnimationTargetPosition[_leg], passedTime / legMovementTime) + legMovementCurve.Evaluate(passedTime / legMovementTime) * transform.up;

            passedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        ikTargets[_leg].position = nextAnimationTargetPosition[_leg];
        moveingLegs[_leg] = false;

        //moveing = false;
    }

    private void AdjustBody()
    {
        bodyNormal = Vector3.Cross((nextAnimationTargetPosition[nextAnimationTargetPosition.Length-1] - nextAnimationTargetPosition[0]), (nextAnimationTargetPosition[nextAnimationTargetPosition.Length/2] - nextAnimationTargetPosition[nextAnimationTargetPosition.Length/2-1]));
        bodyNormal.Normalize();

        //Will kick me in the ass
        bodyNormal *= -1;

        this.transform.rotation = Quaternion.LookRotation(transform.forward, Vector3.Lerp(this.transform.up, bodyNormal, 20 * Time.deltaTime));
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        if (!showDebugGizmos) return;


        for (int i = 0; i < currentAnimationTargetPosition.Length; i++)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(currentAnimationTargetPosition[i], 0.1f);

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(ikTargets[i].position, 0.1f);

            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(animationRaycastOrigins[i].position, 0.1f);

            Gizmos.color = Color.black;
            Gizmos.DrawWireSphere(currentAnimationTargetPosition[i], maxLegRange);
        }

        Gizmos.color = Color.magenta;
        Gizmos.DrawRay(this.transform.position, bodyNormal);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(this.transform.position, transform.up);
    }
}
