using System.Collections;
using UnityEngine;

public class BoidBehaviour : MonoBehaviour
{
    public float travelSpeed = 5f;
    public float searchSpeed = 2f;
    public float fieldSize = 100f;
    public Vector2 gridPosition; // Position in the grid
    private Vector3 targetPosition;
    private bool isSearching = false;
    private Vector3 searchFieldCenter = new Vector3(0, 0, 0); // Center of the search field
    private float searchFieldRadius = 50f; // Radius of the search field

    void Start()
    {
        SetInitialPosition();
        StartCoroutine(MoveToSearchField());
    }

    void Update()
    {
        if (isSearching)
        {
            MoveTowardsTarget();
        }
    }

    void SetInitialPosition()
    {
        // Convert grid position to world position within the search field
        float cellSize = fieldSize / 10f;
        float x = (gridPosition.x * cellSize) - (fieldSize / 2f) + (cellSize / 2f);
        float z = (gridPosition.y * cellSize) - (fieldSize / 2f) + (cellSize / 2f);
        targetPosition = new Vector3(x, 0, z);
    }

    IEnumerator MoveToSearchField()
    {
        while (Vector3.Distance(transform.position, searchFieldCenter) > searchFieldRadius)
        {
            transform.position = Vector3.MoveTowards(transform.position, searchFieldCenter, travelSpeed * Time.deltaTime);
            yield return null;
        }

        isSearching = true;
        StartCoroutine(SearchRoutine());
    }

    void MoveTowardsTarget()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, searchSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            SetNewTargetPosition();
        }
    }

    void SetNewTargetPosition()
    {
        float cellSize = fieldSize / 10f;
        float x = Random.Range(-cellSize / 2f, cellSize / 2f);
        float z = Random.Range(-cellSize / 2f, cellSize / 2f);
        targetPosition = new Vector3(transform.position.x + x, 0, transform.position.z + z);
    }

    IEnumerator SearchRoutine()
    {
        while (isSearching)
        {
            // Perform a Levy Flight step
            float stepLength = Random.Range(1f, 10f);
            float angle = Random.Range(0, 2 * Mathf.PI);
            Vector3 levyStep = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * stepLength;
            targetPosition = transform.position + levyStep;

            // Clamp target position within the grid cell
            float cellSize = fieldSize / 10f;
            targetPosition.x = Mathf.Clamp(targetPosition.x, gridPosition.x * cellSize - cellSize / 2f, gridPosition.x * cellSize + cellSize / 2f);
            targetPosition.z = Mathf.Clamp(targetPosition.z, gridPosition.y * cellSize - cellSize / 2f, gridPosition.y * cellSize + cellSize / 2f);

            yield return new WaitForSeconds(1f); // Adjust this value for the search interval
        }
    }
}