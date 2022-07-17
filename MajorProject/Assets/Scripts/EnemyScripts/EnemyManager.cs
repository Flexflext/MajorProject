using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    [SerializeField] private Transform playerTransform;
    public Vector3 PlayerPosition => playerTransform.position;
    public Transform Player => playerTransform;

    [SerializeField] private float enemyPlayerRange = 75f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private int maxNumerOfattackingEnemies = 3;
    [SerializeField] private int currentNumberOfAttackingEnemies = 0;
    private float currentAngleBetweenAttackingEnemies = 0;

    [Header("Boid Settings")]
    [SerializeField] private float boidViewAngle;
    [SerializeField] private float boidViewRange;
    [SerializeField] private float boidSpeed;
    [SerializeField] private float boidSeperationAmountMultiplier;
    private Vector3 desiredVelocity;
    public float BoidSeperationAmountMultiplier => boidSeperationAmountMultiplier;

    private List<EnemyController> enemyControllers = new List<EnemyController>();

    private List<EnemyController> neighbours = new List<EnemyController>();

    private List<EnemyController> attackingEnemies = new List<EnemyController>();
    private List<AttackWaitingPosition> attackingWaitingPositions = new List<AttackWaitingPosition>();

    // Boid Steering Behavior
    private ASteeringBehavior[] behaviors = new ASteeringBehavior[]
    {
        new Seperation(),
        new Cohesion(),
        new Alignment(),
    };

    private void Awake()
    {
        if (EnemyManager.Instance == null)
        {
            EnemyManager.Instance = this;
        }
        else if (EnemyManager.Instance != this)
        {
            Destroy(this.gameObject);
        }
    }

    private void Update()
    {
        //// Check if Enemys are Close to Player and disables the Enemys GFXs if the Player is to far away
        //foreach (EnemyController controller in enemyControllers)
        //{
        //    if (PosCloseToPlayerPosition(controller.transform.position))
        //    {
        //        controller.GFXs.SetActive(true);
        //    }
        //    else
        //    {
        //        controller.GFXs.SetActive(false);
        //    }
        //}


        // Check if Other Enemys are Close by
        BoidBehavior();
    }

    private void BoidBehavior()
    {
        //Boids Behavior
        foreach (EnemyController controller in enemyControllers)
        {
            // Check if Enemy might be inactive
            if (!controller.gameObject.activeSelf)
            {
                continue;
            }

            // Checks that the Enemy is not agressive
            if (!controller.IsAgressive)
            {
                // Count Neigbours of the Contoller
                foreach (EnemyController neighbour in enemyControllers)
                {
                    // Check that the Neigbour is not Inactive
                    if (!neighbour.gameObject.activeSelf)
                    {
                        continue;
                    }

                    // Check that the Neibour is not the Enemy itself
                    if (neighbour != controller)
                    {
                        // Calculate the direction between the Neighbour and the Enemy
                        Vector3 direction = GetDirectionBetweenPositions(controller.transform.position, neighbour.transform.position);

                        //Check if the neighbour is in Range
                        if (direction.sqrMagnitude < boidViewRange * boidViewRange)
                        {
                            // Checkj if the Neigbour is in given view Angle
                            if (Vector3.Angle(controller.transform.forward, direction) < boidViewAngle)
                            {
                                // Check if the Neigbour is agressive
                                if (neighbour.IsAgressive)
                                {
                                    neighbours.Add(neighbour);
                                }
                            }
                        }
                    }
                }

                desiredVelocity = Vector3.zero;

                // Calculate desired Velocity with all given Steeringbehaviors
                foreach (ASteeringBehavior behavior in behaviors)
                {
                    desiredVelocity += behavior.CalculateDesiredVelocity(controller, neighbours);
                }

                //Check that there are Neigbours
                if (neighbours.Count != 0)
                {
                    // Adds the calculated desired velocity to the agents velocity
                    controller.Agent.velocity += (controller.Agent.velocity + desiredVelocity) * boidSpeed * Time.deltaTime;
                }

                neighbours.Clear();
            }
        }
    }

    /// <summary>
    /// Checks if any Enemy is in Range of a given Position
    /// </summary>
    /// <param name="_pos"></param>
    /// <param name="_range"></param>
    /// <param name="_enemyPosition"></param>
    /// <returns></returns>
    public bool EnemyInRange(Vector3 _pos, float _range, out Vector3 _enemyPosition)
    {
        bool enemyInRange = false;
        _enemyPosition = _pos;

        // Check if any Enemy is near the Position
        foreach (EnemyController controller in enemyControllers)
        {
            if (GetDirectionBetweenPositions(_pos, controller.transform.position).sqrMagnitude <= (_range * _range))
            {
                enemyInRange = true;
                _enemyPosition = controller.transform.position;
                break;
            }
        }

        // Returns if any Enemy is Close by
        return enemyInRange;
    }

    /// <summary>
    /// Check if any Enemy can be seen from a given Position
    /// </summary>
    /// <param name="_pos"></param>
    /// <returns></returns>
    public bool CheckIfCanSeeAnyEnemy(Vector3 _pos)
    {
        RaycastHit hit;

        bool canSeeEnemy = false;

        // Check if any Enemy can be Seen
        foreach (EnemyController controller in enemyControllers)
        {
            // Raycast from pos to Enemy
            if (Physics.Raycast(_pos, GetDirectionBetweenPositions(_pos, controller.transform.position), out hit, 75f))
            {
                // Chack if Raycast hit the Enemy layer
                if (hit.collider.gameObject.layer == 7)
                {
                    canSeeEnemy = true;
                    break;
                }
            }
        }

        // Return if any Enemy can be Seen
        return canSeeEnemy;
    }

    /// <summary>
    /// Check if any Enemy can be seen from a given Position
    /// </summary>
    /// <param name="_pos"></param>
    /// <returns></returns>
    public bool CheckIfPositionCanSeePlayer(Vector3 _pos)
    {

        bool canSeeEnemy = false;

        // Raycast from pos to Enemy
        if (Physics.Raycast(_pos, GetDirectionBetweenPositions(_pos, PlayerPosition), out RaycastHit hit, float.MaxValue))
        {
            if (hit.collider.gameObject.layer == 3)
            {
                canSeeEnemy = true;
            }
        }

        // Return if any Player can be seen
        return canSeeEnemy;
    }

    /// <summary>
    /// Check if Position is Close to Player
    /// </summary>
    /// <param name="_pos"></param>
    /// <returns></returns>
    public bool PosCloseToPlayerPosition(Vector3 _pos)
    {
        return (PlayerPosition - _pos).sqrMagnitude <= (enemyPlayerRange * enemyPlayerRange);
    }

    /// <summary>
    /// Calculates a Direction between two Vectors
    /// </summary>
    /// <param name="_start"></param>
    /// <param name="_end"></param>
    /// <returns></returns>
    private Vector3 GetDirectionBetweenPositions(Vector3 _start, Vector3 _end)
    {
        Vector3 direction;
        direction = _end - _start;
        return direction;
    }

    public Vector3 FindAttackWaitingPosition(float _range, Vector3 _enemyposition, out AttackWaitingPosition _curattackwaitingposition)
    {
        List<AttackWaitingPosition> currentattackingWaitingPositions = new List<AttackWaitingPosition>();
        float sqrdrange = _range * _range;


        for (int i = 0; i < attackingWaitingPositions.Count; i++)
        {
            if (attackingWaitingPositions[i].inUse)
            {
                continue;
            }

            if ((attackingWaitingPositions[i].transform.position - PlayerPosition).sqrMagnitude <= sqrdrange)
            {
                currentattackingWaitingPositions.Add(attackingWaitingPositions[i]);
            }
        }

        currentattackingWaitingPositions.OrderBy(x => (x.transform.position - PlayerPosition).sqrMagnitude).ToList();

        for (int i = currentattackingWaitingPositions.Count - 1; i >= 0; i--)
        {
            if (!CheckIfPositionCanSeePlayer(currentattackingWaitingPositions[i].transform.position))
            {
                currentattackingWaitingPositions[i].inUse = true;
                _curattackwaitingposition = currentattackingWaitingPositions[i];
                return currentattackingWaitingPositions[i].transform.position;
            }
        }

        Vector3 newPos = PlayerPosition + ((_enemyposition - PlayerPosition).normalized * _range);

        _curattackwaitingposition = null;
        return newPos;
    }

    private void CheckCanCanAttack(EnemyController _controller)
    {
        _controller.CanAttack = true;
        currentNumberOfAttackingEnemies++;
    }

    private void CheckIfOthersCanAttack(EnemyController _controller)
    {
        currentNumberOfAttackingEnemies--;
        

        if (currentNumberOfAttackingEnemies < maxNumerOfattackingEnemies)
        {
            for (int i = 0; i < attackingEnemies.Count; i++)
            {
                if (attackingEnemies[i].CanAttack == false)
                {  
                    CheckCanCanAttack(attackingEnemies[i]);
                    break;
                }
            }
        }
        _controller.CanAttack = false;
    }

    public Vector3 GetAttackPosition(float _range, int _idx)
    {
        Vector3 origin = Vector3.zero;

        currentAngleBetweenAttackingEnemies = (360f / attackingEnemies.Count) * _idx;

        origin.z = Mathf.Sin(currentAngleBetweenAttackingEnemies * Mathf.Deg2Rad);
        origin.x = Mathf.Cos(currentAngleBetweenAttackingEnemies * Mathf.Deg2Rad);

        origin.z *= _range;
        origin.x *= _range;


        origin += PlayerPosition;

        return origin;
    }


    /// <summary>
    /// Subscribe to the List of Enemys
    /// </summary>
    /// <param name="_controller"></param>
    public void EnemySubscribe(EnemyController _controller)
    {
        // Check if Enemy is already in List
        if (!enemyControllers.Contains(_controller))
        {
            enemyControllers.Add(_controller);
        }
    }

    /// <summary>
    /// Unsubscribe to the List of Enemys
    /// </summary>
    /// <param name="_controller"></param>
    public void EnemyUnSubscribe(EnemyController _controller)
    {
        // Check if Enemy is already in List
        if (enemyControllers.Contains(_controller))
        {
            enemyControllers.Remove(_controller);
            UnSubscribeToAttacking(_controller);
        }
    }

    public int SubscribeToAttacking(EnemyController _controller)
    {
        // Check if Enemy is already in List
        if (!attackingEnemies.Contains(_controller))
        {
            attackingEnemies.Add(_controller);
            if (currentNumberOfAttackingEnemies < maxNumerOfattackingEnemies)
            {
                CheckCanCanAttack(_controller);
            }
            return attackingEnemies.IndexOf(_controller);
        }

        return 0;
    }

    /// <summary>
    /// Unsubscribe to the List of Attacking Enemys
    /// </summary>
    /// <param name="_controller"></param>
    public void UnSubscribeToAttacking(EnemyController _controller)
    {
        // Check if Enemy is already in List
        if (attackingEnemies.Contains(_controller))
        {
            
            attackingEnemies.Remove(_controller);
            CheckIfOthersCanAttack(_controller);
        }
    }

    public void AttackWaitingPositionSubscribe(AttackWaitingPosition _position)
    {
        if (!attackingWaitingPositions.Contains(_position))
        {
            attackingWaitingPositions.Add(_position);
        }
    }

    public void AttackWaitingPositionUnSubscribe(AttackWaitingPosition _position)
    {
        if (attackingWaitingPositions.Contains(_position))
        {
            attackingWaitingPositions.Remove(_position);
        }
    }
}
