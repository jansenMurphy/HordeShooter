using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameServer {
    public class ServerSend
    {
        private static void EnqueTCPData(byte toClient, Packet packet, System.Action<bool> onACKorNACK)
        {
            Server.clients[toClient].tcp.EnqueData(packet, onACKorNACK);
        }

        private static void EnqueTCPDataToAll(Packet packet, System.Action<bool> onACKorNACK)
        {
            foreach (byte clientId in Server.clients.Keys)
            {
                Server.clients[clientId].tcp.EnqueData(packet, onACKorNACK);
            }
        }

        private static void EnqueTCPDataToAllExcept(Packet packet, short clientException, System.Action<bool> onACKorNACK)
        {
            foreach (byte clientId in Server.clients.Keys)
            {
                if (clientException == clientId) continue;
                Server.clients[clientId].tcp.EnqueData(packet, onACKorNACK);
            }
        }

        private static void EnqueUDPData(byte toClient, Packet packet, System.Action<bool> onACKorNACK)
        {
            Server.clients[toClient].udp.EnqueData(packet, onACKorNACK);
        }
        private static void SendUDPDataToAll(Packet packet, System.Action<bool> onACKorNACK)
        {
            foreach (byte clientId in Server.clients.Keys)
            {
                Server.clients[clientId].udp.EnqueData(packet, onACKorNACK);
            }
        }

        private static void EnqueUDPDataToAllExcept(Packet packet, short clientException, System.Action<bool> onACKorNACK)
        {
            foreach (byte clientId in Server.clients.Keys)
            {
                if (clientException == clientId) continue;
                Server.clients[clientId].udp.EnqueData(packet, onACKorNACK);
            }
        }

        public static void SpawnObjectOnAll(ushort objectID,string assetGUID, System.Action<bool> onACKorNACK = null)
        {
            using (Packet packet = new Packet((byte)ServerPackets.spawnObject))
            {
                packet.Write(objectID);
                packet.Write(assetGUID);
                EnqueTCPDataToAll(packet, onACKorNACK);
            }
        }
        public static void DespawnObjectOnAll(ushort objectID, System.Action<bool> onACKorNACK = null)
        {
            using (Packet packet = new Packet((byte)ServerPackets.despawnObject))
            {
                packet.Write(objectID);
                EnqueTCPDataToAll(packet, onACKorNACK);
            }
        }
        public static void UpdateObjectOnAll(ushort objectID, byte[] newObjectState, System.Action<bool> onACKorNACK = null)
        {
            using (Packet packet = new Packet((byte)ServerPackets.spawnObject))
            {
                packet.Write(objectID);
                packet.Write(newObjectState);
                EnqueTCPDataToAll(packet, onACKorNACK);
            }
        }
        public static void SpawnObject(byte clientID, ushort objectID, string assetGUID, System.Action<bool> onACKorNACK = null, bool clientHasControl = false)
        {
            using (Packet packet = new Packet((byte)ServerPackets.spawnObject))
            {
                packet.Write(objectID);
                packet.Write(assetGUID);
                packet.Write(clientHasControl);
                EnqueTCPData(clientID,packet, onACKorNACK);
            }
        }
        public static void DespawnObject(byte clientID, ushort objectID, System.Action<bool> onACKorNACK = null)
        {
            using (Packet packet = new Packet((byte)ServerPackets.despawnObject))
            {
                packet.Write(objectID);
                EnqueTCPData(clientID, packet, onACKorNACK);
            }
        }
        public static void UpdateObject(byte clientID, ushort objectID, byte[] newObjectState, System.Action<bool> onACKorNACK = null)
        {
            using (Packet packet = new Packet((byte)ServerPackets.spawnObject))
            {
                packet.Write(objectID);
                packet.Write(newObjectState);
                EnqueTCPData(clientID,packet, onACKorNACK);
            }
        }

        public static void Ack(byte clientID,ushort ackRangeStart, ushort ackRangeCount, System.Action<bool> onACKorNACK = null)
        {
            using (Packet packet = new Packet((byte)ServerPackets.ackSingle))
            {
                packet.Write(ackRangeStart);
                packet.Write(ackRangeCount);
                EnqueTCPData(clientID, packet, onACKorNACK);
            }
        }
        public static void Welcome(byte clientID, System.Action<bool> onACKorNACK = null)
        {
            using (Packet packet = new Packet((byte)ServerPackets.welcome))
            {
                packet.Write(clientID);
                EnqueTCPData(clientID, packet, onACKorNACK);
            }
        }
        public static void RMI(byte clientID, ushort objectID, string method,  string[] args, System.Action<bool> onACKorNACK = null)//TODO This may be a very bad way of calling RMIs
        {
            using (Packet packet = new Packet((byte)ServerPackets.rmi))
            {
                packet.Write(objectID);
                packet.Write(method);
                for (int i = 0; i < args.Length; i++)
                {
                    packet.Write(args[i]);
                }
                EnqueTCPData(clientID, packet, onACKorNACK);
            }
        }

        public static void SanityCheck(byte clientID, System.Action<bool> onACKorNACK=null)
        {
            using (Packet packet = new Packet((byte)ServerPackets.sanityCheck))
            {
                packet.Write(clientID);
                EnqueTCPData(clientID, packet, onACKorNACK);
            }
        }

        public static void FlushUDP(byte clientID)
        {
            Server.clients[clientID].udp.FlushData();
        }
    }
}
