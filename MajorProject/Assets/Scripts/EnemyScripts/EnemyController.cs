using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;
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
    [SerializeField] protected GameObject bulletPrefab;
    [SerializeField] protected Transform barrel;
    [SerializeField] protected float bulletSpeed;
    [SerializeField] protected float bulletUpMultiplier;
    [SerializeField] protected float playerDeltaDistranceToAttackPos;

    [Header("After Attack Properties")]
    [SerializeField] protected Vector2 randomIdleAfterAttackTime;
    [SerializeField] protected bool timeReadyAfterAttack;


    private bool canAttack;
    public bool CanAttack { get { return canAttack; } set { canAttack = value; } }

    protected bool isAgressive;
    public bool IsAgressive { get { return isAgressive; } set { isAgressive = value; } }

    protected NavMeshAgent myAgent;
    public NavMeshAgent Agent { get { return myAgent; } }
    protected bool nextIdleState;

    protected AttackWaitingPosition currentWaitingPosition;
    protected Animator enemyAnimator;
    protected Rig rig;

    protected bool isDead;
    protected EnemyHealth enemyHealth;

    protected EnemyState currentEnemyState;
    protected Dictionary<EnemyState, Dictionary<StateMachineSwitchDelegate, EnemyState>> stateDictionary;

    #endregion

    #region Unity Methods

    private void Start()
    {
        EnemyManager.Instance.EnemySubscribe(this);
        myAgent = GetComponent<NavMeshAgent>();
        enemyHealth = GetComponent<EnemyHealth>();
        enemyHealth.SubToOnDeath(OnDeath);
        enemyAnimator = GetComponentInChildren<Animator>();
        rig = GetComponentInChildren<Rig>();
        InitializeStateMachine();

        MultiAimConstraint[] aim = GetComponentsInChildren<MultiAimConstraint>();
        RigBuilder builder = GetComponentInChildren<RigBuilder>();

        for (int i = 0; i < aim.Length; i++)
        {
            aim[i].data.sourceObjects = new WeightedTransformArray { new WeightedTransform(EnemyManager.Instance.Player, 1) };
        }

        builder.Build();
    }

    private void Update()
    {
        SetAnimationVelo();
        isAgressive = CheckIfEnemyIsAgressive();
        UpdateStateMachine();
    }

    private void OnDestroy()
    {
        enemyHealth.UnsubToOnDeath(OnDeath);
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
        EnemyDeathState deathState = new EnemyDeathState(this);

        stateDictionary = new Dictionary<EnemyState, Dictionary<StateMachineSwitchDelegate, EnemyState>>
        {
            {
                idleState,
                new Dictionary<StateMachineSwitchDelegate, EnemyState>
                {
                    {() => isDead, deathState },
                    {() => isAgressive, attackState },
                    {() => !nextIdleState, walkState },
                }
            },
            {
                walkState,
                new Dictionary<StateMachineSwitchDelegate, EnemyState>
                {
                    {() => isDead, deathState },
                    {() => isAgressive, attackState },
                    {() => nextIdleState, idleState },
                }
            },
            {
                attackState,
                new Dictionary<StateMachineSwitchDelegate, EnemyState>
                {
                    {() => isDead, deathState },
                    {() => !isAgressive, afterAttackState },
                }
            },
            {
                afterAttackState,
                new Dictionary<StateMachineSwitchDelegate, EnemyState>
                {
                    {() => isDead, deathState },
                    {() => isAgressive, attackState },
                    {() => timeReadyAfterAttack, idleState },
                }
            },
            {
                deathState,
                new Dictionary<StateMachineSwitchDelegate, EnemyState>
                {
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
        //ResetHidingPosition();

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

        if ((pos - transform.position).sqrMagnitude >= playerDeltaDistranceToAttackPos)
        {
            if (PositionIsOnNavMesh(pos))
            {
                if (EnemyManager.Instance.CheckIfPositionCanSeePlayer(pos))
                {
                    return pos;
                }
            }
        }

        return transform.position;
    }

    public void StayAwayFromPlayer(float _multiplier)
    {
        Vector3 direction = transform.position - EnemyManager.Instance.PlayerPosition;

        if ((direction).sqrMagnitude < (attackRange * attackRange) * _multiplier)
        {
            myAgent.velocity += direction * moveAwayFromPlayerMultiplier * Time.deltaTime;
        }
    }

    public Quaternion GetLookToPlayerRotation()
    {
        Vector3 playerPosOnMyYPlane = new Vector3(EnemyManager.Instance.PlayerPosition.x, transform.position.y, EnemyManager.Instance.PlayerPosition.z);

        return Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(playerPosOnMyYPlane - transform.position), 20 * Time.deltaTime); ;
    }

    public void Shoot()
    {
        Vector3 velo = ((EnemyManager.Instance.PlayerPosition - barrel.transform.position).normalized + barrel.up * bulletUpMultiplier) * bulletSpeed;

        Rigidbody bulletRB = Instantiate(bulletPrefab, barrel.position, Quaternion.identity).GetComponent<Rigidbody>();

        bulletRB.AddForce(velo, ForceMode.Impulse);
    }

    public void SetAttackAnimations(int _setto)
    {
        enemyAnimator.SetLayerWeight(1, _setto);
        rig.weight = _setto;
    } 

    public void Die()
    {
        Destroy(this.gameObject);
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

    protected void SetAnimationVelo()
    {
        Vector2 velo = ExtraMath.GetAnimatorVeloFromAgent(new Vector2(transform.right.x, transform.right.z), new Vector2(myAgent.velocity.x, myAgent.velocity.z));

        enemyAnimator.SetFloat("XVelo", velo.x);
        enemyAnimator.SetFloat("YVelo", velo.y);
    }

    protected void OnDeath()
    {
        //Disable Colliders
        if (!isDead)
        {
            isDead = true;
            myAgent.isStopped = true;
            enemyAnimator.SetTrigger("isDead");
            HUD.Instance.KillObjAnim();
        }

        
    }

    #endregion
}
