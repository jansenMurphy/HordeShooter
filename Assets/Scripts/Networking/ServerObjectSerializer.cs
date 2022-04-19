using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameServer
{
    public class ServerObjectSerializer
    {
        public void SerializeAndSendObjects()//Probably call this in fixed update or maybe update I dunno
        {
            foreach (var client in Server.clients.Values)
            {
                foreach (var networkObject in Server.serverNetworkedObjects)
                {
                    byte[] possiblySend = networkObject.Value.SerializeObject(client.observationObjects);
                    if (possiblySend.Length > 0)
                    {
                        ServerSend.UpdateObject(client.id, networkObject.Key, possiblySend, networkObject.Value.OnAckOrNack);
                    }
                }
                var ack = client.PopAnAckPacket();
                if (ack != null) ServerSend.Ack(client.id, ack.Value.Item1, ack.Value.Item2, (tf) => { if (!tf) client.FailedAckPacket(ack.Value.Item1, ack.Value.Item2); });
                ServerSend.FlushUDP(client.id);
                client.TimeOutOldPackets();
            }
        }
    }
}
