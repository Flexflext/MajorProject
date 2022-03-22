using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ChainIK : MonoBehaviour
{
    public bool DebugGizmos = true;
    [Space]

    [Header("Chain Paramters")]
    [Tooltip("The Lenght of the Chain")]
    [Min(2.0f)]
    [SerializeField] private int chainLength = 2;

    [Tooltip("Target for the Chain to Follow")]
    [SerializeField] private Transform target;

    [Tooltip("Hint to Orient the Chain towards a Specific Position")]
    [SerializeField] private Transform hint;

    [Tooltip("Maximum Iterations of the Inverse Kinematics")]
    [Min(1.0f)]
    [SerializeField] private int maxIterations = 1;

    [Tooltip("Range in wich the Margain of Error from the Result of the Inverse Kinematics is already acceptable")]
    [Min(0.001f)]
    [SerializeField] private float errorMargain = 0.01f;

    [Tooltip("Snapback to the Start Position")]
    [Range(0f, 1f)]
    [SerializeField] private float snapbackStrenght = 1.0f;

    #region//--> Position <--\\

    //All Bones
    private Transform[] bones;

    //Current Calculated Position for the Bone
    private Vector3[] currentPositions;

    //Each Lenght of the Bones
    private float[] boneLenghts;

    //Lenght of all the Bone-Lenghts Combined
    private float completeLenght;

    //Direction to from Root Bone to Target
    private Vector3 rootTargetDirection;

    #endregion

    #region//--> Rotation <--\\

    //Direction from bone to the Succesor Bone
    private Vector3[] startdirectionsSucc;

    //StartRotation of Each Bone
    private Quaternion[] startRotationBone;

    //Startrotation of the Target
    private Quaternion startRotationTarget;

    //StartRotation of the Root Bone
    private Quaternion startRotationRoot;

    #endregion

    #region//--> Hint <--\\

    //Plane to Project the Hint on to get the closest Rotation Angle
    private Plane projectionPlane;

    //Projected Position of the Hint on the projectionPlane
    private Vector3 hintProjectionOnPlane;

    //Projected Position of the bone on the projectionPlane
    private Vector3 boneProjectedOnPlane;

    //Angle on wich the Projected Position is the Closest to the Projected Hint
    private float closestProjectionAngle;

    #endregion

    #region//--> Snapback <--\\

    //Rotation of the Parent Object of the Root
    private Quaternion rootRot;

    //Rotation Difference of Root Object and Parent of Root
    private Quaternion rootRotDiff;

    #endregion


    private void Awake()
    {
        InitializeChain();
    }

    private void LateUpdate()
    {
        ResolveIK();
    }

    /// <summary>
    /// Initialize the Chain Variables for IK System
    /// </summary>
    private void InitializeChain()
    {
        //Initialize the Array Sizes and Initial Variables
        bones  = new Transform[chainLength + 1];
        currentPositions = new Vector3[chainLength + 1];
        boneLenghts = new float[chainLength];
        completeLenght =  0;
        startdirectionsSucc = new Vector3[chainLength + 1];
        startRotationBone = new Quaternion[chainLength + 1];

        if (target != null) startRotationTarget = Quaternion.identity;

        Transform currentBone = transform;
        for (int i = bones.Length - 1; i >= 0; i--)
        {
            bones[i] = currentBone;
            startRotationBone[i] = currentBone.rotation;

            if (i == bones.Length - 1)
            {
                //last Bone
                startdirectionsSucc[i] = target.position - currentBone.position;
            }
            else
            {
                //mid Bone
                startdirectionsSucc[i] = bones[i + 1].position - currentBone.position;
                boneLenghts[i] = (bones[i + 1].position - currentBone.position).magnitude;
                completeLenght += boneLenghts[i];
            }

            currentBone = currentBone.parent;
        }

        startRotationRoot = bones[0].rotation;
    }

    /// <summary>
    /// Resolve the IK System with Position and Rotation
    /// </summary>
    private void ResolveIK()
    {
        //Flags to Check if Chain is Okay and if Target os Filled
        if (target == null) return;
        if (boneLenghts.Length != chainLength) InitializeChain();

        //Get the Current Position
        for (int i = 0; i < bones.Length; i++) currentPositions[i] = bones[i].position;

        FabricAlgortihm();

        if (hint != null)
        {
            AddHintOffset(ref currentPositions);
        }

        //Apply the Current Position and Rotation
        for (int i = 0; i < currentPositions.Length; i++)
        {
            if (i == currentPositions.Length - 1)
            {
                //Multiplies the the target rotation with the Inverse of the startrotation of the target // --> get the Difference of the Rotation to the Start with Multiply of the current and the Original Rotation
                bones[i].rotation = target.rotation * Quaternion.Inverse(startRotationTarget) * startRotationBone[i];
            }
            else
            {
                //Word Rotation from the Original Direction of the Bone to the Direction to the next Bone + Multiplies with start direction to get the Local Rotation
                bones[i].rotation = Quaternion.FromToRotation(startdirectionsSucc[i], currentPositions[i + 1] - currentPositions[i]) * startRotationBone[i];
            }

            //Set Transform Position
            bones[i].position = currentPositions[i];
        }

        
    }

    /// <summary>
    /// Add the Hint Offset to the current
    /// </summary>
    private void AddHintOffset(ref Vector3[] _currentposarray)
    {
        for (int i = 1; i < _currentposarray.Length - 1; i++)
        {
            //Create Plane at Previous Position with Direction from Previous to CUrrent Position as the Normal
            projectionPlane = new Plane(_currentposarray[i + 1] - _currentposarray[i - 1], _currentposarray[i - 1]);

            //Project the Hint on the Created Plane
            hintProjectionOnPlane = projectionPlane.ClosestPointOnPlane(hint.position);

            //Project the current Bone on the Plane
            boneProjectedOnPlane = projectionPlane.ClosestPointOnPlane(_currentposarray[i]);

            //Calculate the Angle from the current Position ans the Hint from the Previous Position
            closestProjectionAngle = Vector3.SignedAngle(boneProjectedOnPlane - _currentposarray[i - 1], hintProjectionOnPlane - _currentposarray[i - 1], projectionPlane.normal);

            //Calculate the Position for the Current Position from the Angle Between + the previous Position and the Direction to from the Previous Position
            _currentposarray[i] = Quaternion.AngleAxis(closestProjectionAngle, projectionPlane.normal) * (_currentposarray[i] - _currentposarray[i - 1]) + _currentposarray[i - 1];
        }
    }

    /// <summary>
    /// The F.A.B.R.I.K Algorthim (Forward And Backwords Reaching Inverse Kinematics)
    /// </summary>
    private void FabricAlgortihm()
    {
        //Rotation of the Parent of the Root -> Identity when null
        rootRot = (bones[0].parent != null) ? bones[0].parent.rotation : Quaternion.identity;
        //Root Rotation Difference between the Root Parent Rotation and the Start Root Rotation
        rootRotDiff = rootRot * Quaternion.Inverse(startRotationRoot);

        //IK Calculations
        //Check for Target Outside of Chain Reach
        if ((target.position - bones[0].position).sqrMagnitude >= completeLenght * completeLenght)
        {
            rootTargetDirection = (target.position - currentPositions[0]).normalized;

            for (int i = 1; i < currentPositions.Length; i++)
            {
                currentPositions[i] = currentPositions[i - 1] + rootTargetDirection * boneLenghts[i - 1];
            }
        }
        else
        {
            //Snapback to the Previous Position //-> More Dynamic Realistic Movement
            for (int i = 0; i < currentPositions.Length - 1; i++)
            {
                currentPositions[i + 1] = Vector3.Lerp(currentPositions[i + 1], currentPositions[i] + rootRotDiff * startdirectionsSucc[i], snapbackStrenght);
            }

            //Check Iterations
            for (int iterations = 0; iterations < maxIterations; iterations++)
            {
                //Backwords
                for (int i = currentPositions.Length - 1; i > 0; i--)
                {
                    if (i == currentPositions.Length - 1) currentPositions[i] = target.position;
                    else
                    {
                        //Set to Positon of the Next Chain Component + the Direction to the old position * the startlenght
                        currentPositions[i] = currentPositions[i + 1] + (currentPositions[i] - currentPositions[i + 1]).normalized * boneLenghts[i];
                    }
                }

                //Forward
                for (int i = 1; i < currentPositions.Length; i++)
                {
                    //Inverse of Backwords (Set to position of Previous Chain Component + Direction from Current to Previous * lenght to Previous)
                    currentPositions[i] = currentPositions[i - 1] + (currentPositions[i] - currentPositions[i - 1]).normalized * boneLenghts[i - 1];
                }

                //Check if in the Margain of Error
                if ((currentPositions[currentPositions.Length - 1] - target.position).sqrMagnitude <= errorMargain * errorMargain) break;
            }
        }
    }


    private void OnDrawGizmos()
    {
        if (DebugGizmos)
        {
            Transform current = this.transform;
            for (int i = 0; i < chainLength && current != null && current.parent != null; i++)
            {
                float scale = Vector3.Distance(current.position, current.parent.position) * 0.1f;
                Handles.matrix = Matrix4x4.TRS(current.position, Quaternion.FromToRotation(Vector3.up, current.parent.position - current.position), new Vector3(scale, Vector3.Distance(current.parent.position, current.position), scale));
                Handles.color = Color.blue;
                Handles.DrawWireCube(Vector3.up * 0.5f, Vector3.one);
                current = current.parent;
            }
        }
    }
}
