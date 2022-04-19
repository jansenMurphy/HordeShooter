using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;

namespace GameServer
{
    public class Server
    {
        public static byte maxPlayers { get; private set; }
        public static int port { get; private set; }//28002
        private static TcpListener tcpListener;
        private static UdpClient udpListener;

        public static Dictionary<byte, ServerClient> clients = new Dictionary<byte, ServerClient>();
        public delegate void PacketHandler(byte fromClient, Packet packet);
        public static Dictionary<byte, PacketHandler> packetHandlers;

        private static Queue<ushort> availableObjectIDs;
        private static Queue<byte> availableClientIDs;
        private static int currentPlayerCount;

        public static Dictionary<ushort, NetworkObject> serverNetworkedObjects;

        public static int timeoutDelay = 20;
        public static void Start(byte _maxPlayers, int _port = 28002, int _timeoutDelay = 20)
        {
            currentPlayerCount = 0;
            maxPlayers = _maxPlayers;
            port = _port;
            timeoutDelay = _timeoutDelay;
            InitializeServerData();

            tcpListener = new TcpListener(IPAddress.IPv6Any, port);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

            udpListener = new UdpClient(port);
            udpListener.AllowNatTraversal(true);
            udpListener.BeginReceive(UDPReceiveCallback, null);

            Debug.Log($"Starting Server on port {port}.");
        }

        private static void TCPConnectCallback(IAsyncResult result)
        {
            TcpClient client = tcpListener.EndAcceptTcpClient(result);
            currentPlayerCount++;
            if(currentPlayerCount<maxPlayers)
                tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);
            try
            {
                byte newClientID = availableClientIDs.Dequeue();
                clients.Add(newClientID, new ServerClient(newClientID));
                clients[newClientID].tcp.Connect(client);
                ServerSend.Welcome(newClientID);
            }catch (InvalidOperationException)
            {
                Debug.LogWarning("No available client IDs; server probably full");
                //TODO Send message that server is full
            }
        }

        private static void UDPReceiveCallback(IAsyncResult result)
        {
            try
            {
                IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.IPv6Any, 0);
                byte[] data = udpListener.EndReceive(result, ref clientEndPoint);
                udpListener.BeginReceive(UDPReceiveCallback, null);

                if(data.Length < 2)
                {
                    return;
                }

                using (Packet packet = new Packet(data))
                {
                    byte clientId = packet.ReadByte();
                    if (!clients.ContainsKey(clientId))
                    {
                        return;
                    }
                    if(clients[clientId].udp.endPoint == null)
                    {
                        clients[clientId].udp.Connect(clientEndPoint);
                    }
                    if (clients[clientId].udp.endPoint.ToString().CompareTo(clientEndPoint.ToString()) == 0)
                    {
                        clients[clientId].udp.HandleData(packet.ReadBytes(packet.UnreadLength()));
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
        }

        public static void SendUDPData(IPEndPoint clientEndPoint, Packet packet)
        {
            try
            {
                if(clientEndPoint != null)
                {
                    udpListener.BeginSend(packet.ToArray(), packet.Length(), clientEndPoint, null, null);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static void SpawnObject(string newObjectGUID)
        {
            ushort objId = availableObjectIDs.Dequeue();
            ObjectPools.Spawn(newObjectGUID, (x) =>
            {
                serverNetworkedObjects.Add(objId, x.GetComponent<NetworkObject>());
                return true;
            });
            ServerSend.SpawnObjectOnAll(objId, newObjectGUID);
        }

        public static void DespawnObject(ushort netID)
        {
            if (serverNetworkedObjects.ContainsKey(netID))
            {
                serverNetworkedObjects[netID].DespawnObject();
                ServerSend.DespawnObjectOnAll(netID);
                availableObjectIDs.Enqueue(netID);
            }
            else
            {
                Debug.LogWarning("Object that was despawned does not exist");
            }
        }

        public static void SpawnClientObject(byte client, string newObjectGUID)
        {
            if (!clients.ContainsKey(client))
            {
                Debug.LogWarning("Invalid client ID");
            }

            ushort objId = availableObjectIDs.Dequeue();
            ObjectPools.Spawn(newObjectGUID, (x) =>
            {
                clients[client].clientOwnedNetworkObjects.Add(objId, x.GetComponent<NetworkObject>());
                return true;
            });
            ServerSend.SpawnObject(client, objId, newObjectGUID,(tf) => { if (!tf) SpawnClientObject(client, newObjectGUID); },true);
        }

        public static void DespawnClientObject(byte client, ushort netID)
        {
            if (!clients.ContainsKey(client))
            {
                Debug.LogWarning("Invalid client ID");
            }

            if (clients[client].clientOwnedNetworkObjects.ContainsKey(netID))
            {
                clients[client].clientOwnedNetworkObjects[netID].DespawnObject();
                ServerSend.DespawnObject(client,netID);
                availableObjectIDs.Enqueue(netID);
            }
            else
            {
                Debug.LogWarning("Object that was despawned does not exist");
            }
        }

        private static void InitializeServerData()
        {
            availableObjectIDs = new Queue<ushort>();
            serverNetworkedObjects = new Dictionary<ushort, NetworkObject>();
            for (byte i = 1; i <= maxPlayers; i++)
            {
                availableClientIDs.Enqueue(i);
            }

            for (ushort i = 1; i <= ushort.MaxValue; i++)
            {
                availableObjectIDs.Enqueue(i);
            }

            packetHandlers = new Dictionary<byte, PacketHandler>()
            {
                {(byte)ClientPackets.welcomeReceived, ServerHandle.HandleWelcomeReceived},
                {(byte)ClientPackets.updateObject, ServerHandle.HandleUpdateObject},
                {(byte)ClientPackets.ackSingle, ServerHandle.HandleAck},
                {(byte)ClientPackets.sanityCheck, ServerHandle.HandleSanityCheck}
            };
        }

        public static void Disconnect(byte id)
        {
            if (!clients.ContainsKey(id))
            {
                Debug.LogWarning("Client does not exist to be removed");
                return;
            }
            clients[id].Disconnect();
            clients.Remove(id);
            availableClientIDs.Enqueue(id);
            currentPlayerCount--;
            if(currentPlayerCount == maxPlayers - 1)
                tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);
        }

        public static void Stop()
        {
            tcpListener.Stop();
            udpListener.Close();
        }
    }
}