using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtraMath
{
    // Extra Math needed for world velo to local velo

    /// <summary>
    /// Gets a Degree from a Vector2
    /// </summary>
    /// <param name="_originvec"></param>
    /// <returns></returns>
    public static float GetDegFromVec(Vector2 _originvec)
    {
        _originvec.Normalize();
        // P.x = cos(deg)
        // p.y = sin(deg)
        float degFromx = Mathf.Rad2Deg * Mathf.Acos(_originvec.x);

        // Check if over 180 degree
        if (_originvec.y < 0)
        {
            degFromx = 360 - degFromx;
        }

        // Return Degree
        return degFromx;
    }

    /// <summary>
    /// Get a Vector 2 from a degree
    /// </summary>
    /// <param name="_deg"></param>
    /// <returns></returns>
    public static Vector2 GetVecFromDeg(float _deg)
    {
        // P.x = cos(deg)
        // p.y = sin(deg)
        float rad = _deg * Mathf.Deg2Rad;
        Vector2 vec = Vector2.zero;
        vec.x = Mathf.Cos(rad);
        vec.y = Mathf.Sin(rad);

        return vec;
    }

    public static Vector2 GetAnimatorVeloFromAgent(Vector2 _righttransformvec, Vector2 _agentvelovec)
    {
        if (_agentvelovec.x == 0 && _agentvelovec.y == 0)
        {
            return Vector2.zero;
        }

        // Veloxity from World to Local
        float localDeg = GetDegFromVec(new Vector2(_righttransformvec.x, _righttransformvec.y));
        float worldDeg = GetDegFromVec(new Vector2(_agentvelovec.x, _agentvelovec.y));

        float worldToLocalDeg = worldDeg - localDeg;

        // Check that no minus Deg
        if (worldToLocalDeg <= 0) worldToLocalDeg += 360;

        // realtiv Velocity
        return GetVecFromDeg(worldToLocalDeg); ;
    }
}
