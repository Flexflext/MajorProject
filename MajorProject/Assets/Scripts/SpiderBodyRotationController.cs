using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class SpiderBodyRotationController : MonoBehaviour
{
    [SerializeField] private bool showDebugGizmos = true;

    [Header("Ray Paramters")]
    [Tooltip("Amount of Rays to be Shot for the Ground Detection")]
    [SerializeField] private int rayAmount = 8;
    [Tooltip("Radius of the Ray Circle (InnerRays)")]
    [SerializeField] private float innerRayPositionRadius = 0.21f;
    [Tooltip("Radius of the Ray Circle (OuterRays)")]
    [SerializeField] private float outerRayPositionRadius = 0.4f;
    [Tooltip("Tilt of the inner Rays")]
    [SerializeField] private float innerRayTiltDegree = 57;
    [Tooltip("Tilt of the outer Rays")]
    [SerializeField] private float outerRayTiltDegree = 311;
    [Tooltip("Offset of all Rays")]
    [SerializeField] private float rayOriginUpOffset = 0;
    [Tooltip("Length of all Rays")]
    [SerializeField] private float rayLength = 2;
    [Tooltip("Distance that is kept to the Ground at all Times")]
    [SerializeField] private float bodyDistanceToGround = 1;
    [Tooltip("Layers that the Rays can Hit")]
    [SerializeField] private LayerMask rayHitLayers = 1;
    [Tooltip("Weight of the InnerRays")]
    [SerializeField, Min(0)] private float innerRayWeight = 1;
    [Tooltip("Weight of the OuterRays")]
    [SerializeField, Min(0)] private float outerRayWeight = 1;

    [Header("Smoothing")]
    [Tooltip("Lerp Smoothing of the Rotation")]
    [SerializeField] private float rotationSmoothing = 20;
    [Tooltip("Lerp Smoothing of the Position")]
    [SerializeField] private float positionSmoothing = 20;

    private Vector3 rotatedForward;
    private Vector3 curPoint;
    private Vector3 currentRayDirection;
    private Vector3 tiltPosition;
    private RaycastHit hit;
    private float deltaAngleDeg;
    private float curDeg;
    private Vector3 averagePositionToPositionDirection;
    private Vector3[] innerPositions;
    private Vector3[] outerPositions;
    private Ray[] innerRays = new Ray[0];
    private Ray[] outerRays = new Ray[0];
    private AveragePositionAndUp currentAverages;


    struct AveragePositionAndUp
    {
        public Vector3 AveragePosition;
        public Vector3 AverageUp;
        public int Hits;

        public void MakeMedians()
        {
            AveragePosition /= Hits;
            AverageUp /= Hits;
        }
    }

    private void Start()
    {
        InitializeRays(rayAmount);
    }

    private void Update()
    {
        RotateSpider();
    }

    private void InitializeRays(int _points)
    {
        innerPositions = new Vector3[_points];
        outerPositions = new Vector3[_points];

        innerRays = new Ray[_points];
        outerRays = new Ray[_points];
    }


    /// <summary>
    /// Rotates the Whole Spider so that the Calculates up Vector is the CUrrent Transform Up Vector
    /// </summary>
    private void RotateSpider()
    {
        //Get the Current Average Up and Position Vector that was Scanned by the Rays
        currentAverages = GetCurrentPositionAndNormalsAvergage(transform.position, rayAmount, innerRayPositionRadius, outerRayPositionRadius, outerRayTiltDegree, innerRayTiltDegree, rayLength, rayHitLayers);

        //Sets the Distance to the Ground Dependend on the Calculated Avergage Position Vector
        SetDistanceToGround(currentAverages.AveragePosition);

        //Lerps the Up Vector -> Smooth Transitions
        currentAverages.AverageUp = Vector3.Lerp(transform.up, currentAverages.AverageUp, rotationSmoothing * Time.deltaTime);

        //Calculates the Rotation From the Previous Up Vector to the Calculated One -> Rotzates the Forward Vector with this Rotation
        rotatedForward = Quaternion.FromToRotation(transform.up, currentAverages.AverageUp) * transform.forward;

        //Calculates a new Rotation from the new Up Vector and the Rotated Forward Vector
        this.transform.rotation = Quaternion.LookRotation(rotatedForward, currentAverages.AverageUp);
    }

    /// <summary>
    /// Sets the Distance to the Ground Dependent on the Average Position of the Rays
    /// </summary>
    /// <param name="_averagepos"></param>
    private void SetDistanceToGround(Vector3 _averagepos)
    {
        //Calculate the Direction from the current Position to the _avergePosition
        averagePositionToPositionDirection = transform.position - _averagepos;
        //Normalize Direction
        averagePositionToPositionDirection.Normalize();

        //Calculate a new Position with a given Distance from the Average Position
        transform.position = Vector3.Lerp(transform.position, _averagepos + averagePositionToPositionDirection * bodyDistanceToGround * bodyDistanceToGround, positionSmoothing * Time.deltaTime);
    }

    /// <summary>
    /// Calculate the Average Position of the Ray hits and the Average Normal (Up) Vector from the Ray  hits
    /// </summary>
    /// <param name="_origin">Origin of the Rays</param>
    /// <param name="_points">Amount of Rays to be Shot</param>
    /// <param name="_innerr">Distance from the Origin to Each Origin of the Inner Rays</param>
    /// <param name="_outerr">Distance from the Origin to Each Origin of the Outer Rays</param>
    /// <param name="_outerdeg">Tilt Degree of the Outer Rays</param>
    /// <param name="_innerdeg">Tilt Degree of the Inner Rays</param>
    /// <param name="_raylength">Lenght of all Rays</param>
    /// <param name="_layermask">Mask of the Layers that the Rays can hit</param>
    /// <returns></returns>
    private AveragePositionAndUp GetCurrentPositionAndNormalsAvergage(Vector3 _origin, int _points, float _innerr, float _outerr, float _outerdeg, float _innerdeg, float _raylength, LayerMask _layermask)
    {
        //Check if the Arrays have to be Reinitilized
        if (innerPositions.Length != _points)
        {
            InitializeRays(_points);
        }

        //Set the Origin of the Rays with a given Offset
        _origin += this.transform.up * rayOriginUpOffset;

        //Calculate the Angle between each of the Ray Origins
        deltaAngleDeg = 360f / _points;
        //Reset the Current Angle Degree
        curDeg = 0;
        //Reset the Current Point
        curPoint = Vector3.zero;

        //Reset the Current Aveages Struct
        currentAverages = new AveragePositionAndUp();


        for (int i = 0; i < innerRays.Length; i++)
        {
            //Inner and Outer Positions
            curPoint = Quaternion.AngleAxis(curDeg, transform.up) * transform.right;
            innerPositions[i] = curPoint * _innerr;
            outerPositions[i] = curPoint * _outerr;

            //Add the Angle Between to the Current Degree
            curDeg += deltaAngleDeg;

            //Inner Rays
            //Calculate the Ray Direction with the Tilt for the Inner Rays
            tiltPosition = -this.transform.up * Mathf.Tan(_innerdeg * Mathf.Deg2Rad) * innerPositions[i].magnitude;
            currentRayDirection = (tiltPosition - innerPositions[i]).normalized;

            //Set the Ray
            innerRays[i] = new Ray(_origin + innerPositions[i], currentRayDirection.normalized * _raylength);

            //Check and Add the Average Position and Up for the innerRays
            CheckAverage(innerRays[i], innerRayWeight, _raylength, _layermask, ref currentAverages);


            //Outer Rays
            //Calculate the Ray Direction with the Tilt for the Outer Rays
            tiltPosition = -this.transform.up * Mathf.Tan(_outerdeg * Mathf.Deg2Rad) * outerPositions[i].magnitude;
            currentRayDirection = (outerPositions[i] - tiltPosition).normalized;
            //Set the Ray
            outerRays[i] = new Ray(_origin + outerPositions[i], currentRayDirection.normalized * _raylength);

            //Check and Add the Average Position and Up for the outerRays
            CheckAverage(outerRays[i], outerRayWeight, _raylength, _layermask, ref currentAverages);
        }

        //Calaculate the Medians
        currentAverages.MakeMedians();

        return currentAverages;
    }

    /// <summary>
    /// Check if the Ray hit anything and adds a Average of the Normal and the Position
    /// </summary>
    /// <param name="_ray">Ray to be Checked</param>
    /// <param name="_weight">Weight of the Ray</param>
    /// <param name="_raylength">Length of the Ray</param>
    /// <param name="_layermask">Hit Layermask</param>
    /// <param name="_average">Average Struct Reference</param>
    private void CheckAverage(Ray _ray, float _weight, float _raylength, LayerMask _layermask, ref AveragePositionAndUp _average)
    {
        //Check if Ray hits anything
        if (Physics.Raycast(_ray, out hit, _raylength, _layermask))
        {
            //Add hit number
            _average.Hits++;
            //Add Average Position
            _average.AveragePosition += hit.point;
            //Add Average Normal (Up)
            _average.AverageUp += hit.normal * _weight;
        }
    }

    private void OnDrawGizmos()
    {
        if (!showDebugGizmos) return;

        for (int i = 0; i < innerRays.Length; i++)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(innerRays[i].origin, innerRays[i].origin + innerRays[i].direction * rayLength);

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(outerRays[i].origin, outerRays[i].origin + outerRays[i].direction * rayLength);
        }
    }
}
