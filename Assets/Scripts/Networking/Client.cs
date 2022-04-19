using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace GameServer {
    public class Client : MonoBehaviour
    {
        public static Client singleton;
        public static int dataBufferSize = 1300;

        public string ip = "127.0.0.1";
        public int port = 28002;
        public byte id;
        public TCP tcp;
        public UDP udp;
        PacketAckManager pam = new PacketAckManager();

        public Dictionary<ushort, NetworkObject> networkObjects,ownedNetworkObjects;

        private delegate void PacketHandler(Packet packet);
        private static Dictionary<byte, PacketHandler> packetHandlers;

        public bool isConnected = false;

        void Awake()
        {
            if (singleton != null)
            {
                Destroy(gameObject);
                return;
            }
            singleton = this;
            networkObjects = new Dictionary<ushort, NetworkObject>();
            ownedNetworkObjects = new Dictionary<ushort, NetworkObject>();
            DontDestroyOnLoad(gameObject);
        }

        private void OnApplicationQuit()
        {
            Disconnect();
        }

        private void Start()
        {
            tcp = new TCP();
            udp = new UDP(pam);
        }

        public void ConnectToServer()
        {
            InitializeClientPackets();

            isConnected = true;
            tcp.Connect();
        }

        public void DeserializeAnAckPacket(Packet packet)
        {
            pam.DeserializeAnAckPacket(packet);
        }
        public void TimeOutOldPackets()
        {
            pam.TimeOutPackets();
        }

        public class TCP
        {
            public TcpClient socket;
            private NetworkStream networkStream;
            private byte[] receiveBuffer;
            private Packet receivePacket;
            private PacketAndMetadata sendPacketAndMetadata;
            private int packetReadLengthRemaining = 0;

            public TCP()
            {
                sendPacketAndMetadata = new PacketAndMetadata();
            }

            public void Connect()
            {
                socket = new TcpClient
                {
                    ReceiveBufferSize = dataBufferSize,
                    SendBufferSize = dataBufferSize
                };
                receiveBuffer = new byte[dataBufferSize];
                socket.BeginConnect(singleton.ip, singleton.port, ConnectCallback, socket);
            }

            private void ConnectCallback(IAsyncResult result)
            {
                socket.EndConnect(result);
                if (!socket.Connected)
                {
                    return;
                }
                networkStream = socket.GetStream();
                receivePacket = new Packet();
                networkStream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                //Docs also say that you can only read and write to streams simultaneously if they are on different threads. Methinks this is automatic
            }

            private void ReceiveCallback(IAsyncResult result)
            {
                try
                {
                    int byteLength = networkStream.EndRead(result);
                    if (byteLength <= 0)
                    {
                        singleton.Disconnect();
                    }

                    byte[] data = new byte[byteLength];
                    Array.Copy(receiveBuffer, data, byteLength);


                    receivePacket.Reset(HandleData(data));
                    networkStream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Cannot properly receive TCP data " + e);
                    Disconnect();
                }
            }

            public void EnqueData(Packet packet)
            {
                if (sendPacketAndMetadata.packet == null)
                {
                    sendPacketAndMetadata.packet = new Packet();
                }
                if (sendPacketAndMetadata.packet.Length() + packet.Length() > 1200)
                {
                    FlushData();
                }
                sendPacketAndMetadata.packet.Write(packet.ToArray());
            }

            public void FlushData()
            {
                try
                {
                    sendPacketAndMetadata.packet.WriteLength();
                    sendPacketAndMetadata.packet.InsertByte(singleton.id);
                    if (socket != null)
                    {
                        networkStream.BeginWrite(sendPacketAndMetadata.packet.ToArray(), 0, sendPacketAndMetadata.packet.Length(), null, null);
                    }
                    sendPacketAndMetadata = new PacketAndMetadata();
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e);
                }
            }

            private bool HandleData(byte[] data)
            {
                using (Packet packet = new Packet(data))
                {
                    receivePacket.Write(packet.ReadBytes(packet.UnreadLength()));
                }

                packetReadLengthRemaining = receivePacket.ReadUShort(false);

                while (packetReadLengthRemaining <= receivePacket.UnreadLength()-sizeof(ushort))
                {
                    packetReadLengthRemaining = receivePacket.ReadUShort();
                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        using (Packet packet = new Packet(receivePacket.ReadBytes(packetReadLengthRemaining)))
                        {
                            byte packetId = packet.ReadByte();
                            packetHandlers[packetId](packet);
                        }
                    });
                    packetReadLengthRemaining = receivePacket.ReadUShort(false);
                }
                if (receivePacket.UnreadLength() ==0)
                    return true;
                return false;
            }

            public void Disconnect()
            {
                singleton.Disconnect();
                networkStream.Close();
                receivePacket = null;
                receiveBuffer = null;
                networkStream = null;
                socket = null;
            }
        }

        public class UDP
        {
            public UdpClient socket;
            public IPEndPoint endPoint;
            private PacketAckManager pam;
            private PacketAndMetadata sendPacketAndMetadata;
            private Packet receivePacket;
            private int packetReadLengthRemaining;

            public UDP(PacketAckManager pam)
            {
                this.pam = pam;
                sendPacketAndMetadata = new PacketAndMetadata();
                endPoint = new IPEndPoint(IPAddress.Parse(singleton.ip), singleton.port);
            }

            public void Connect(int localPort)
            {
                socket = new UdpClient(localPort);
                socket.AllowNatTraversal(true);
                socket.Connect(endPoint);
                socket.BeginReceive(ReceiveCallback, null);

                using (Packet packet = new Packet())
                {
                    EnqueData(packet);
                    FlushData();
                }
            }

            public void EnqueData(Packet packet)
            {
                if (sendPacketAndMetadata.packet == null)
                {
                    sendPacketAndMetadata.packet = new Packet();
                }
                if (sendPacketAndMetadata.packet.Length() + packet.Length() > 1200)
                {
                    FlushData();
                }
                sendPacketAndMetadata.packet.Write(packet.ToArray());
            }

            public void FlushData()
            {
                try
                {
                    sendPacketAndMetadata.packet.WriteLength();
                    sendPacketAndMetadata.packet.InsertUShort(pam.AddPacketToInFlight(sendPacketAndMetadata));
                    sendPacketAndMetadata.packet.InsertByte(singleton.id);
                    if (socket != null)
                    {
                        socket.BeginSend(sendPacketAndMetadata.packet.ToArray(), sendPacketAndMetadata.packet.Length(), null, null);
                    }
                    sendPacketAndMetadata = new PacketAndMetadata();
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e);
                }
            }

            public void ReceiveCallback(IAsyncResult result)
            {
                try
                {
                    byte[] data = socket.EndReceive(result, ref endPoint);
                    socket.BeginReceive(ReceiveCallback, null);

                    if(data.Length < 2)
                    {
                        singleton.Disconnect();
                        return;
                    }

                    HandleData(data);
                }
                catch (Exception)
                {
                    Disconnect();
                }
            }

            private bool HandleData(byte[] data)
            {
                using (Packet packet = new Packet(data))
                {
                    if (pam.NotePacketReceived(packet.ReadUShort()))
                    {
                        receivePacket.Write(packet.ReadBytes(packet.UnreadLength()));
                    }
                    else
                    {
                        return false;
                    }
                }

                packetReadLengthRemaining = receivePacket.ReadUShort(false);

                while (packetReadLengthRemaining <= receivePacket.UnreadLength() - sizeof(ushort))
                {
                    packetReadLengthRemaining = receivePacket.ReadUShort();
                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        using (Packet packet = new Packet(receivePacket.ReadBytes(packetReadLengthRemaining)))
                        {
                            byte packetId = packet.ReadByte();
                            packetHandlers[packetId](packet);
                        }
                    });
                    packetReadLengthRemaining = receivePacket.ReadUShort(false);
                }
                if (receivePacket.UnreadLength() == 0)
                    receivePacket.Reset();
                return false;
            }

            public void Disconnect()
            {
                singleton.Disconnect();

                endPoint = null;
                socket = null;
            }

        }
        private void InitializeClientPackets()
        {
            packetHandlers = new Dictionary<byte, PacketHandler>()
            {
                {(byte)ServerPackets.welcome,ClientHandle.HandleWelcome},
                {(byte)ServerPackets.spawnObject,ClientHandle.HandleObjectSpawn},
                {(byte)ServerPackets.despawnObject,ClientHandle.HandleObjectDespawn},
                {(byte)ServerPackets.updateObject,ClientHandle.HandleObjectUpdate},
                {(byte)ServerPackets.rmi,ClientHandle.HandleRMI},
                {(byte)ServerPackets.ackSingle,ClientHandle.HandleAckSingle},
                {(byte)ServerPackets.sanityCheck,ClientHandle.HandleSanityCheck}
            };
        }

        private void Disconnect()
        {
            if (isConnected)
            {
                isConnected = false;
                tcp.socket.Close();
                udp.socket.Close();
            }
        }
    }
}