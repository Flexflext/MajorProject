using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderController : MonoBehaviour
{
    public int Points;
    public float InnerRadius;
    public float OuterRadius;
    public float InnerDeg;
    public float OuterDeg;
    public float Offset;
    public float Lenght;
    public float Speed;
    public float distanceToGround;
    public LayerMask layers;

    private Vector3 rotatedForward;

    Vector3[] innerPositions;
    Vector3[] outerPositions;

    Ray[] innerRays = new Ray[0];
    Ray[] outerRays = new Ray[0];

    private Vector3 input;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        RotateSpider();
        HandlePlayerInput();
        SetDistanceToGround();
    }

    private void FixedUpdate()
    {
        MoveSpider();
    }

    private void HandlePlayerInput()
    {
        input = Vector3.zero;

        input += Input.GetAxis("Horizontal") * transform.right;
        input += Input.GetAxis("Vertical") * transform.forward;

        input.Normalize();
    }

    private void MoveSpider()
    {
        transform.position += input * Time.fixedDeltaTime * Speed;
    }

    private void RotateSpider()
    {
        Vector3 up = GetCurrentMedians(transform.position, Points, InnerRadius, OuterRadius, OuterDeg, InnerDeg, Lenght, layers);

        up = Vector3.Lerp(transform.up, up, 20 * Time.deltaTime);

        rotatedForward = Quaternion.FromToRotation(transform.up, up) * transform.forward;
        this.transform.rotation = Quaternion.LookRotation(rotatedForward, up);
    }

    private void SetDistanceToGround()
    {
        Ray ray = new Ray(transform.position, -transform.up);
        RaycastHit hit = new RaycastHit();

        if (Physics.Raycast(ray, out hit, float.MaxValue, layers))
        {
            transform.position = Vector3.Lerp(transform.position, hit.point + transform.up * distanceToGround, 20 * Time.deltaTime);
        }
    }

    private Vector3 GetCurrentMedians(Vector3 _origin, int _points, float _innerr, float _outerr, float _outerdeg, float _innerdeg, float _raylength, LayerMask _layermask)
    {
        _origin += this.transform.up * Offset;

        innerPositions = new Vector3[_points];
        outerPositions = new Vector3[_points];

        float deltaDeg = 360f / _points;
        float curDeg = 0;

        Vector3 curPoint = Vector3.zero;

        for (int i = 0; i < _points; i++)
        {
            curPoint = Quaternion.AngleAxis(curDeg, transform.up) * transform.right;
            innerPositions[i] = curPoint * _innerr;
            outerPositions[i] = curPoint * _outerr;

            curDeg += deltaDeg;
        }

        innerRays = new Ray[_points];
        outerRays = new Ray[_points];


        for (int i = 0; i < innerRays.Length; i++)
        {
            //Inner Rays
            Vector3 t = -this.transform.up * Mathf.Tan(_innerdeg * Mathf.Deg2Rad) * innerPositions[i].magnitude;
            Vector3 dir = (t - innerPositions[i]).normalized;


            //Gizmos.color = Color.green;
            innerRays[i] = new Ray(_origin + innerPositions[i], dir.normalized * _raylength);
            //Gizmos.DrawLine(_origin + innerPositions[i], _origin + innerPositions[i] + dir * _raylength);

            //Outer Rays
            t = -this.transform.up * Mathf.Tan(_outerdeg * Mathf.Deg2Rad) * outerPositions[i].magnitude;
            dir = (outerPositions[i] - t).normalized;

            //Gizmos.color = Color.cyan;
            outerRays[i] = new Ray(_origin + outerPositions[i], dir.normalized * _raylength);
            //Gizmos.DrawLine(_origin + outerPositions[i], _origin + outerPositions[i] + dir * _raylength);
        }

        Vector3 results = new Vector3();
        int hits = 0;

        for (int i = 0; i < innerRays.Length; i++)
        {
            if (Physics.Raycast(innerRays[i], out RaycastHit hit, _raylength, _layermask))
            {
                hits++;
                results += hit.normal;
            }
        }

        for (int i = 0; i < outerRays.Length; i++)
        {
            if (Physics.Raycast(outerRays[i], out RaycastHit hit, _raylength, _layermask))
            {
                hits++;
                results += hit.normal;
            }
        }

        results /= hits;

        return results;

    }

    private void OnDrawGizmos()
    {
        //GetCurrentMedians(transform.position, Points, InnerRadius, OuterRadius, OuterDeg, InnerDeg, Lenght, 1);

        for (int i = 0; i < innerRays.Length; i++)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(innerRays[i].origin, innerRays[i].origin + innerRays[i].direction * Lenght);

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(outerRays[i].origin, outerRays[i].origin + outerRays[i].direction * Lenght);
        }
    }
}
