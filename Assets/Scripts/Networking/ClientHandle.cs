using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameServer {
    public class ClientHandle : MonoBehaviour
    {
        public static void HandleWelcome(Packet packet)
        {
            Client.singleton.id = packet.ReadByte();
            ClientSend.WelcomeReceived();
            Client.singleton.udp.Connect(((System.Net.IPEndPoint)Client.singleton.tcp.socket.Client.LocalEndPoint).Port);
        }

        public static void HandleObjectSpawn(Packet packet)
        {
            ushort objectNetID = packet.ReadUShort();
            string assetRef = packet.ReadString();
            bool clientHasControl = packet.ReadBool();
            if (!Client.singleton.networkObjects.ContainsKey(objectNetID))
            {
                ObjectPools.Spawn(assetRef, x =>
                {
                    if(clientHasControl)
                        Client.singleton.networkObjects.Add(objectNetID,x.GetComponent<NetworkObject>());
                    else
                        Client.singleton.ownedNetworkObjects.Add(objectNetID, x.GetComponent<NetworkObject>());
                    return true;
                });
            }
            else
            {
                Debug.LogWarning("Object ID already in use");
            }
        }
        public static void HandleObjectDespawn(Packet packet)
        {
            ushort objectNetID = packet.ReadUShort();
            if (Client.singleton.networkObjects.ContainsKey(objectNetID))
            {
                Client.singleton.networkObjects[objectNetID].DespawnObject();
                Client.singleton.networkObjects.Remove(objectNetID);
            }
            else if (Client.singleton.ownedNetworkObjects.ContainsKey(objectNetID))
            {
                Client.singleton.ownedNetworkObjects[objectNetID].DespawnObject();
                Client.singleton.ownedNetworkObjects.Remove(objectNetID);
            }
            else { 
                Debug.LogWarning("Could not despawn object by nonexistant id");
            }
        }
        public static void HandleObjectUpdate(Packet packet)
        {
            ushort objectNetID = packet.ReadUShort();
            if (Client.singleton.networkObjects.ContainsKey(objectNetID))
            {
                Client.singleton.networkObjects[objectNetID].UpdateObject(packet);
            }
        }
        public static void HandleRMI(Packet packet)
        {
            ushort objectNetID = packet.ReadUShort();
            if (Client.singleton.networkObjects.ContainsKey(objectNetID))
            {
                Client.singleton.networkObjects[objectNetID].RMI(packet);
            }
        }
        public static void HandleAckSingle(Packet packet)
        {
            Client.singleton.DeserializeAnAckPacket(packet);
        }

        public static void HandleSanityCheck(Packet packet)
        {
            packet.ReadByte();
            ClientSend.SanityCheck();
        }
    }
}
