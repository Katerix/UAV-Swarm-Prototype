using System.Collections.Generic;
using UnityEngine;

public class SeparationBehaviour : MonoBehaviour
{
    public (int, int) fieldBorder = (0, 100);
    public List<Vector2> fieldCorners = new List<Vector2>
    {
        new Vector2(0, 0),
        new Vector2(0, 100),
        new Vector2(100, 0),
        new Vector2(100, 100)
    };

    float speed = 10f;
    float separationDistance = 35f;  // Minimum distance to maintain from other drones
    float separationStrength = 5f;  // Strength of the separation force
    Vector3 targetPosition;
    GameObject[] drones;

    public void Start()
    {
        drones = GameObject.FindGameObjectsWithTag("drone");

        CalculateFurthestCorner();
    }

    public void Update()
    {
        // Combine target direction and separation force
        Vector3 combinedForce = Separation().normalized;

        if (!IsPointInSearchArea(transform.position))
        {
            combinedForce += UpdateDirection().normalized;
        }

        combinedForce = combinedForce.normalized * speed * Time.deltaTime;
        transform.Translate(combinedForce, Space.World);
    }

    Vector3 Separation()
    {
        Vector3 separationForce = Vector3.zero;

        foreach (var drone in drones)
        {
            if (drone != gameObject)
            {
                Vector3 toDrone = transform.position - drone.transform.position;

                if (toDrone.magnitude < separationDistance)
                {
                    separationForce += toDrone.normalized / toDrone.magnitude;
                }
            }
        }

        separationForce *= separationStrength;

        return separationForce;
    }

    void CalculateFurthestCorner()
    {
        float greatestDistance = float.MinValue;

        foreach (var corner in fieldCorners)
        {
            Vector3 cornerPosition = new Vector3(corner.x, transform.position.y, corner.y);
            float tempDistance = Vector3.Distance(new Vector3(corner.x, 0, corner.y), new Vector3(transform.position.x, 0, transform.position.z));

            if (tempDistance > greatestDistance)
            {
                Debug.Log($"x={cornerPosition.x} z={cornerPosition.z}");
                greatestDistance = tempDistance;
                targetPosition = cornerPosition;
            }
        }
    }

    Vector3 UpdateDirection()
    {
        Vector3 targetDirection = new Vector3(targetPosition.x, transform.position.y, targetPosition.z) - transform.position;
        targetDirection.y = 0;  // Maintain the current y level

        return targetDirection;
    }

    bool IsPointInSearchArea(Vector3 point) =>
        point.x >= fieldBorder.Item1 && point.x <= fieldBorder.Item2 &&
        point.z >= fieldBorder.Item1 && point.z <= fieldBorder.Item2;
}