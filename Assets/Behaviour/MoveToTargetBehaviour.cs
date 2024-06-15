using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Random = UnityEngine.Random;

public class UavBehaviour : MonoBehaviour
{
    DateTime start;
    DateTime end;

    #region Fields

    #region Key Values

    OperationState state = OperationState.HeadingToSearchField;

    UavBehaviour[] drones;
    GameObject[] targets;
    Vector3 SearchFieldPosition;
    Vector3 InitialPosition;

    #endregion

    #region Search Field Related Values
    
    public (int, int) fieldBorder = (0, 100);
    public List<Vector2> fieldCorners = new List<Vector2>
    {
        new Vector2(0, 0),
        new Vector2(0, 100),
        new Vector2(100, 0),
        new Vector2(100, 100)
    };

    bool HasEnteredField = false;

    #endregion

    #region Control Parameters

    float speed = 10f;
    float separationDistance;
    float separationStrength;
    float availabilityRadius = 45f;

    #endregion

    #region Search and Detection Related Values

    public static List<Vector2> foundTargets = new List<Vector2>();
    float detectionRadius = 1f;
    static int failureCount = 0;
    TimeSpan failureTimeLimit = TimeSpan.FromSeconds(25);
    DateTime lastTargetFoundTime;
    DateTime lastRotationTime = DateTime.Now; 
    Vector3 currentSearchDirection = Vector3.zero;

    #endregion

    #endregion

    #region MonoBehaviour Implementation

    public void Start()
    {
        InitialPosition = transform.position;

        drones = FindObjectsOfType<UavBehaviour>();
        targets = GameObject.FindGameObjectsWithTag("goal");

        CalculateFurthestCorner();
    }

    public void Update()
    {
        CheckState();

        transform.Translate(speed * Time.deltaTime * UpdateDirection(), Space.World);

        if (DetectedTarget())
        {
            lastTargetFoundTime = DateTime.UtcNow;
        }
    }

    #endregion

    #region Decision-making Methods

    void CheckState()
    {
        HasEnteredField = IsInSearchArea();

        switch (state)
        {
            case OperationState.HeadingToSearchField:
                foreach (var drone in drones)
                {
                    if (!drone.HasEnteredField)
                    {
                        return;
                    }
                }
                Debug.Log("Start searching..");
                start = DateTime.UtcNow;

                state = OperationState.PerformingSearch;
                lastTargetFoundTime = DateTime.UtcNow;
                return;

            case OperationState.PerformingSearch:
                if (failureCount >= 15)
                {
                    foreach (var drone in drones)
                    {
                        drone.state = OperationState.ReturningBack;
                    }
                    end = DateTime.UtcNow;
                    Debug.Log($"Returning... Found {foundTargets.Count} targets. Time spent: {(end - start)}");
                }
                else if (DateTime.UtcNow - lastTargetFoundTime > failureTimeLimit)
                {
                    Interlocked.Increment(ref failureCount);
                }
                return;

            case OperationState.ReturningBack:
                if (transform.position == InitialPosition)
                {
                    state = OperationState.Stopped;
                }
                return;
        }
    }

    #endregion

    #region Other Methods

    Vector3 UpdateDirection()
    {
        separationStrength = 10f;
        separationDistance = 30f;
        Vector3 combinedForce = SeparationForce().normalized;

        switch (state)
        {
            case OperationState.HeadingToSearchField:
                combinedForce += SearchFieldDirectionForce().normalized;
                break;

            case OperationState.PerformingSearch:
                if (!HasEnteredField)
                {
                    CalculateFurthestCorner();
                    combinedForce += SearchFieldDirectionForce().normalized;
                }
                else
                {
                    speed = 15f;
                    separationDistance = 10f;
                    separationStrength = 10f;
                    combinedForce += LevyFlightForce().normalized;
                }
                break;

            case OperationState.ReturningBack:
                speed = 15f;
                combinedForce += InitialPosition - transform.position;
                break;
        }

        return combinedForce.normalized;
    }

    Vector3 SearchFieldDirectionForce()
    {
        Vector3 targetDirection = new Vector3(SearchFieldPosition.x, transform.position.y, SearchFieldPosition.z) - transform.position;
        targetDirection.y = 0;

        return targetDirection;
    }

    Vector3 SeparationForce()
    {
        Vector3 separationForce = Vector3.zero;

        foreach (var drone in drones)
        {
            if (drone.gameObject != gameObject)
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

    Vector3 LevyFlightForce()
    {
        TimeSpan cooldownTime = TimeSpan.FromSeconds(1);

        if (DateTime.Now - lastRotationTime >= cooldownTime)
        {
            float stepLength = 5 * Random.Range(2f, failureCount);
            float angle = Random.Range(0, 2 * Mathf.PI);

            currentSearchDirection = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * stepLength;

            lastRotationTime = DateTime.Now;
        }

        return currentSearchDirection;
    }

    void CalculateFurthestCorner()
    {
        float greatestDistance = float.MinValue;

        foreach (var corner in fieldCorners)
        {
            Vector3 cornerPosition = new Vector3(corner.x, transform.position.y, corner.y);

            float tempDistance = Vector3.Distance(
                new Vector3(corner.x, 0, corner.y),
                new Vector3(transform.position.x, 0, transform.position.z));

            if (tempDistance > greatestDistance)
            {
                greatestDistance = tempDistance;
                SearchFieldPosition = cornerPosition;
            }
        }
    }

    bool DetectedTarget()
    {
        GameObject found = null;

        foreach (var target in targets)
        {
            if (target != null && IsFound(target))
            {
                //Debug.Log($"Target found! x:{target.transform.position.x}, y:{target.transform.position.y}, z:{target.transform.position.z}");
                found = target;
                break;
            }
        }

        if (found == null)
        {
            return false;
        }

        foundTargets.Add(found.transform.position);
        Destroy(found);

        return true;
    }

    bool IsInSearchArea()
    {
        if (transform.position.x >= fieldBorder.Item1 && transform.position.x <= fieldBorder.Item2 &&
            transform.position.z >= fieldBorder.Item1 && transform.position.z <= fieldBorder.Item2)
        {
            return true;
        }

        return false;
    }

    bool IsFound(GameObject potentialTarget)
    {
        var etherPosition = potentialTarget.transform.position;

        if (etherPosition.x - transform.position.x < detectionRadius &&
            etherPosition.x - transform.position.x > -detectionRadius &&
            etherPosition.z - transform.position.z < detectionRadius &&
            etherPosition.x - transform.position.x > -detectionRadius)
        {
            return true;
        }

        return false;
    }

    #endregion
}

public enum OperationState
{
    HeadingToSearchField,
    PerformingSearch,
    ReturningBack,
    Stopped
}