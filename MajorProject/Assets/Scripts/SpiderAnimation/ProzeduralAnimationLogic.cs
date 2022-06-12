using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public enum ELegStates
{
    LS_Normal,
    LS_Limping,
    LS_LimpingHalfLeg,
    LS_Broken,
}

public class ProzeduralAnimationLogic : MonoBehaviour
{
    #region Properties

    [SerializeField] private bool showDebugGizmos = true;

    [Header("Movement Animation")]
    [Tooltip("Time in wich the Leg Moves from old to new Position")]
    [SerializeField] private float legMovementTime;
    public float LegMovementTime { get { return legMovementTime + Random.Range(randomExtraLegMovementTime.x, randomExtraLegMovementTime.y); } }
    [SerializeField] private Vector2 randomExtraLegMovementTime = Vector2.zero;
    [Tooltip("Curve in wich the leg Moves up")]
    [SerializeField] private AnimationCurve legMovementCurve;
    public AnimationCurve LegMovementCurve { get { return legMovementCurve; } }
    [Tooltip("Maximum Range of that the leg can be before moveing to new Position")]
    [SerializeField] private float maxLegRange;

    [Header("Additional Flags")]
    [Tooltip("Use the Closeset Possible or the Farthest Posssible Position")]
    [SerializeField] private bool useFarthestPoint;
    [SerializeField] private bool additionalLegRangeCheck;
    [SerializeField] private bool adjustLastLimbToNormal;
    [SerializeField] private bool additionalLegCollisionCheck;
    [SerializeField] private bool alwaysCheckLegState;
    [SerializeField] private bool checkDeathState;
    [SerializeField] private bool alwaysCheckDeathState;
    [SerializeField] private bool preferLongestRangeLeg;


    [Header("Body Animation")]
    [SerializeField] private bool animateBody = true;
    [SerializeField] private BodyAnimation xAnimation;
    [SerializeField] private BodyAnimation yAnimation;
    [SerializeField] private BodyAnimation zAnimation;


    [Header("Death Animation")]
    [Range(0f, 1f)]
    [SerializeField] private float maxPercentOfAlmostBrokenLegs = 0.5f;
    [SerializeField] private ELegStates almostBrokenLegState = ELegStates.LS_LimpingHalfLeg;
    [Range(0f, 1f)]
    [SerializeField] private float minPercentOfNormalLegs = 0.25f;
    [Space]
    [SerializeField] private float pauseTillDeath = 0.3f;
    [SerializeField] private bool deathBodyAnimate = true;
    [SerializeField] private BodyAnimation deathXAnimation;
    [SerializeField] private BodyAnimation deathYAnimation;
    [SerializeField] private BodyAnimation deathZAnimation;
    [Space]
    [SerializeField] private float legDeathFoldTime = 1f;
    [SerializeField] private Vector2 legDeathFoldRndTimeAddMinMax = new Vector2(-0.15f, 0.15f);
    [Space]
    [Range(0f, 1f)]
    [SerializeField] private float legDeathFoldPositionToBodyPercent = 0.5f;
    [SerializeField] private Vector2 legDeathFoldRndPositionAddMinMax = new Vector2(-0.15f, 0.15f);
    [Space]
    [SerializeField] private UnityEvent onDeathEvent;
    [SerializeField] private UnityEvent onDeathResetEvent;

    [Header("ExtraLegAnimation")]
    [SerializeField] private float bodySmoothing = 8;
    [SerializeField] private float hightAddMultiplier = 0.35f;
    [SerializeField] private float originBackwardsMultiplier = 0.35f;
    [SerializeField] private Vector3 originLocalBack = Vector3.left;
    [SerializeField] private float hintBackwardsMultiplier = 0.35f;
    [SerializeField] private float downAddPerBrokenLeg = 0.1f;
    [SerializeField] private float maxDownAddPerBrokenLeg = 0.2f;

    private float currentDownAddPerBrokenLeg = 0;
    public float CurrentDownAddPerBrokenLeg { get { return currentDownAddPerBrokenLeg; } set { currentDownAddPerBrokenLeg = Mathf.Clamp(value, 0, maxDownAddPerBrokenLeg); } }

    [SerializeField] private float percentOfLegHeightMovement = 0.1f;
    public float PercentOfLegHeightMovement { get { return percentOfLegHeightMovement; } }

    [Range(0.001f, 1)]
    [SerializeField] private float brokenLegRangeMultiplier;


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
    [SerializeField] private LegParams[] legs;

    private float ranges;

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

    private Quaternion toRot = Quaternion.identity;
    private Quaternion startRot;
    private Quaternion newRot = Quaternion.identity;

    private ELegStates currentLegStateEnum;

    private LegState[] beforeDeathStates;

    private bool isDead;

    private LegDeadState legDeadState;

    private int currentNumberOfNormalLegs = 0;
    private int currentNumberOfAlmostBrokenLegs = 0;

    public Dictionary<StateMachineSwitchDelegate, LegState> stateDictionary { get; set; }

    #endregion

    #region Help-Structs

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

    [System.Serializable]
    public struct LegParams
    {
#if UNITY_EDITOR
        [HideInInspector] public string name;
#endif

        [Header("Leg Targets and Ray Origins")] //--> First all Left Legs than right Legs
        public IKSystem legIKSystem;
        public Transform animationRaycastOrigin;
        public ELegStates legState;
        public bool stopLegAnimationFlag;

        [Header("LegPrefabs")]
        [Tooltip("FirstHalf of the Leg -- starting from target")]
        public GameObject halfLegPrefabFirstRagdoll;
        [Tooltip("Second Half of the Leg -- starting from end of first part")]
        public GameObject halfLegPrefabSecondRagdoll;


        [HideInInspector] public Vector3 currentAnimationTargetPosition;
        [HideInInspector] public Vector3 nextAnimationTargetPosition;
        [HideInInspector] public Vector3 hintLocalStartPosition;
        [HideInInspector] public Vector3 originLocalStartPosition;
        [HideInInspector] public Vector3 targetUp;
        [HideInInspector] public bool moveingLeg;
        [HideInInspector] public bool isOnMoveDelay;
        [HideInInspector] public Transform ikTarget;
        [HideInInspector] public Transform animationHint;
        [HideInInspector] public float currentRangeMultiplier;
        [HideInInspector] public float rangeLegToCalcPos;
        [HideInInspector] public LegState currentLegState;
        [HideInInspector] public LegState beforeDeathLegState;
    }

    #endregion

    #region Unity-Methods

    private void Start()
    {
        startLocalPosition = transform.localPosition;
        startRot = transform.localRotation;
        newRot = startRot;

        InitializeLegsArray();

        CreateStateDictionary();

        GenerateDeathAndBodyAnimationCurves();
    }

    private void OnValidate()
    {
        GenerateDeathAndBodyAnimationCurves();

#if UNITY_EDITOR
        for (int i = 0; i < legs.Length; i++)
        {
            if (i < legs.Length / 2)
            {
                if (i == 0)
                {
                    legs[i].name = "Left Leg Front";
                }
                else if (i == legs.Length / 2 - 1)
                {
                    legs[i].name = "Left Leg Back";
                }
                else
                {
                    legs[i].name = "Left Leg Middle" + i;
                }
            }
            else
            {
                if (i == legs.Length / 2)
                {
                    legs[i].name = "Right Leg Front";
                }
                else if (i == legs.Length - 1)
                {
                    legs[i].name = "Right Leg Back";
                }
                else
                {
                    legs[i].name = "Right Leg Middle" + (i - (legs.Length / 2));
                }
            }
        }
#endif
    }

    private void Update()
    {
        CalculateTargetPosition();
        CheckRange();

        if (!isDead)
        {
            AnimateBody();
            this.transform.localRotation = Quaternion.Lerp(transform.localRotation, newRot, bodySmoothing * Time.deltaTime);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!isDead)
            {
                SetDeath();

            }
            else
            {
                ResetDeath();
            }
        }

        if (alwaysCheckLegState)
        {
            for (int i = 0; i < legs.Length; i++) CheckAndSetLegState(i);     
        }

        if (checkDeathState && alwaysCheckDeathState)
        {
            CheckDeath();
        }
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        if (!showDebugGizmos) return;


        for (int i = 0; i < legs.Length; i++)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(legs[i].currentAnimationTargetPosition, 0.1f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(legs[i].animationRaycastOrigin.position, 0.1f);

            Gizmos.color = Color.black;
            Gizmos.DrawWireSphere(legs[i].currentAnimationTargetPosition, maxLegRange * legs[i].currentRangeMultiplier);
        }

        for (int i = 0; i < legs.Length; i++)
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
                curPoint = Quaternion.AngleAxis(curDeg, legs[i].animationRaycastOrigin.up) * legs[i].animationRaycastOrigin.right;
                //Set the leg Ray Radius
                curPoint = curPoint * legRayPositionradius;

                //Tilt the Vector by the Tilt Degree
                tiltedVector = -legs[i].animationRaycastOrigin.up * Mathf.Tan(legRayTiltDegree * Mathf.Deg2Rad) * curPoint.magnitude;
                //Set Ray Direction
                rayDir = (tiltedVector - curPoint).normalized;

                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(legs[i].animationRaycastOrigin.position + curPoint, legs[i].animationRaycastOrigin.position + curPoint + rayDir * legRaylength);

                curDeg += deltaDeg;
            }
        }

        Gizmos.color = Color.magenta;
        Gizmos.DrawRay(this.transform.position, bodyNormal);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(this.transform.position, transform.up);
    }

    #endregion

    #region Public-Methods

    /// <summary>
    /// Calculates a Rotation wich elevates the Opposite Side of the Leg that should be Limping
    /// </summary>
    /// <param name="_leg"></param>
    public void AdjustBrokenLegRotation(int _leg)
    {
        Plane plane = new Plane(transform.up, transform.position);

        Vector3 pos = plane.ClosestPointOnPlane(legs[_leg].animationRaycastOrigin.position);

        toRot = Quaternion.FromToRotation((transform.InverseTransformPoint((legs[_leg].animationRaycastOrigin.position + transform.up * hightAddMultiplier)) - transform.localPosition), (transform.InverseTransformPoint(pos) - transform.localPosition));

        newRot = startRot * Quaternion.Inverse(toRot);
    }

    /// <summary>
    /// Resets the Broken Leg Rotation
    /// </summary>
    public void ResetBrokenLegRotation()
    {
        newRot = startRot;
    }

    public void AddDeathEventListener(UnityAction _event)
    {
        onDeathEvent.AddListener(_event);
    }

    public void RemoveDeathEventListener(UnityAction _event)
    {
        onDeathEvent.RemoveListener(_event);
    }

    public void AddDeathResetEventListener(UnityAction _event)
    {
        onDeathResetEvent.AddListener(_event);
    }

    public void RemoveDeathResetEventListener(UnityAction _event)
    {
        onDeathResetEvent.RemoveListener(_event);
    }

    public void CheckDeath()
    {
        if (!isDead)
        {
            float maxNumberOfNormalLegs = legs.Length;
            currentNumberOfNormalLegs = 0;
            currentNumberOfAlmostBrokenLegs = 0;

            for (int i = 0; i < legs.Length; i++)
            {
                if (legs[i].legState == ELegStates.LS_Normal)
                {
                    currentNumberOfNormalLegs++;
                }
                else if ((int)legs[i].legState >= (int)almostBrokenLegState)
                {
                    currentNumberOfAlmostBrokenLegs++;
                }
            }


            if (currentNumberOfNormalLegs / maxNumberOfNormalLegs <= minPercentOfNormalLegs)
            {

                SetDeath();
                return;
            }

            if (currentNumberOfAlmostBrokenLegs / maxNumberOfNormalLegs >= maxPercentOfAlmostBrokenLegs)
            {
                SetDeath();
                return;
            }
        }
    }

    public void SetDeath()
    {
        if (onDeathEvent != null)
        {
            onDeathEvent.Invoke();
        }
        isDead = true;
        StartCoroutine(C_WaitToDie());
    }

    public void ResetDeath()
    {
        if (onDeathResetEvent != null)
        {
            onDeathResetEvent.Invoke();
        }

        isDead = false;
        Destroy(gameObject.GetComponent<Rigidbody>());

        for (int i = 0; i < legs.Length; i++)
        {
            legs[i].legState = ELegStates.LS_Normal;
            currentLegStateEnum = legs[i].legState;
            legs[i].currentLegState.ExitLegState(i);
            legs[i].currentLegState = legs[i].beforeDeathLegState;
        }
    }

    public void SetLegState(int _leg, ELegStates _legstate)
    {
        legs[_leg].legState = _legstate;
    }

    public void DecreaseLegHealth(int _leg)
    {
        if (legs[_leg].legState != ELegStates.LS_Broken)
        {
            legs[_leg].legState ++;
        }
    }

    public LegParams GetFrontRightLeg()
    {
        for (int i = legs.Length / 2; i < legs.Length; i++)
        {
            if (legs[i].legState < ELegStates.LS_Broken)
            {
                legs[i].stopLegAnimationFlag = true;
                return legs[i];
            }
        }


        return legs[legs.Length / 2];
    }

    #endregion

    #region Private-Methods

    /// <summary>
    /// Creates the StateMachine Dictionary for the StateMachine
    /// </summary>
    private void CreateStateDictionary()
    {
        //Create States
        LegNormalState legNormalState = new LegNormalState(this, null, null, legs);
        LegLimpingState legLimpingState = new LegLimpingState(this, SetLegLimp, ResetLegLimp, legs);
        LegLimpingHalfLegState legLimpngHalfLegState = new LegLimpingHalfLegState(this, SetHalfLeg, ResetHalfLeg, legs);
        LegBrokenState legBrokenState = new LegBrokenState(this, SetBrokenLeg, ResetBrokenLeg, legs);
        legDeadState = new LegDeadState(this, null, ResetBrokenLeg, legs, legDeathFoldTime, legDeathFoldRndTimeAddMinMax, legDeathFoldPositionToBodyPercent, legDeathFoldRndPositionAddMinMax);

        //Set States and Change Parameters in the Dictionary
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
            {
                () => (currentLegStateEnum == ELegStates.LS_LimpingHalfLeg),
                legLimpngHalfLegState
            },
            {
                () => (currentLegStateEnum == ELegStates.LS_Broken),
                legBrokenState
            }
        };


        for (int i = 0; i < legs.Length; i++)
        {
            legs[i].currentLegState = legNormalState;
        }
    }

    /// <summary>
    /// Calculate the the currentAnimationTargetPositions each Frame for all Legs
    /// </summary>
    private void CalculateTargetPosition()
    {
        //Check all Legs
        for (int i = 0; i < legs.Length; i++)
        {
            if (legs[i].stopLegAnimationFlag) continue;

            //Set the delta Degree -> degree per point
            deltaDeg = 360f / legRayNumber;
            //Reset Current Degree
            curDeg = 0;

            //Reset CurrentPoint
            curPoint = Vector3.zero;
            //Reset ClosestPoint
            closestPoint = Vector3.zero;
            first = true;

            if (useFarthestPoint)
            {
                closestPoint = transform.root.position;
            }
            else
            {
                closestPoint = legs[i].ikTarget.position;
            }


            //Create Ray for each legnum
            for (int j = 0; j < legRayNumber; j++)
            {

                //Set the CurrentPoint with the Rotated right Vector around the up Vector with the current Angle
                curPoint = Quaternion.AngleAxis(curDeg, legs[i].animationRaycastOrigin.up) * legs[i].animationRaycastOrigin.right;
                //Set the leg Ray Radius
                curPoint = curPoint * legRayPositionradius;

                //Tilt the Vector by the Tilt Degree
                tiltedVector = -legs[i].animationRaycastOrigin.up * Mathf.Tan(legRayTiltDegree * Mathf.Deg2Rad) * curPoint.magnitude;
                //Set Ray Direction
                rayDir = (tiltedVector - curPoint).normalized;

                hit = new RaycastHit();

                //Check the íf the Ray hit anything
                if (Physics.Raycast(legs[i].animationRaycastOrigin.position + curPoint, rayDir, out hit, legRaylength, raycastHitLayers))
                {
                    curLength = (hit.point - transform.root.position).sqrMagnitude;



                    //Check what the closest Point is +set the first Ray as the first Closest Point
                    if (first)
                    {
                        //Set ClosestPoint
                        if (additionalLegRangeCheck && ((legs[i].legIKSystem.transform.position - hit.point).sqrMagnitude > (legs[i].legIKSystem.GetMaxRangeOfChain() * legs[i].legIKSystem.GetMaxRangeOfChain())))
                        {
                            continue;
                        }



                        SetClosestPoint(hit, i);
                    }
                    //Check if Closer than the Current Closest Point
                    if (useFarthestPoint && curLength >= (closestPoint - transform.root.position).sqrMagnitude || !useFarthestPoint && (hit.point - transform.root.position).sqrMagnitude <= curLength)
                    {
                        if (additionalLegCollisionCheck)
                        {
                            if (Physics.Raycast(transform.position, hit.point - transform.position, out secondhit, float.MaxValue, raycastHitLayers))
                            {
                                if (Vector3.SqrMagnitude(secondhit.point - hit.point) > 0.05f)
                                {
                                    continue;
                                }
                            }
                        }

                        

                        if (additionalLegRangeCheck && ((legs[i].legIKSystem.transform.position - hit.point).sqrMagnitude > (legs[i].legIKSystem.GetMaxRangeOfChain() * legs[i].legIKSystem.GetMaxRangeOfChain())))
                        {
                            continue;
                        }

                        //Set ClosestPoint
                        SetClosestPoint(hit, i);
                    }
                }

                //Add Degree for Next Ray
                curDeg += deltaDeg;
            }
        }
    }

    /// <summary>
    /// Set the Closest Point for a Given Leg at the Index | Also Sets the normal and Resets first bool
    /// </summary>
    /// <param name="_hit"></param>
    /// <param name="_idx"></param>
    private void SetClosestPoint(RaycastHit _hit, int _idx)
    {
        closestPoint = _hit.point;
        legs[_idx].currentAnimationTargetPosition = _hit.point;
        legs[_idx].targetUp = _hit.normal;
        first = false;
    }

    /// <summary>
    /// Set the NextAnimationTargetPosition to the currentAnimationTargetPosition for a given Leg
    /// </summary>
    /// <param name="_legtomove"></param>
    private void SetNewTargetPosition(int _legtomove)
    {
        //Set the Up Vector
        if (adjustLastLimbToNormal) legs[_legtomove].ikTarget.up = legs[_legtomove].targetUp;
        //Set the new AnimationTarget Position
        legs[_legtomove].nextAnimationTargetPosition = legs[_legtomove].currentAnimationTargetPosition;
    }

    /// <summary>
    /// Check the Ranges of Each Leg --> And Invokes Move for that Leg
    /// </summary>
    private void CheckRange()
    {
        int preferredLeg = 0;
        

        if (preferLongestRangeLeg)
        {
            float longRange = 0;

            for (int i = 0; i < legs.Length; i++)
            {
                legs[i].rangeLegToCalcPos = (legs[i].currentAnimationTargetPosition - legs[i].nextAnimationTargetPosition).sqrMagnitude;

                if (longRange < legs[i].rangeLegToCalcPos)
                {
                    longRange = legs[i].rangeLegToCalcPos;
                    preferredLeg = i;
                }
            }
        }


        for (int i = 0; i < legs.Length; i++)
        {
            //Check if the Current Leg isnt already Moving
            if (!legs[i].moveingLeg)
            {
                if (legs[i].stopLegAnimationFlag) continue;
                //Calculate the Squared Lenght from the Old Animation Target to the current Animation Target
                ranges = (legs[i].currentAnimationTargetPosition - legs[i].nextAnimationTargetPosition).sqrMagnitude;

                //Reset the Target Position
                legs[i].ikTarget.position = legs[i].nextAnimationTargetPosition;

                if (!legs[i].isOnMoveDelay || (preferLongestRangeLeg && preferredLeg == i))
                {
                    //Check if the Calculated Range is greater than the maxLegRange
                    if (ranges >= ((maxLegRange * legs[i].currentRangeMultiplier) * (maxLegRange * legs[i].currentRangeMultiplier))
                        || additionalLegRangeCheck && ((legs[i].legIKSystem.transform.position - legs[i].nextAnimationTargetPosition).sqrMagnitude > (legs[i].legIKSystem.GetMaxRangeOfChain() * legs[i].legIKSystem.GetMaxRangeOfChain())))
                    {
                        //Check Edge Cases with at Position 0 or half
                        if (i == 0 || i == legs.Length / 2)
                        {
                            //Check the First Leg (Front Left) (Edge Case)
                            if (i == 0)
                            {
                                //Check that the Previous Leg or the Leg on the Other Side is not Moving
                                if (legs[legs.Length / 2].moveingLeg)
                                {
                                    continue;
                                }
                            }
                            //Check the Leg on the Other Side of the First Leg (Front Right)
                            else
                            {
                                if (legs[0].moveingLeg)
                                {
                                    continue;
                                }
                            }
                        }
                        else
                        {
                            //Check if Left Side Leg or Right Side Leg //-> Left Side
                            if (i < legs.Length / 2)
                            {
                                //Check that the Previous Leg or the Leg on the Other Side is not Moving
                                if (legs[i - 1].moveingLeg || legs[i + legs.Length / 2 - 1].moveingLeg)
                                {
                                    continue;
                                }
                            }
                            else //-> Right Side
                            {
                                //Check that the Previous Leg or the Leg on the Other Side is not Moving
                                if (legs[i - 1].moveingLeg || legs[i - legs.Length / 2].moveingLeg)
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
    }

    /// <summary>
    /// Move the Leg at the given Index
    /// </summary>
    /// <param name="_leg"></param>
    private void MoveLeg(int _leg)
    {
        //Set Flag
        legs[_leg].moveingLeg = true;
        legs[_leg].isOnMoveDelay = true;
        //Set the new Target Position
        SetNewTargetPosition(_leg);

        if (!alwaysCheckLegState)
        {
            CheckAndSetLegState(_leg);
        }

        //Start the Move Coroutine
        StartCoroutine(legs[_leg].currentLegState.C_MoveLegCoroutine(_leg));

        if (checkDeathState && !alwaysCheckDeathState)
        {
            CheckDeath();
        }
    }

    /// <summary>
    /// Animates the Body Position from the Animation Curves
    /// </summary>
    private void AnimateBody()
    {
        if (!animateBody) return;

        Vector3 newLocalPosition = UseAnimationCurves(xAnimation, yAnimation, zAnimation);

        transform.localPosition = newLocalPosition + startLocalPosition + Vector3.down * CurrentDownAddPerBrokenLeg;
    }

    /// <summary>
    /// Evaluate BodyAnimation Curves and get back the a new VEctor at the Current-Time
    /// </summary>
    /// <param name="_xanimation"></param>
    /// <param name="_yanimation"></param>
    /// <param name="_zanimation"></param>
    /// <returns></returns>
    private Vector3 UseAnimationCurves(BodyAnimation _xanimation, BodyAnimation _yanimation, BodyAnimation _zanimation)
    {
        Vector3 add = Vector3.zero;

        //Check if the to Use the X Animation
        if (_xanimation.AnimationParameter.useAnimation)
        {
            //Evaluate the Curve at the Current Time and multiplie the specified Height 
            add.x = _xanimation.AnimationCurveSettings.positionCurve.Evaluate(Time.time / xAnimation.AnimationParameter.timeMultiplier) * xAnimation.AnimationParameter.heightMultiplier;
        }

        //Check if to use the Y Aniamtion
        if (_yanimation.AnimationParameter.useAnimation)
        {
            //Evaluate the Curve at the Current Time and multiplie the specified Height 
            add.y = _yanimation.AnimationCurveSettings.positionCurve.Evaluate(Time.time / yAnimation.AnimationParameter.timeMultiplier) * yAnimation.AnimationParameter.heightMultiplier;
        }

        //Check if to use the Z Animation
        if (_zanimation.AnimationParameter.useAnimation)
        {
            //Evaluate the Curve at the Current Time and multiplie the specified Height 
            add.z = _zanimation.AnimationCurveSettings.positionCurve.Evaluate(Time.time / zAnimation.AnimationParameter.timeMultiplier) * zAnimation.AnimationParameter.heightMultiplier;
        }

        return add;
    }

    /// <summary>
    /// Set Leg Limp on given Leg Index
    /// </summary>
    /// <param name="_leg"></param>
    private void SetLegLimp(int _leg)
    {
        legs[_leg].animationRaycastOrigin.localPosition += originLocalBack.normalized * originBackwardsMultiplier;
        legs[_leg].animationHint.position += ((transform.position) - (legs[_leg].animationHint.position)).normalized * hintBackwardsMultiplier;
        CurrentDownAddPerBrokenLeg += downAddPerBrokenLeg;
    }

    /// <summary>
    /// Reset leg Limp on given Leg Index
    /// </summary>
    /// <param name="_leg"></param>
    private void ResetLegLimp(int _leg)
    {
        legs[_leg].animationHint.localPosition = legs[_leg].hintLocalStartPosition;
        legs[_leg].animationRaycastOrigin.localPosition = legs[_leg].originLocalStartPosition;
        CurrentDownAddPerBrokenLeg -= downAddPerBrokenLeg;
    }

    /// <summary>
    /// Set Half Leg Limp on given Leg
    /// </summary>
    /// <param name="_leg"></param>
    private void SetHalfLeg(int _leg)
    {
        int newLenght = legs[_leg].legIKSystem.ChainLenght;

        int oldlenght = newLenght;

        newLenght = ((newLenght % 2 == 0) ? (newLenght / 2) : (newLenght / 2 + 1));

        legs[_leg].legIKSystem.ChainLenght = newLenght;

        CurrentDownAddPerBrokenLeg += downAddPerBrokenLeg;


        Plane plane = new Plane(transform.up, legs[_leg].animationRaycastOrigin.position);

        Vector3 pos = plane.ClosestPointOnPlane(transform.position);

        pos = ((legs[_leg].animationRaycastOrigin.localPosition - legs[_leg].animationRaycastOrigin.InverseTransformPoint(pos)).normalized * 0.2f);


        legs[_leg].currentRangeMultiplier = brokenLegRangeMultiplier;
        legs[_leg].animationRaycastOrigin.localPosition -= pos;

        legs[_leg].legIKSystem.SpawnLegAtTransformIndex(legs[_leg].halfLegPrefabFirstRagdoll, oldlenght - newLenght);
    }

    /// <summary>
    /// Reset Half Leg on Given Leg
    /// </summary>
    /// <param name="_leg"></param>
    private void ResetHalfLeg(int _leg)
    {
        legs[_leg].legIKSystem.ResetChainLenght();
        CurrentDownAddPerBrokenLeg -= downAddPerBrokenLeg;

        Plane plane = new Plane(transform.up, legs[_leg].animationRaycastOrigin.position);

        Vector3 pos = plane.ClosestPointOnPlane(transform.position);

        pos = ((legs[_leg].animationRaycastOrigin.localPosition - legs[_leg].animationRaycastOrigin.InverseTransformPoint(pos)).normalized * 0.2f);

        legs[_leg].currentRangeMultiplier = 1;
        legs[_leg].animationRaycastOrigin.localPosition += pos;
    }

    /// <summary>
    /// Set Broken Leg on Given Leg
    /// </summary>
    /// <param name="_leg"></param>
    private void SetBrokenLeg(int _leg)
    {
        legs[_leg].legIKSystem.SpawnLegAtTransformIndex(legs[_leg].halfLegPrefabSecondRagdoll, 0);

        legs[_leg].legIKSystem.gameObject.SetActive(false);
        legs[_leg].legIKSystem.transform.localScale = Vector3.zero;

        int nextLeg = _leg;

        if (_leg + 1 >= legs.Length || _leg + 1 == legs.Length / 2)
        {
            //Leg is One of the Edge Cases Left Back or Right Back
            nextLeg--;
        }
        else
        {
            nextLeg++;
        }

        legs[nextLeg].animationRaycastOrigin.position += (legs[_leg].animationRaycastOrigin.position - legs[nextLeg].animationRaycastOrigin.position) / 2;

        
    }

    /// <summary>
    /// Reset Broken Leg on given Legs
    /// </summary>
    /// <param name="_leg"></param>
    private void ResetBrokenLeg(int _leg)
    {
        legs[_leg].legIKSystem.transform.localScale = Vector3.one;
        legs[_leg].legIKSystem.gameObject.SetActive(true);

        int nextLeg = _leg;

        if (_leg + 1 >= legs.Length || _leg + 1 == legs.Length / 2)
        {
            //Leg is One of the Edge Cases Left Back or Right Back
            nextLeg--;
        }
        else
        {
            nextLeg++;
        }

        legs[nextLeg].animationRaycastOrigin.position -= (legs[_leg].animationRaycastOrigin.position - legs[nextLeg].animationRaycastOrigin.position) / 2;
    }

    private IEnumerator C_WaitToDie()
    {
        float curTime = 0;

        while (curTime <= pauseTillDeath)
        {
            //Shake Spider
            if (deathBodyAnimate)
            {
                Vector3 newLocalPosition = UseAnimationCurves(deathXAnimation, deathYAnimation, deathZAnimation);
                transform.localPosition = newLocalPosition + startLocalPosition;
            }


            curTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }



        Rigidbody rb = gameObject.AddComponent<Rigidbody>();
        rb.Sleep();

        for (int i = 0; i < legs.Length; i++)
        {
            legs[i].legIKSystem.GiveBodyRigidbody(rb);
            legs[i].beforeDeathLegState = legs[i].currentLegState;

            currentLegStateEnum = legs[i].legState;
            legs[i].currentLegState = legDeadState;
            legs[i].currentLegState.EnterLegState(i);
        }

        yield return new WaitForSeconds(legDeathFoldTime);

        rb.WakeUp();
    }

    private void CheckAndSetLegState(int _leg)
    {
        currentLegStateEnum = legs[_leg].legState;

        foreach (var state in stateDictionary)
        {
            if (state.Key())
            {
                if (state.Value != legs[_leg].currentLegState)
                {
                    legs[_leg].currentLegState.ExitLegState(_leg);
                    legs[_leg].currentLegState = state.Value;
                    legs[_leg].currentLegState.EnterLegState(_leg);
                }
            }
        }
    }

    private void GenerateDeathAndBodyAnimationCurves()
    {
        if (animateBody)
        {
            xAnimation.AnimationCurveSettings.positionCurve = GenerateAnimationCurve(xAnimation.AnimationCurveSettings.frequency, xAnimation.AnimationCurveSettings.amplitude, xAnimation.AnimationCurveSettings.seed, xAnimation.AnimationCurveSettings.randomScale);
            yAnimation.AnimationCurveSettings.positionCurve = GenerateAnimationCurve(yAnimation.AnimationCurveSettings.frequency, yAnimation.AnimationCurveSettings.amplitude, yAnimation.AnimationCurveSettings.seed, yAnimation.AnimationCurveSettings.randomScale);
            zAnimation.AnimationCurveSettings.positionCurve = GenerateAnimationCurve(zAnimation.AnimationCurveSettings.frequency, zAnimation.AnimationCurveSettings.amplitude, zAnimation.AnimationCurveSettings.seed, zAnimation.AnimationCurveSettings.randomScale);
        }

        if (deathBodyAnimate)
        {
            deathXAnimation.AnimationCurveSettings.positionCurve = GenerateAnimationCurve(deathXAnimation.AnimationCurveSettings.frequency, deathXAnimation.AnimationCurveSettings.amplitude, deathXAnimation.AnimationCurveSettings.seed, deathXAnimation.AnimationCurveSettings.randomScale);
            deathYAnimation.AnimationCurveSettings.positionCurve = GenerateAnimationCurve(deathYAnimation.AnimationCurveSettings.frequency, deathYAnimation.AnimationCurveSettings.amplitude, deathYAnimation.AnimationCurveSettings.seed, deathYAnimation.AnimationCurveSettings.randomScale);
            deathZAnimation.AnimationCurveSettings.positionCurve = GenerateAnimationCurve(deathZAnimation.AnimationCurveSettings.frequency, deathZAnimation.AnimationCurveSettings.amplitude, deathZAnimation.AnimationCurveSettings.seed, deathZAnimation.AnimationCurveSettings.randomScale);
        }
    }

    private void InitializeLegsArray()
    {
        //Set the Initial Leg target Position Data
        for (int i = 0; i < legs.Length; i++)
        {
            legs[i].ikTarget = legs[i].legIKSystem.GetTarget();
            legs[i].animationHint = legs[i].legIKSystem.GetHint();

            legs[i].nextAnimationTargetPosition = legs[i].ikTarget.position;
            legs[i].hintLocalStartPosition = legs[i].animationHint.localPosition;
            legs[i].originLocalStartPosition = legs[i].animationRaycastOrigin.localPosition;

            legs[i].currentRangeMultiplier = 1;
        }
    }

    /// <summary>
    /// Generate an Animation Curve with the given Parameters
    /// </summary>
    /// <param name="_frequency"></param>
    /// <param name="_amplitude"></param>
    /// <param name="_seed"></param>
    /// <param name="_randomscale"></param>
    /// <returns></returns>
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

#endregion
}
