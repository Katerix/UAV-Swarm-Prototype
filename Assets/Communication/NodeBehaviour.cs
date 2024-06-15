using System.Collections.Generic;
using System.Net;
using UnityEngine;

[RequireComponent(typeof(BoidBehaviour))]
public class NodeBehaviour : MonoBehaviour
{
    public IPAddress ipAddress;
    public float availabilityRadius = 20f;
    public float broadcastInterval = 3;

    private Dictionary<IPAddress, Vector3> neighborLocations;

    // таблиця маршрутизації
    //private Dictionary<IPAddress, RouteInfo> routingTable = new Dictionary<IPAddress, RouteInfo>();

    // AODV менеджер симуляційної мережі (для детекції вузлів)
    private NetworkManager networkManager;

    void Start()
    {
        networkManager = FindObjectOfType<NetworkManager>();
        neighborLocations = new Dictionary<IPAddress, Vector3>();

        foreach (var node in networkManager.nodes)
        {
           // routingTable.Add(node.ipAddress, null);
        }

        InvokeRepeating("BroadcastLocation", 0f, broadcastInterval);
    }

    private void BroadcastLocation()
    {
        foreach (var node in networkManager.nodes)
        {
            if (node != this && Vector3.Distance(transform.position, node.transform.position) <= availabilityRadius)
            {
                node.SendMessage("OnLocationBroadcast", new LocationBroadcast(ipAddress, transform.position), SendMessageOptions.RequireReceiver);
            }
        }
    }

    // отримуємо локацію сусіда
    private void OnLocationBroadcast(LocationBroadcast broadcast)
    {
        if (broadcast.IpAddress != ipAddress)
        {
            neighborLocations[broadcast.IpAddress] = broadcast.Position;
        }
    }

    public Vector3? GetNeighborLocation(IPAddress neighborId)
    {
        if (neighborLocations.ContainsKey(neighborId))
        {
            return neighborLocations[neighborId];
        }
        return null;
    }

    public Dictionary<IPAddress, Vector3> GetAllNeighborLocations() => neighborLocations;

    /*public void Receive(string messageBody,
                        MessageType messageType,
                        IPAddress sourceId,
                        IPAddress destinationId,
                        IPAddress senderId = null)
    {
        switch (messageType)
        {
            case MessageType.RREQ:
                if (destinationId == ipAddress)
                {
                    Send(null, MessageType.RREP, sourceId, destinationId); //seq id
                }
                else
                { //forward
                    Send(messageBody, MessageType.RREP, sourceId, destinationId, ipAddress);
                }
                break;

            case MessageType.RREP:
                routingTable[destinationId].NextHop = senderId;

                if (sourceId != ipAddress)
                {
                    if (IsNeighbor(sourceId))
                    {
                        Send(messageBody, messageType, sourceId, destinationId);
                        //po kaifu
                    }
                    else
                    {//forward
                        Send(messageBody, MessageType.RREP, sourceId, destinationId, ipAddress);
                    }
                }
                //finish
                break;

            case MessageType.RERR:
                Debug.Log($"Error occured while sending data from {sourceId} to {destinationId}. SeqID: {00}");
                break;
        }
    }

    public void Send(string messageBody,
                    MessageType messageType,
                    IPAddress sourceId,
                    IPAddress destinationId,
                    IPAddress senderId = null)
    {
        var destinationNode = routingTable[destinationId];

        if (destinationNode == null)
        {
            return;
        }

        if (destinationNode.NextHop == null || messageType == MessageType.RREQ /* || timeout)
        {
            //broadcast
            foreach (var node in routingTable.Keys) 
            {
                if (IsNeighbor(node)) 
                {
                    Send(null, MessageType.RREQ, sourceId, destinationId, ipAddress);
                }
            }
        }
        
        Receive(messageBody, messageType, sourceId, destinationId, ipAddress);
        
    }*/

    public bool IsNeighbor(IPAddress potentialNeighborId) //імітація досяжності
    {
        var foundNode = networkManager.nodes.Find(n => n.ipAddress == potentialNeighborId);

        if (Vector3.Distance(transform.position, foundNode.transform.position) < availabilityRadius)
        {
            return true;
        }

        return false;
    }
}

public enum MessageType : byte
{
    None = 0,
    RREQ = 1,
    RREP = 2,
    RERR = 3,
    HELLO = 4
}

public struct LocationBroadcast
{
    public IPAddress IpAddress;
    public Vector3 Position;

    public LocationBroadcast(IPAddress ipAddress, Vector3 position)
    {
        IpAddress = ipAddress;
        Position = position;
    }
}

class RouteInfo
{
    public IPAddress NextHop;
    public (IPAddress, IPAddress, int) SequenceId; // source, destination, counter
}