using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour , IStateMachineController
{
    #region Debug

    public enum EEnemyStates
    {
        ES_Walking,
        ES_Idle,
        ES_Attacking,
        ES_AfterAttacking,
    }

    [SerializeField] private EEnemyStates currentState;
    public EEnemyStates CurrentState { get { return currentState; } set { currentState = value; } }   


    #endregion

    #region Properties

    [Header("Walking Properties")]
    [SerializeField] protected Vector2 randomWalkRange;
    [SerializeField] protected float walkPositionYCheckOffset;
    [SerializeField] protected LayerMask walkableLayer;

    [Header("Idle Properties")]
    [SerializeField] protected Vector2 randomIdleTime;

    [Header("Attack Properties")]
    [SerializeField] protected float agressiveRange;
    [SerializeField] protected float attackRange;
    [SerializeField] protected float attackWaitRange;
    [SerializeField] protected float moveAwayFromPlayerMultiplier;

    [Header("After Attack Properties")]
    [SerializeField] protected Vector2 randomIdleAfterAttackTime;
    [SerializeField] protected bool timeReadyAfterAttack;


    private bool canAttack;
    public bool CanAttack { get { return canAttack; } set { canAttack = value; } }

    protected bool isAgressive;
    public bool IsAgressive { get { return isAgressive; } }

    protected NavMeshAgent myAgent;
    public NavMeshAgent Agent { get { return myAgent; } }
    protected bool nextIdleState;

    protected AttackWaitingPosition currentWaitingPosition;

    protected EnemyState currentEnemyState;
    protected Dictionary<EnemyState, Dictionary<StateMachineSwitchDelegate, EnemyState>> stateDictionary;

    #endregion

    #region Unity Methods

    private void Start()
    {
        EnemyManager.Instance.EnemySubscribe(this);
        myAgent = GetComponent<NavMeshAgent>();
        InitializeStateMachine();
    }

    private void Update()
    {
        isAgressive = CheckIfEnemyIsAgressive();
        UpdateStateMachine();
    }

    private void OnDestroy()
    {
        EnemyManager.Instance.EnemyUnSubscribe(this);
    }

    #endregion

    #region Public Methods

    public void InitializeStateMachine()
    {
        EnemyIdleState idleState = new EnemyIdleState(this);
        EnemyWalkingState walkState = new EnemyWalkingState(this, myAgent);
        EnemyAttackState attackState = new EnemyAttackState(this, myAgent);
        EnemyAfterAttackState afterAttackState = new EnemyAfterAttackState(this, myAgent);

        stateDictionary = new Dictionary<EnemyState, Dictionary<StateMachineSwitchDelegate, EnemyState>>
        {
            {
                idleState,
                new Dictionary<StateMachineSwitchDelegate, EnemyState>
                {
                    {() => isAgressive, attackState },
                    {() => !nextIdleState, walkState },
                }
            },
            {
                walkState,
                new Dictionary<StateMachineSwitchDelegate, EnemyState>
                {
                    {() => isAgressive, attackState },
                    {() => nextIdleState, idleState },
                }
            },
            {
                attackState,
                new Dictionary<StateMachineSwitchDelegate, EnemyState>
                {
                    {() => !isAgressive, afterAttackState },
                }
            },
            {
                afterAttackState,
                new Dictionary<StateMachineSwitchDelegate, EnemyState>
                {
                    {() => isAgressive, attackState },
                    {() => timeReadyAfterAttack, idleState },
                }
            },
        };


        currentEnemyState = idleState;
    }

    public void UpdateStateMachine()
    {
        foreach (var transition in stateDictionary[currentEnemyState])
        {
            // Check if the Transition Parameters are given
            if (transition.Key())
            {
                // Exit Current State
                currentEnemyState.ExitState();
                // Enter new State
                transition.Value.EnterState();
                currentEnemyState = transition.Value;

                break;
            }
        }

        currentEnemyState.UpdateState();
    }

    public Vector3 FindNewWalkPosition()
    {
        RaycastHit hit = new RaycastHit();

        Vector3 origin = new Vector3();
        
        float range = Random.Range(randomWalkRange.x, randomWalkRange.y);
        float degree = Random.Range(0.0f, 360.0f);

        origin.z = Mathf.Sin(degree * Mathf.Deg2Rad);
        origin.x = Mathf.Cos(degree * Mathf.Deg2Rad);

        origin = origin * range;
        origin += transform.position + (Vector3.up * walkPositionYCheckOffset);

        if (Physics.Raycast(origin, Vector3.down, out hit, float.MaxValue, walkableLayer))
        {
            if (PositionIsOnNavMesh(hit.point))
            {
                return hit.point;
            }
            else
            {
                return transform.position;
            }        
        }

        return transform.position;
    }

    public void ResetHidingPosition()
    {
        if (currentWaitingPosition != null)
        {
            currentWaitingPosition.inUse = false;
            currentWaitingPosition = null;
        }
    }

    public Vector3 FindNewHidingPosition()
    {
        ResetHidingPosition();

        Vector3 pos = EnemyManager.Instance.FindAttackWaitingPosition(attackWaitRange, transform.position, out currentWaitingPosition);

        if (PositionIsOnNavMesh(pos))
        {
            return pos;
        }

        return transform.position;
    }

    public void ChangeIdleState()
    {
        nextIdleState = !nextIdleState;
    }

    public float GetIdleTimer()
    {
        return Random.Range(randomIdleTime.x, randomIdleTime.y);
    }

    public float GetAfterAttackIdleTime()
    {
        timeReadyAfterAttack = false;
        return Random.Range(randomIdleAfterAttackTime.x, randomIdleAfterAttackTime.y);
    }

    public void SetAttackDoneFlag()
    {
        timeReadyAfterAttack = true;
    }

    public Vector3 FindAttackPosition(int _idx)
    {
        Vector3 pos = EnemyManager.Instance.GetAttackPosition(attackRange, _idx);

        if (PositionIsOnNavMesh(pos))
        {
            if (EnemyManager.Instance.CheckIfPositionCanSeePlayer(pos))
            {
                return pos;
            }
        }


        return transform.position;
    }

    public void StayAwayFromPlayer()
    {
        Vector3 direction = transform.position - EnemyManager.Instance.PlayerPosition;

        if ((direction).sqrMagnitude < attackRange * attackRange)
        {
            myAgent.velocity += direction * moveAwayFromPlayerMultiplier * Time.deltaTime;
        }
    }

    public Quaternion GetLookToPlayerRotation()
    {
        Vector3 playerPosOnMyYPlane = new Vector3(EnemyManager.Instance.PlayerPosition.x, transform.position.y, EnemyManager.Instance.PlayerPosition.z);

        return Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(playerPosOnMyYPlane - transform.position), 20 * Time.deltaTime); ;
    }

    #endregion

    #region Private/Protected Methods

    /// <summary>
    /// Check if a given Poition is on a Nav Mesh
    /// </summary>
    /// <param name="_position"></param>
    /// <returns></returns>
    protected bool PositionIsOnNavMesh(Vector3 _position)
    {
        NavMeshHit hit;

        // Check if hit hit any surface of the NavMesh
        if (NavMesh.SamplePosition(_position, out hit, 3f, NavMesh.AllAreas))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    protected bool CheckIfEnemyIsAgressive()
    {
        if (CheckIfPlayerIsInRange(agressiveRange))
        {
            return true;
        }
        else
        {
            if (isAgressive)
            {
                if (EnemyManager.Instance.CheckIfPositionCanSeePlayer(this.transform.position))
                {
                    return true;
                }
            }
        }

        return false;
    }

    protected bool CheckIfPlayerIsInRange(float _range)
    {
        if ((EnemyManager.Instance.PlayerPosition - transform.position).sqrMagnitude <= _range * _range)
        {
            return true;
        }

        return false;
    }

    #endregion
}
