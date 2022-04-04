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
    public LayerMask layers;

    public Transform test;

    Vector3[] innerPositions;
    Vector3[] outerPositions;

    Ray[] innerRays = new Ray[0];
    Ray[] outerRays = new Ray[0];

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3[] up = GetCurrentMedians(transform.position, Points, InnerRadius, OuterRadius, OuterDeg, InnerDeg, Lenght, layers);
        Debug.DrawRay(transform.position, up[1]);


        Debug.Log(up[1]);
        Debug.Log("Vorher: " + test.rotation);

        //test.up = up[1];


        if (Input.GetKeyDown(KeyCode.E))
        {
            test.forward = test.right;

            
        }

        this.transform.rotation = Quaternion.LookRotation(transform.forward, Vector3.Lerp(this.transform.up, up[1], 20 * Time.deltaTime));

        Debug.Log("Nachher: " + test.rotation);


        Debug.DrawRay(transform.position, test.up, Color.yellow);
    }

    private Vector3[] GetCurrentMedians(Vector3 _origin, int _points, float _innerr, float _outerr, float _outerdeg, float _innerdeg, float _raylength, LayerMask _layermask)
    {
        Vector3 up = this.transform.up;

        _origin += up * Offset;

        innerPositions = new Vector3[_points];
        outerPositions = new Vector3[_points];

        float deltaDeg = 360f / _points;
        float curDeg = 0;

        Vector3 curPoint = Vector3.zero;



        for (int i = 0; i < _points; i++)
        {
            curPoint.z = Mathf.Sin(curDeg * Mathf.Deg2Rad);
            curPoint.x = Mathf.Cos(curDeg * Mathf.Deg2Rad);

            innerPositions[i] = curPoint * _innerr;
            outerPositions[i] = curPoint * _outerr;


            curDeg += deltaDeg;
        }

        innerRays = new Ray[_points];
        outerRays = new Ray[_points];


        for (int i = 0; i < innerRays.Length; i++)
        {
            //Inner Rays
            Vector3 t = -up * Mathf.Tan(_innerdeg * Mathf.Deg2Rad) * innerPositions[i].magnitude;
            Vector3 dir = (t - innerPositions[i]).normalized;


            //Gizmos.color = Color.green;
            innerRays[i] = new Ray(_origin + innerPositions[i], dir.normalized * _raylength);
            //Gizmos.DrawLine(_origin + innerPositions[i], _origin + innerPositions[i] + dir * _raylength);

            //Outer Rays
            t = -up * Mathf.Tan(_outerdeg * Mathf.Deg2Rad) * outerPositions[i].magnitude;
            dir = (outerPositions[i] - t).normalized;

            //Gizmos.color = Color.cyan;
            outerRays[i] = new Ray(_origin + outerPositions[i], dir.normalized * _raylength);
            //Gizmos.DrawLine(_origin + outerPositions[i], _origin + outerPositions[i] + dir * _raylength);
        }

        Vector3[] results = new Vector3[2];
        int hits = 0;

        for (int i = 0; i < innerRays.Length; i++)
        {
            if (Physics.Raycast(innerRays[i], out RaycastHit hit, _raylength, _layermask))
            {
                hits++;
                results[0] += hit.point;
                results[1] += hit.normal;
            }
        }

        for (int i = 0; i < outerRays.Length; i++)
        {
            if (Physics.Raycast(outerRays[i], out RaycastHit hit, _raylength, _layermask))
            {
                hits++;
                results[0] += hit.point;
                results[1] += hit.normal;
            }
        }

        results[0] /= hits;
        results[1] /= hits;

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
