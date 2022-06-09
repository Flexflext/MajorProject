using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cohesion : ASteeringBehavior
{
    // Claculates the Cohesion for Boids

    public override Vector3 CalculateDesiredVelocity(EnemyController _controller, List<EnemyController> _neighbours)
    {
        // Check if there are any Neigbours
        if (_neighbours.Count == 0)
        {
            return Vector3.zero;
        }

        Vector3 total = Vector3.zero;

        //Check where other Boids are and adds the poition to the total;
        foreach (var neighbour in _neighbours)
        {
            total += neighbour.transform.position;
        }

        // Divides the total Poitions by the neigbour count
        total /= _neighbours.Count;

        // Calculates the Direction between the boid pos and the total pos
        Vector3 dir = total - _controller.transform.position;

        // Returns the Normailied Direction
        return dir.normalized;
    }
}
