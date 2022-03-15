using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ChainIK : MonoBehaviour
{
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

    //All Bones
    private Transform[] bones;

    //Current Calculated Position for the Bone
    private Vector3[] currentPositions;

    //Each Lenght of the Bones
    private float[] boneLenghts;

    //Lenght of all the Bone-Lenghts Combined
    private float completeLenght;

    private Vector3[] startdirectionsSucc;
    private Quaternion[] startRotationBone;
    private Quaternion startRotationTarget;
    private Quaternion startRotationRoot;

    private void Awake()
    {
        InitializeChain();
    }

    private void LateUpdate()
    {
        ResolveIK();
    }

    private void InitializeChain()
    {
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

    private void ResolveIK()
    {
        if (target == null) return;
        if (boneLenghts.Length != chainLength) InitializeChain();

        //Get the Current Position
        for (int i = 0; i < bones.Length; i++) currentPositions[i] = bones[i].position;

        Quaternion rootRot = (bones[0].parent != null) ? bones[0].parent.rotation : Quaternion.identity;
        Quaternion rootRotDiff = rootRot * Quaternion.Inverse(startRotationRoot);

        //IK Calculations
        //Check for Target Outside of Chain Reach
        if ((target.position - bones[0].position).sqrMagnitude >= completeLenght * completeLenght)
        {
            Vector3 direction = (target.position - currentPositions[0]).normalized;

            for (int i = 1; i < currentPositions.Length; i++)
            {
                currentPositions[i] = currentPositions[i - 1] + direction * boneLenghts[i - 1];
            }
        }
        else
        {
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

        if (hint != null)
        {
            for (int i = 1; i < currentPositions.Length - 1; i++)
            {
                Plane plane = new Plane(currentPositions[i + 1] - currentPositions[i - 1], currentPositions[i - 1]);
                Vector3 projectedHint = plane.ClosestPointOnPlane(hint.position);
                Vector3 projectedBone = plane.ClosestPointOnPlane(currentPositions[i]);
                float angle = Vector3.SignedAngle(projectedBone - currentPositions[i - 1], projectedHint - currentPositions[i - 1], plane.normal);
                currentPositions[i] = Quaternion.AngleAxis(angle, plane.normal) * (currentPositions[i] - currentPositions[i - 1]) + currentPositions[i - 1];
            }
        }

        //Apply the Current Position and Rotation
        for (int i = 0; i < currentPositions.Length; i++)
        {
            if (i == currentPositions.Length - 1)
            {
                bones[i].rotation = target.rotation * Quaternion.Inverse(startRotationTarget) * startRotationBone[i];
            }
            else
            {
                bones[i].rotation = Quaternion.FromToRotation(startdirectionsSucc[i], currentPositions[i + 1] - currentPositions[i]) * startRotationBone[i];
            }

            bones[i].position = currentPositions[i];
        }

        
    }


    private void OnDrawGizmos()
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
