using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Alignment : ASteeringBehavior
{
    // Claculates the Amount of Velocity needed to Align the boids

    public override Vector3 CalculateDesiredVelocity(EnemyController _controller, List<EnemyController> _neighbours)
    {
        // Check if there are even Neigbours
        if (_neighbours.Count == 0)
        {
            return Vector3.zero;
        }

        Vector3 total = Vector3.zero;

        // Veloxity dependent on the Amount of Neighbours and their veloxity
        foreach (var neighbour in _neighbours)
        {
            total += neighbour.Agent.velocity;
        }

        // Diveide total calculated velocity through amount of Neigbours
        total /= _neighbours.Count;

        // Return normilized
        return total.normalized;
    }
}
