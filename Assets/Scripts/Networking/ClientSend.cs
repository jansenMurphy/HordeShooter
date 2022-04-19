using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameServer
{
    public class ClientSend : MonoBehaviour
    {
        private static void EnqueTCPData(Packet packet, System.Action<bool> onACKorNACK)
        {
            Client.singleton.tcp.EnqueData(packet);
        }
        private static void EnqueUDPData(Packet packet, System.Action<bool> onACKorNACK)
        {
            Client.singleton.udp.EnqueData(packet);
        }

        public static void WelcomeReceived()
        {
            using (Packet packet = new Packet((byte)ClientPackets.welcomeReceived))
            {
                packet.Write(Client.singleton.id);
                packet.Write("Some welcome string TODO");
                EnqueTCPData(packet, tf => { if (!tf) WelcomeReceived(); });
            }
        }
        public static void UpdateObject(NetworkObject networkObject, System.Action<bool> onACKorNACK)
        {
            using (Packet packet = new Packet((byte)ClientPackets.updateObject))
            {
                byte[] serializedObject = networkObject.SerializeObject(null);
                if(serializedObject.Length != 0)
                {
                    packet.Write(networkObject.GetNetworkID());
                    packet.Write(serializedObject);
                    EnqueUDPData(packet, onACKorNACK);
                }
            }
        }
        public static void SanityCheck()
        {
            using (Packet packet = new Packet((byte)ClientPackets.sanityCheck))
            {
                packet.Write(Client.singleton.id);
                EnqueTCPData(packet, tf => { if (!tf) SanityCheck(); });
            }
        }
        public static void Ack(ushort ackStart, ushort ackRange, System.Action<bool> onACKorNACK)
        {
            using (Packet packet = new Packet((byte)ClientPackets.ackSingle))
            {
                packet.Write(ackStart);
                packet.Write(ackRange);
                EnqueUDPData(packet, onACKorNACK);
            }
        }
    }
}