using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ASteeringBehavior
{
    // Base Abstract class for all Boid Steering Behavior 

    public abstract Vector3 CalculateDesiredVelocity(EnemyController _controller, List<EnemyController> _neighbours);
}
