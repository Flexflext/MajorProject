using System.Collections;
using UnityEngine;

public class ProzeduralAnimationLogic : MonoBehaviour
{
    [SerializeField] private bool showDebugGizmos = true;
    [SerializeField] private float added;

    [Header("Movement Animation")]
    [Tooltip("Time in wich the Leg Moves from old to new Position")]
    [SerializeField] private float legMovementTime;
    [Tooltip("Curve in wich the leg Moves up")]
    [SerializeField] private AnimationCurve legMovementCurve;
    [Tooltip("Maximum Range of that the leg can be before moveing to new Position")]
    [SerializeField] private float maxLegRange;

    [Header("Additional Leg Position Flags")]
    [Tooltip("Use the Closeset Possible or the Farthest Posssible Position")]
    [SerializeField] private bool useFarthestPoint;
    [SerializeField] private bool additionalLegRangeCheck;
    [Tooltip("Max Length that the new Position can be from the Body")]
    [SerializeField] private float maxLegLength;
    [SerializeField] private bool adjustBodyRotation;
    [SerializeField] private bool adjustLastLimbToNormal;
    [SerializeField] private bool additionalLegCollisionCheck;


    [Header("Body Animation")]
    [SerializeField] private bool animateBody = true;
    [SerializeField] private BodyAnimation xAnimation;
    [SerializeField] private BodyAnimation yAnimation;
    [SerializeField] private BodyAnimation zAnimation;


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

    //Bool if leg is On Move Delay -> Not only one leg can move again and again
    private bool[] isOnMoveDelay;

    //Raycast hit for Legs Raycasts
    private RaycastHit hit;
    //Raycast hit for Second Check Leg Raycasts
    private RaycastHit secondhit;
    //bodynormal to calcluate the up Vector of the Body
    private Vector3 bodyNormal;

    private float deltaDeg;
    private float curDeg;
    private Vector3 curPoint;
    private Vector3 closestPoint;
    private Vector3 tiltedVector;
    private Vector3 rayDir;
    private bool first;

    private float curLength;

    private Vector3 startLocalPosition;

    [System.Serializable]
    struct AnimParam
    {
        public bool useAnimation;
        public float heightMultiplier;
        public float timeMultiplier;
    }

    [System.Serializable]
    struct AnimCurve
    {
        public AnimationCurve positionCurve;
        [Min(0)]
        public int frequency;
        public float amplitude;
        public float seed;
        public float randomScale;
    }

    [System.Serializable]
    struct BodyAnimation
    {
        public AnimParam AnimationParameter;
        public AnimCurve AnimationCurveSettings;
    }

    private void Start()
    {
        startLocalPosition = transform.localPosition;

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
        isOnMoveDelay = new bool[ikTargets.Length];

        //Set the Initial Leg target Position Data
        for (int i = 0; i < ikTargets.Length; i++)
        {
            nextAnimationTargetPosition[i] = ikTargets[i].position;
        }
    }

    private void OnValidate()
    {
        if (animateBody)
        {
            xAnimation.AnimationCurveSettings.positionCurve = GenerateAnimationCurve(xAnimation.AnimationCurveSettings.frequency, xAnimation.AnimationCurveSettings.amplitude, xAnimation.AnimationCurveSettings.seed, xAnimation.AnimationCurveSettings.randomScale);
            yAnimation.AnimationCurveSettings.positionCurve = GenerateAnimationCurve(yAnimation.AnimationCurveSettings.frequency, yAnimation.AnimationCurveSettings.amplitude, yAnimation.AnimationCurveSettings.seed, yAnimation.AnimationCurveSettings.randomScale);
            zAnimation.AnimationCurveSettings.positionCurve = GenerateAnimationCurve(zAnimation.AnimationCurveSettings.frequency, zAnimation.AnimationCurveSettings.amplitude, zAnimation.AnimationCurveSettings.seed, zAnimation.AnimationCurveSettings.randomScale);
        }
    }

    private void Update()
    {
        CalculateTargetPosition();
        CheckRange();
        if (adjustBodyRotation) AdjustBody();
        if (animateBody) AnimateBody();

    }

    /// <summary>
    /// Calculate the the currentAnimationTargetPositions each Frame for all Legs
    /// </summary>
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
            first = true;


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

                //hit = new RaycastHit();

                //Check the íf the Ray hit anything
                if (Physics.Raycast(animationRaycastOrigins[i].position + curPoint, rayDir, out hit, legRaylength, raycastHitLayers))
                {
                    curLength = (hit.point - transform.root.position).sqrMagnitude;

                    //Check what the closest Point is + set the first Ray as the first Closest Point
                    if (first)
                    {
                        //Set ClosestPoint
                        SetClosestPoint(hit, i);
                    }
                    //Check if Closer than the Current Closest Point
                    else if (curLength >= (closestPoint - transform.root.position).sqrMagnitude && useFarthestPoint || (hit.point - transform.root.position).sqrMagnitude <= curLength && !useFarthestPoint)
                    {
                        if (!additionalLegCollisionCheck || (additionalLegCollisionCheck && Physics.Raycast(transform.root.position, hit.point - transform.root.position, out secondhit, float.MaxValue, raycastHitLayers)))
                        {
                            if (additionalLegCollisionCheck && secondhit.point != hit.point)
                            {
                                continue;
                            }

                            //Set ClosestPoint
                            SetClosestPoint(hit, i);
                        }
                    }
                }

                //Add Degree for Next Ray
                curDeg += deltaDeg;
            }
        }
    }

    private void SetClosestPoint(RaycastHit _hit, int _idx)
    {
        if (!additionalLegRangeCheck || (additionalLegRangeCheck && (curLength <= maxLegLength * maxLegRange)))
        {
            closestPoint = _hit.point;
            currentAnimationTargetPosition[_idx] = _hit.point;
            targetUps[_idx] = _hit.normal;
            first = false;
        }   
    }

    /// <summary>
    /// Set the NextAnimationTargetPosition to the currentAnimationTargetPosition for a given Leg
    /// </summary>
    /// <param name="_legtomove"></param>
    private void SetNewTargetPosition(int _legtomove)
    {
        //Set the Up Vector
        if (adjustLastLimbToNormal) ikTargets[_legtomove].up = targetUps[_legtomove];
        //Set the new AnimationTarget Position
        nextAnimationTargetPosition[_legtomove] = currentAnimationTargetPosition[_legtomove];

    }

    /// <summary>
    /// Check the Ranges of Each Leg --> And Invokes Move for that Leg
    /// </summary>
    private void CheckRange()
    {
        for (int i = 0; i < ikTargets.Length; i++)
        {
            //Check if the Current Leg isnt already Moving
            if (!moveingLegs[i])
            {
                //Calculate the Squared Lenght from the Old Animation Target to the current Animation Target
                ranges = (currentAnimationTargetPosition[i] - nextAnimationTargetPosition[i]).sqrMagnitude;

                //Reset the Target Position
                ikTargets[i].position = nextAnimationTargetPosition[i];

                if (isOnMoveDelay[i])
                {
                    continue;
                }

                //Check if the Calculated Range is greater than the maxLegRange
                if (ranges >= maxLegRange * maxLegRange)
                {
                    //Check Edge Cases with at Position 0 or half
                    if (i == 0 || i == moveingLegs.Length / 2)
                    {
                        //Check the First Leg (Front Left) (Edge Case)
                        if (i == 0)
                        {
                            //Check that the Previous Leg or the Leg on the Other Side is not Moving
                            if (moveingLegs[moveingLegs.Length / 2])
                            {
                                //ikTargets[i].position = nextAnimationTargetPosition[i];
                                continue;
                            }
                        }
                        //Check the Leg on the Other Side of the First Leg (Front Right)
                        else
                        {
                            if (moveingLegs[0])
                            {
                                //ikTargets[i].position = nextAnimationTargetPosition[i];
                                continue;
                            }
                        }
                    }
                    else
                    {
                        //Check if Left Side Leg or Right Side Leg //-> Left Side
                        if (i < moveingLegs.Length / 2)
                        {
                            //Check that the Previous Leg or the Leg on the Other Side is not Moving
                            if (moveingLegs[i - 1] || moveingLegs[i + moveingLegs.Length / 2 - 1])
                            {
                                //ikTargets[i].position = nextAnimationTargetPosition[i];
                                continue;
                            }
                        }
                        else //-> Right Side
                        {
                            //Check that the Previous Leg or the Leg on the Other Side is not Moving
                            if (moveingLegs[i - 1] || moveingLegs[i - moveingLegs.Length / 2])
                            {
                                //ikTargets[i].position = nextAnimationTargetPosition[i];
                                continue;
                            }
                        }
                    }


                    //Move the Leg at the Index
                    MoveLeg(i);
                }
            }
        }
    }

    /// <summary>
    /// Move the Leg at the given Index
    /// </summary>
    /// <param name="_leg"></param>
    private void MoveLeg(int _leg)
    {
        //Set Flag
        moveingLegs[_leg] = true;
        isOnMoveDelay[_leg] = true;
        //Set the new Target Position
        SetNewTargetPosition(_leg);
        //Start the Move Coroutine
        StartCoroutine(C_MoveLegCoroutine(_leg));
    }

    /// <summary>
    /// Coroutine to Move a given Leg to its new Position
    /// </summary>
    /// <param name="_leg"></param>
    /// <returns></returns>
    private IEnumerator C_MoveLegCoroutine(int _leg)
    {
        float passedTime = 0f;

        //Move the Leg for the Given Time
        while (passedTime <= legMovementTime)
        {
            //Lerp the Target Position and add the Evaluated Curve to it
            ikTargets[_leg].position = Vector3.Lerp(ikTargets[_leg].position, nextAnimationTargetPosition[_leg], passedTime / legMovementTime) + legMovementCurve.Evaluate(passedTime / legMovementTime) * transform.up;

            //Add deltaTime and Wait for next Frame
            passedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        //Set Position
        ikTargets[_leg].position = nextAnimationTargetPosition[_leg];

        //Reset Flag
        moveingLegs[_leg] = false;

        yield return new WaitForSeconds(legMovementTime / 2);

        isOnMoveDelay[_leg] = false;
    }

    /// <summary>
    /// Adjust the Body Rotation amd Position
    /// </summary>
    private void AdjustBody()
    {
        //Calculate the Cross Vector from the Outermost Legs
        bodyNormal = Vector3.Cross((ikTargets[nextAnimationTargetPosition.Length-1].position - ikTargets[0].position), (ikTargets[nextAnimationTargetPosition.Length/2].position - ikTargets[nextAnimationTargetPosition.Length/2-1].position));
        //Normalize the Vector
        bodyNormal.Normalize();


        //Check that the Calculated Vector is pointing in the same up Direction
        if (Vector3.Dot(bodyNormal, transform.parent.up) < 0)
        {
            bodyNormal *= -1;
        }

        bodyNormal = Vector3.Lerp(this.transform.up, bodyNormal, 20 * Time.deltaTime);
        

        //Set the Rotation
        this.transform.rotation = Quaternion.LookRotation(transform.forward, bodyNormal);
    }

    private void AnimateBody()
    {
        Vector3 add = new Vector3();

        if (xAnimation.AnimationParameter.useAnimation)
        {
            add.x = xAnimation.AnimationCurveSettings.positionCurve.Evaluate(Time.time / xAnimation.AnimationParameter.timeMultiplier) * xAnimation.AnimationParameter.heightMultiplier;
        }

        if (yAnimation.AnimationParameter.useAnimation)
        {
            add.y = yAnimation.AnimationCurveSettings.positionCurve.Evaluate(Time.time / yAnimation.AnimationParameter.timeMultiplier) * yAnimation.AnimationParameter.heightMultiplier;
        }

        if (zAnimation.AnimationParameter.useAnimation)
        {
            add.z = zAnimation.AnimationCurveSettings.positionCurve.Evaluate(Time.time / zAnimation.AnimationParameter.timeMultiplier) * zAnimation.AnimationParameter.heightMultiplier;
        }

        Vector3 newLocalPosition = Vector3.zero;

        newLocalPosition += add.x * Vector3.right; // startLocalPosition;
        newLocalPosition += add.y * Vector3.up; // startLocalPosition;
        newLocalPosition += add.z * Vector3.forward; // startLocalPosition;



        transform.localPosition = newLocalPosition + startLocalPosition;
    }

    private AnimationCurve GenerateAnimationCurve(int _frequency, float _amplitude, float _seed, float _randomscale)
    {
        AnimationCurve curve = new AnimationCurve();
        curve.preWrapMode = WrapMode.PingPong;
        curve.postWrapMode = WrapMode.PingPong;

        int keys = _frequency;
        float multiplier = 1;

        float delta = (1.0f / keys);

        for (int i = 0; i < keys + 1; i++)
        {
            Keyframe keyframe = new Keyframe((delta * i) - delta, multiplier*(_amplitude + Mathf.PerlinNoise(_seed, i) * (_randomscale * multiplier)/*(Random.Range(_minrandom, _maxrandom))*/));

            curve.AddKey(keyframe);

            multiplier *= -1;
        }

        return curve;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        if (!showDebugGizmos) return;


        for (int i = 0; i < currentAnimationTargetPosition.Length; i++)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(currentAnimationTargetPosition[i], 0.1f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(animationRaycastOrigins[i].position, 0.1f);

            Gizmos.color = Color.black;
            Gizmos.DrawWireSphere(currentAnimationTargetPosition[i], maxLegRange);
        }

        for (int i = 0; i < animationRaycastOrigins.Length; i++)
        {
            //Set the delta Degree -> degree per point
            deltaDeg = 360f / legRayNumber;
            //Reset Current Degree
            curDeg = 0;

            //Reset CurrentPoint
            curPoint = Vector3.zero;

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

                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(animationRaycastOrigins[i].position + curPoint, animationRaycastOrigins[i].position + curPoint + rayDir * legRaylength);

                curDeg += deltaDeg;
            }
        }

        Gizmos.color = Color.magenta;
        Gizmos.DrawRay(this.transform.position, bodyNormal);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(this.transform.position, transform.up);
    }
}
