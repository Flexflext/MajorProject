using System.Collections;
using UnityEngine;

public enum ESpiderLegs
{
    LegLF,
    LegLB,
    LegRF,
    LegRB,
}

public class GroundProzeduralAnimation : MonoBehaviour
{
    public bool ShowDebugBool = true;

    [SerializeField] private float legMovementTime;
    [SerializeField] private AnimationCurve legMovementCurve;
    [SerializeField] private float randomPositionRadius;
    [SerializeField] private float maxLegRange;
    [SerializeField] private LayerMask layers;


    [SerializeField] private Transform ikTargetLF;
    [SerializeField] private Transform ikTargetLB;
    [SerializeField] private Transform ikTargetRF;
    [SerializeField] private Transform ikTargetRB;

    [SerializeField] private Transform rayOriginLF;
    [SerializeField] private Transform rayOriginLB;
    [SerializeField] private Transform rayOriginRF;
    [SerializeField] private Transform rayOriginRB;

    [SerializeField] private int legRayNum;
    [SerializeField] private float deg;
    [SerializeField] private float radius;
    [SerializeField] private float length;

    private Transform[] ikTargets;
    private Transform[] animationRaycastOrigins;
    private Vector3[] currentAnimationTargetPosition;
    private Vector3[] nextAnimationTargetPosition;
    private Vector3[] previousAnimationTargetPosition;
    private Vector3[] targetUps;
    private float[] ranges;
    private bool[] moveingLegs;

    private RaycastHit hit;

    private Vector3 bodyNormal;

    private bool moveing;

    private void Start()
    {
        ikTargets = new Transform[] { ikTargetLF, ikTargetLB, ikTargetRF, ikTargetRB };
        animationRaycastOrigins = new Transform[] { rayOriginLF, rayOriginLB, rayOriginRF, rayOriginRB };

        currentAnimationTargetPosition = new Vector3[4];
        nextAnimationTargetPosition = new Vector3[4];
        previousAnimationTargetPosition = new Vector3[4];
        targetUps = new Vector3[4];
        ranges = new float[4];
        moveingLegs = new bool[4];

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
        for (int i = 0; i < animationRaycastOrigins.Length; i++)
        {
            float deltaDeg = 360f / legRayNum;
            float curDeg = 0;

            Vector3 curPoint = Vector3.zero;
            Vector3 closestPoint = Vector3.zero;

            for (int j = 0; j < legRayNum; j++)
            {
                curPoint = Quaternion.AngleAxis(curDeg, animationRaycastOrigins[i].up) * animationRaycastOrigins[i].right;
                curPoint = curPoint * radius;

                Vector3 t = -animationRaycastOrigins[i].up * Mathf.Tan(deg * Mathf.Deg2Rad) * curPoint.magnitude;
                Vector3 dir = (t - curPoint).normalized;

                Debug.DrawLine(animationRaycastOrigins[i].position + curPoint, animationRaycastOrigins[i].position + curPoint + dir * length);

                if (Physics.Raycast(animationRaycastOrigins[i].position + curPoint, dir, out hit, length, layers))
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

            //Debug.DrawRay(animationRaycastOrigins[i].position, animationRaycastOrigins[i].transform.up * -1);

            //if (Physics.Raycast(animationRaycastOrigins[i].position, animationRaycastOrigins[i].transform.up * -1, out hit, float.MaxValue, layers))
            //{
            //    currentAnimationTargetPosition[i] = hit.point;
            //    targetUps[i] = hit.normal;
            //}
        }
    }

    private void SetNewTargetPosition(int _legtomove)
    {
        previousAnimationTargetPosition[_legtomove] = ikTargets[_legtomove].position;
        ikTargets[_legtomove].up = targetUps[_legtomove];
        nextAnimationTargetPosition[_legtomove] = currentAnimationTargetPosition[_legtomove];

    }

    private void CheckRange()
    {
        for (int i = 0; i < ikTargets.Length; i++)
        {
            ranges[i] = (currentAnimationTargetPosition[i] - nextAnimationTargetPosition[i]).sqrMagnitude;

            if (!moveingLegs[i])
            {
                ikTargets[i].position = nextAnimationTargetPosition[i];

                if (ranges[i] >= maxLegRange * maxLegRange)
                {
                    //Check Edge Cases with at Position 0 and half
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
        moveing = true;
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
        bodyNormal = Vector3.Cross((nextAnimationTargetPosition[3] - nextAnimationTargetPosition[0]), (nextAnimationTargetPosition[2] - nextAnimationTargetPosition[1]));
        bodyNormal.Normalize();

        //Will kick me in the ass
        bodyNormal *= -1;

        this.transform.rotation = Quaternion.LookRotation(transform.forward, Vector3.Lerp(this.transform.up, bodyNormal, 20 * Time.deltaTime));
    }



    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        if (!ShowDebugBool) return;


        for (int i = 0; i < currentAnimationTargetPosition.Length; i++)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(currentAnimationTargetPosition[i], 0.1f);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(currentAnimationTargetPosition[i], randomPositionRadius);

            Gizmos.color = Color.black;
            Gizmos.DrawWireSphere(currentAnimationTargetPosition[i], maxLegRange);
        }

        Gizmos.color = Color.magenta;
        Gizmos.DrawRay(this.transform.position, bodyNormal);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(this.transform.position, transform.up);
    }
}
