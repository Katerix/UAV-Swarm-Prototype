using UnityEngine;

public class TargetMovement : MonoBehaviour
{
    float speed = 5f;
    float maxChangeInterval = 5f;
    float maxDistance = 50f; // Maximum allowed distance from the starting point

    Vector3 startPosition;
    Vector3 randomDirection;
    float changeInterval;

    void Start()
    {
        startPosition = transform.position; // Record the starting position
        randomDirection = GetRandomGroundDirection();
        changeInterval = Random.Range(0.1f, maxChangeInterval);
    }

    void Update()
    {
        Vector3 nextPosition = transform.position + speed * Time.deltaTime * randomDirection;

        // Check if the next position is within the allowed distance
        if (Vector3.Distance(startPosition, nextPosition) <= maxDistance)
        {
            transform.Translate(speed * Time.deltaTime * randomDirection);
        }
        else
        {
            // Change direction if the target is about to move out of bounds
            randomDirection = GetRandomGroundDirection();
            changeInterval = Random.Range(0.1f, maxChangeInterval);
        }

        changeInterval -= Time.deltaTime;

        if (changeInterval <= 0f)
        {
            randomDirection = GetRandomGroundDirection();
            changeInterval = Random.Range(0.1f, maxChangeInterval);
        }

        //Debug.Log($"Target: x={transform.position.x} y={transform.position.y} z={transform.position.z}");
    }

    Vector3 GetRandomGroundDirection()
    {
        Vector2 randomDirection2D = Random.insideUnitCircle.normalized;
        return new Vector3(randomDirection2D.x, 0f, randomDirection2D.y).normalized;
    }
}
