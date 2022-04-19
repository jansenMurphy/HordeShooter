using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameServer
{
    public class ServerHandle
    {
        public static void HandleWelcomeReceived(byte fromClient, Packet packet)
        {
            if (!Server.clients.ContainsKey(fromClient))
            {
                Debug.LogWarning("Client ID does not exist and is sending HandleWelcomeReceived packets");
                return;
            }

            short clientId = packet.ReadShort();
            string username = packet.ReadString();
            //TODO Check that client claims correct id
        }
        public static void HandleUpdateObject(byte fromClient, Packet packet)
        {
            if (!Server.clients.ContainsKey(fromClient))
            {
                Debug.LogWarning("Client ID does not exist and is sending update packets");
                return;
            }

            ushort objectNetID = packet.ReadUShort();
            if (Server.clients[fromClient].clientOwnedNetworkObjects.ContainsKey(objectNetID))
            {
                Server.clients[fromClient].clientOwnedNetworkObjects[objectNetID].UpdateObject(packet);
            }
        }
        public static void HandleAck(byte fromClient, Packet packet)
        {
            if (!Server.clients.ContainsKey(fromClient))
            {
                Debug.LogWarning("Client ID does not exist and is sending ack packets");
                return;
            }

            Server.clients[fromClient].DeserializeAnAckPacket(packet);
        }
        public static void HandleSanityCheck(byte fromClient, Packet packet)
        {
            if (!Server.clients.ContainsKey(fromClient))
            {
                Debug.LogWarning("Client ID does not exist and is sending sanityCheck packets");
                return;
            }
            packet.ReadByte();
        }
    }
}