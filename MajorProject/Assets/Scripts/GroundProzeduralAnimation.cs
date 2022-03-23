using System.Collections;
using System.Collections.Generic;
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


    [SerializeField] private Transform ikTargetLF;
    [SerializeField] private Transform ikTargetLB;
    [SerializeField] private Transform ikTargetRF;
    [SerializeField] private Transform ikTargetRB;

    [SerializeField] private Transform rayOriginLF;
    [SerializeField] private Transform rayOriginLB;
    [SerializeField] private Transform rayOriginRF;
    [SerializeField] private Transform rayOriginRB;


    private Transform[] ikTargets;
    private Transform[] animationRaycastOrigins;
    private Vector3[] currentAnimationTargetPosition;
    private Vector3[] nextAnimationTargetPosition;
    private Vector3[] previousAnimationTargetPosition;
    private float[] ranges;
    private bool[] moveingLegs;
     
    private RaycastHit hit;

    private void Start()
    {
        ikTargets = new Transform[]{ ikTargetLF, ikTargetLB, ikTargetRF, ikTargetRB};
        animationRaycastOrigins = new Transform[]{ rayOriginLF, rayOriginLB, rayOriginRF, rayOriginRB };

        currentAnimationTargetPosition = new Vector3[4];
        nextAnimationTargetPosition = new Vector3[4];
        previousAnimationTargetPosition = new Vector3[4];
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
        //MoveLeg();
    }

    private void CalculateTargetPosition()
    {
        for (int i = 0; i < animationRaycastOrigins.Length; i++)
        {
            if (Physics.Raycast(animationRaycastOrigins[i].position, Vector3.down, out hit))
            {
                currentAnimationTargetPosition[i] = hit.point;
            }
        }
    }

    private void SetNewTargetPosition(int _legtomove)
    {
        Vector3 randomizer = new Vector3(Random.Range(-randomPositionRadius, randomPositionRadius), 3, Random.Range(-randomPositionRadius, randomPositionRadius));

        if (Physics.Raycast(currentAnimationTargetPosition[_legtomove] + randomizer, Vector3.down, out hit))
        {
            previousAnimationTargetPosition[_legtomove] = ikTargets[_legtomove].position;
            nextAnimationTargetPosition[_legtomove] = hit.point;
        }

    }

    private void CheckRange()
    {
        for (int i = 0; i < ikTargets.Length; i++)
        {
            ranges[i] = (currentAnimationTargetPosition[i] - nextAnimationTargetPosition[i]).sqrMagnitude;

            if (ranges[i] >= maxLegRange * maxLegRange)
            {
                if (!moveingLegs[i])
                {
                    //switch (i)
                    //{
                    //    case 3:
                    //        {
                    //            if (moveingLegs[2] || moveingLegs[1])
                    //            {
                    //                ikTargets[i].position = nextAnimationTargetPosition[i];
                    //            }
                    //        }
                    //        break;
                    //    case 1:
                    //        {
                    //            if (moveingLegs[3] || moveingLegs[0])
                    //            {
                    //                ikTargets[i].position = nextAnimationTargetPosition[i];
                    //            }
                    //        }
                    //        break;
                    //    default:
                    //        break;
                    //}

                    moveingLegs[i] = true;
                    SetNewTargetPosition(i);
                    StartCoroutine(C_MoveLegCoroutine(i));
                } 
            }

            if (!moveingLegs[i])
            {
                ikTargets[i].position = nextAnimationTargetPosition[i];
            }


            
        }
    }

    private void MoveLeg()
    {
        for (int i = 0; i < moveingLegs.Length; i++)
        {
            if (moveingLegs[i])
            {
               

                ikTargets[i].position = Vector3.MoveTowards(ikTargets[i].position, nextAnimationTargetPosition[i], legMovementTime * Time.deltaTime);

                if ((nextAnimationTargetPosition[i] - ikTargets[i].position).sqrMagnitude <= 0.02f * 0.02f)
                {
                    moveingLegs[i] = false;
                }
            }
            else
            {
                
            }
        }
    }

    private IEnumerator C_MoveLegCoroutine(int _leg)
    {
        float passedTime = 0f;

        while ((nextAnimationTargetPosition[_leg] - ikTargets[_leg].position).sqrMagnitude <= 0.02f * 0.02f)
        {
            ikTargets[_leg].position = Vector3.MoveTowards(ikTargets[_leg].position, nextAnimationTargetPosition[_leg], passedTime / legMovementTime) + legMovementCurve.Evaluate(passedTime /legMovementTime) * Vector3.up;

            passedTime += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        moveingLegs[_leg] = false;
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
    }
}
