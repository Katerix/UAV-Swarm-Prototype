using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public List<NodeBehaviour> nodes = new List<NodeBehaviour>();

    void Start()
    {
        // пошук всіх вузлів мережі
        NodeBehaviour[] nodeArray = FindObjectsOfType<NodeBehaviour>();

        nodes.AddRange(nodeArray);

        for (int i = 0; i < nodes.Count; i++)
        {
            nodes[i].ipAddress = new IPAddress(new byte[] { 100, 100, 100, (byte)i });
        }
    }
}