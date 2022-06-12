using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seperation : ASteeringBehavior
{
    // Steering Behavior for Seperation of Boids

    // Calculates the Velocity for the Boid to be Seperated from its Neigbours
    public override Vector3 CalculateDesiredVelocity(EnemyController _controller, List<EnemyController> _neighbours)
    {
        // Check that there ary neighbours
        if (_neighbours.Count == 0)
        {
            return Vector3.zero;
        }

        Vector3 total = Vector3.zero;

        // Calculate the desired Velo dependent on the Neigbours
        foreach (EnemyController neighbour in _neighbours)
        {
            total += (neighbour.transform.position - _controller.transform.position).normalized;
        }

        // Divide total by Amount of Neigbours
        total /= _neighbours.Count;

        // times minus on to Seperate
        total *= -1;

        // Return normalized * Seperation Amount Multiplier to Controll the Distance
        return total.normalized * EnemyManager.Instance.BoidSeperationAmountMultiplier;
    }
}
