using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ELegStates
{
    LS_Normal,
    LS_Limping,
    LS_LimpingHalfLeg,
    LS_Broken,
}

public class ProzeduralAnimationLogic : MonoBehaviour, IStateMachineController
{
    [SerializeField] private bool showDebugGizmos = true;

    [Header("Movement Animation")]
    [Tooltip("Time in wich the Leg Moves from old to new Position")]
    [SerializeField] private float legMovementTime;
    public float LegMovementTime { get { return legMovementTime; } }
    [Tooltip("Curve in wich the leg Moves up")]
    [SerializeField] private AnimationCurve legMovementCurve;
    public AnimationCurve LegMovementCurve { get { return legMovementCurve; } }
    [Tooltip("Maximum Range of that the leg can be before moveing to new Position")]
    [SerializeField] private float maxLegRange;

    [Header("Additional Leg Position Flags")]
    [Tooltip("Use the Closeset Possible or the Farthest Posssible Position")]
    [SerializeField] private bool useFarthestPoint;
    [SerializeField] private bool additionalLegRangeCheck;
    [SerializeField] private bool adjustBodyRotation;
    [SerializeField] private bool adjustLastLimbToNormal;
    [SerializeField] private bool additionalLegCollisionCheck;


    [Header("Body Animation")]
    [SerializeField] private bool animateBody = true;
    [SerializeField] private BodyAnimation xAnimation;
    [SerializeField] private BodyAnimation yAnimation;
    [SerializeField] private BodyAnimation zAnimation;

    [Header("ExtraLegAnimation")]
    [SerializeField] private bool[] stopLegAnimationFlags;
    [SerializeField] private ELegStates[] legState;
    [SerializeField] private float bodySmoothing = 8;
    [SerializeField] private float hightAddMultiplier = 0.35f;
    [SerializeField] private float originBackwardsMultiplier = 0.35f;
    [SerializeField] private Vector3 originLocalBack = Vector3.left;
    [SerializeField] private float hintBackwardsMultiplier = 0.35f;
    [SerializeField] private float downAddPerBrokenLeg = 0.1f;
    [SerializeField] private float percentOfLegHeightMovement = 0.1f;
    public float PercentOfLegHeightMovement { get { return percentOfLegHeightMovement; } }


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


    [Header("Leg Targets and Ray Origins")] //--> First all Left Legs than right Legs
    [Tooltip("IK Systems of the Different Legs --> front to back --> first all left legs than all right legs")]
    [SerializeField] private IKSystem[] legIKSystems;
    [Tooltip("Animation Raycast Targets of the Different Legs --> front to back --> first all left legs than all right legs")]
    [SerializeField] private Transform[] animationRaycastOrigins;

    private Transform[] ikTargets;
    public Transform[] IkTargets { get { return ikTargets; } set { ikTargets = value; } }
    private Transform[] animationHints;

    private Vector3[] hintLocalStartPosition;
    public Vector3[] HintLocalStartPosition { get { return hintLocalStartPosition; } set { hintLocalStartPosition = value; } }
    private Vector3[] originLocalStartPosition;
    public Vector3[] OriginLocalStartPosition { get { return originLocalStartPosition; } set { originLocalStartPosition = value; } }

    //The Current Animation Target Position --> always updated
    private Vector3[] currentAnimationTargetPosition;
    //The Next Animation Target Position --> updated when the leg is supposed to be moved
    private Vector3[] nextAnimationTargetPosition;
    public Vector3[] NextAnimationTargetPosition { get { return nextAnimationTargetPosition; } set { nextAnimationTargetPosition = value; } }
    //Up Vector to be Set for the Leg IK Target
    private Vector3[] targetUps;
    //Current Range from legs current Position to Calculated Position
    private float ranges;
    //Bool if wich legs are currently moving
    private bool[] moveingLegs;

    public bool[] MoveingLegs { get { return moveingLegs; } set { moveingLegs = value; } }

    //Bool if leg is On Move Delay -> Not only one leg can move again and again
    private bool[] isOnMoveDelay;

    public bool[] IsOnMoveDelay { get { return isOnMoveDelay; } set { isOnMoveDelay = value; } }

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
    public Vector3 StartLocalPosition { get { return startLocalPosition; } set { startLocalPosition = value; } }

    private Quaternion toRot = Quaternion.identity;
    private Quaternion startRot;
    private Quaternion newRot = Quaternion.identity;

    private ELegStates currentLegStateEnum;
    private LegState[] currentLegState;

    public Dictionary<StateMachineSwitchDelegate, LegState> stateDictionary { get; set; }

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
        startRot = transform.localRotation;
        newRot = startRot;

        //Check ik targets and Raycast Origins are the same Length
        if (legIKSystems.Length != animationRaycastOrigins.Length)
        {
            Debug.LogError("IK Targets Array and Animation Raycast Origins Array isnt the Same Size");
            Debug.Break();
        }

        //Initialize Arrays with the correct Length
        currentAnimationTargetPosition = new Vector3[legIKSystems.Length];
        nextAnimationTargetPosition = new Vector3[legIKSystems.Length];
        hintLocalStartPosition = new Vector3[legIKSystems.Length];
        originLocalStartPosition = new Vector3[legIKSystems.Length];
        legState = new ELegStates[legIKSystems.Length];
        targetUps = new Vector3[legIKSystems.Length];
        moveingLegs = new bool[legIKSystems.Length];
        isOnMoveDelay = new bool[legIKSystems.Length];
        ikTargets = new Transform[legIKSystems.Length];
        animationHints = new Transform[legIKSystems.Length];
        currentLegState = new LegState[legIKSystems.Length];
        stopLegAnimationFlags = new bool[legIKSystems.Length];

        //Set the Initial Leg target Position Data
        for (int i = 0; i < legIKSystems.Length; i++)
        {
            ikTargets[i] = legIKSystems[i].GetTarget();
            animationHints[i] = legIKSystems[i].GetHint();

            nextAnimationTargetPosition[i] = ikTargets[i].position;
            hintLocalStartPosition[i] = animationHints[i].localPosition;
            originLocalStartPosition[i] = animationRaycastOrigins[i].localPosition;
        }

        CreateStateDictionary();
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
        AnimateBody();


        this.transform.localRotation = Quaternion.Lerp(transform.localRotation, newRot , bodySmoothing * Time.deltaTime);
    }

    private void CreateStateDictionary()
    {
        LegNormalState legNormalState = new LegNormalState(this);
        LegLimpingState legLimpingState = new LegLimpingState(this);

        stateDictionary = new Dictionary<StateMachineSwitchDelegate, LegState>()
        {
            {
                () => (currentLegStateEnum == ELegStates.LS_Normal),
                legNormalState
            },
            {
                () => (currentLegStateEnum == ELegStates.LS_Limping),
                legLimpingState
            },
        };


        for (int i = 0; i < currentLegState.Length; i++)
        {
            currentLegState[i] = legNormalState;
        }
    }

    /// <summary>
    /// Calculate the the currentAnimationTargetPositions each Frame for all Legs
    /// </summary>
    private void CalculateTargetPosition()
    {
        //Check all Legs
        for (int i = 0; i < animationRaycastOrigins.Length; i++)
        {
            if (stopLegAnimationFlags[i]) continue;

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
        if (!additionalLegRangeCheck || (additionalLegRangeCheck && (curLength <= legIKSystems[_idx].GetMaxRangeOfChain() * legIKSystems[_idx].GetMaxRangeOfChain())))
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
                if (stopLegAnimationFlags[i]) continue;
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
                                continue;
                            }
                        }
                        //Check the Leg on the Other Side of the First Leg (Front Right)
                        else
                        {
                            if (moveingLegs[0])
                            {
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
                                continue;
                            }
                        }
                        else //-> Right Side
                        {
                            //Check that the Previous Leg or the Leg on the Other Side is not Moving
                            if (moveingLegs[i - 1] || moveingLegs[i - moveingLegs.Length / 2])
                            {
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

        //currentLegStateEnum = legState[_leg];

        //foreach (var state in stateDictionary)
        //{
        //    if (state.Key())
        //    {
        //        currentLegState[_leg].ExitLegState(_leg);
        //        currentLegState[_leg] = state.Value;
        //        currentLegState[_leg].EnterLegState(_leg);
        //    }
        //}

        //Start the Move Coroutine
        StartCoroutine(currentLegState[_leg].C_MoveLegCoroutine(_leg));
    }

    /// <summary>
    /// Adjust the Body Rotation amd Position
    /// </summary>
    private void AdjustBody()
    {
        //Calculate the Cross Vector from the Outermost Legs
        bodyNormal = Vector3.Cross((ikTargets[nextAnimationTargetPosition.Length - 1].position - ikTargets[0].position), (ikTargets[nextAnimationTargetPosition.Length / 2].position - ikTargets[nextAnimationTargetPosition.Length / 2 - 1].position));
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

        if (animateBody)
        {
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
        }

        

        Vector3 newLocalPosition = Vector3.zero;

        newLocalPosition += add.x * Vector3.right; // startLocalPosition;
        newLocalPosition += add.y * Vector3.up; // startLocalPosition;
        newLocalPosition += add.z * Vector3.forward; // startLocalPosition;



        transform.localPosition = newLocalPosition + startLocalPosition;
    }

    public void AdjustBrokenLeg(int _leg)
    {
        Plane plane = new Plane(transform.up, transform.position);

        Vector3 pos = plane.ClosestPointOnPlane(ikTargets[_leg].position);

        toRot = Quaternion.FromToRotation((transform.InverseTransformPoint(pos) - transform.localPosition), (transform.InverseTransformPoint((ikTargets[_leg].position + transform.up * hightAddMultiplier)) - transform.localPosition));

        newRot = transform.localRotation * Quaternion.Inverse(toRot);
    }

    public void ResetBrokenLegRotation()
    {
        newRot = startRot;
    }

    public void SetLegLimp(int _leg)
    {
        animationRaycastOrigins[_leg].localPosition += originLocalBack.normalized * originBackwardsMultiplier;
        animationHints[_leg].position += ((transform.position) - (animationHints[_leg].position)).normalized * hintBackwardsMultiplier;
        startLocalPosition += Vector3.down * downAddPerBrokenLeg;
    }

    public void ResetLegLimp(int _leg)
    {
        animationHints[_leg].localPosition = hintLocalStartPosition[_leg];
        animationRaycastOrigins[_leg].localPosition = originLocalStartPosition[_leg];
        startLocalPosition += Vector3.up * downAddPerBrokenLeg;
    }

    private static AnimationCurve GenerateAnimationCurve(int _frequency, float _amplitude, float _seed, float _randomscale)
    {
        AnimationCurve curve = new AnimationCurve();
        curve.preWrapMode = WrapMode.PingPong;
        curve.postWrapMode = WrapMode.PingPong;

        int keys = _frequency;
        float multiplier = 1;

        float delta = (1.0f / keys);

        for (int i = 0; i < keys + 1; i++)
        {
            Keyframe keyframe = new Keyframe((delta * i) - delta, multiplier * (_amplitude + Mathf.PerlinNoise(_seed, i) * (_randomscale * multiplier)));

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
