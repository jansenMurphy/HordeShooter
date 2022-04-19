using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;

namespace GameServer {
    public class ServerClient
    {
        public static int dataBufferSize = 1300;
        public byte id;
        public TCP tcp;
        public UDP udp;
        public List<PlayerManager> observationObjects = new List<PlayerManager>();
        private double lastSuccessfulReceiveTime = 0;
        public delegate void voidVoidDelg();
        private PacketAckManager pam;
        public Dictionary<ushort, NetworkObject> clientOwnedNetworkObjects = new Dictionary<ushort, NetworkObject>();

        public ServerClient(byte id)
        {
            pam = new PacketAckManager();
            this.id = id;
            tcp = new TCP(id);
            udp = new UDP(id,pam);
            tcp.receiveData += updateLastSuccessfulReceiveTime;
            udp.receiveData += updateLastSuccessfulReceiveTime;
            DisconnectNonresponsive();
        }

        public void Disconnect()
        {
            tcp.Disconnect();
            /*
            udp.Disconnect();
            */
        }

        private void updateLastSuccessfulReceiveTime()
        {
            lastSuccessfulReceiveTime = Time.unscaledTimeAsDouble;
        }

        public (ushort,ushort)? PopAnAckPacket()
        {
            return pam.PopAnAckRangePacket();
        }
        public void FailedAckPacket(ushort start, ushort length)
        {
            ServerSend.Ack(id, start, length, (tf) => { if (!tf) FailedAckPacket(start, length); });
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
            private readonly byte id;
            private NetworkStream networkStream;
            private byte[] receiveBuffer;
            private Packet receivePacket;
            public voidVoidDelg receiveData;
            private PacketAndMetadata sendPacketAndMetadata;
            private int packetReadLengthRemaining = 0;

            public TCP(byte id)
            {
                this.id = id;
            }

            public void Connect(TcpClient socket)
            {
                this.socket = socket;
                socket.ReceiveBufferSize = dataBufferSize;
                socket.SendBufferSize = dataBufferSize;

                networkStream = socket.GetStream();

                receiveBuffer = new byte[dataBufferSize];
                networkStream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                receivePacket = new Packet();
            }

            private void ReceiveCallback(IAsyncResult result)
            {
                try
                {
                    int byteLength = networkStream.EndRead(result);
                    if (byteLength <= 0)
                    {
                        Server.clients[id].Disconnect();
                        return;
                    }

                    byte[] data = new byte[byteLength];
                    Array.Copy(receiveBuffer, data, byteLength);

                    receivePacket.Reset(HandleData(data));

                    networkStream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);
                    //Docs also say that you can only read and write to streams simultaneously if they are on different threads. Methinks this is automatic
                }
                catch (Exception e)
                {
                    Debug.LogWarning("Cannot properly receive TCP data " + e);
                    Server.clients[id].Disconnect();
                }
            }

            public void EnqueData(Packet packet, Action<bool> onACKorNACK)
            {
                if(sendPacketAndMetadata.packet == null)
                {
                    sendPacketAndMetadata.packet = new Packet();
                }
                if(sendPacketAndMetadata.packet.Length() + packet.Length() > 1200)
                {
                    FlushData(sendPacketAndMetadata);
                }
                sendPacketAndMetadata.packet.Write(packet.ToArray());
                sendPacketAndMetadata.ACKedOrNacked += onACKorNACK;
            }

            public void FlushData(PacketAndMetadata packetAndMetadata)
            {
                try
                {
                    packetAndMetadata.packet.WriteLength();
                    if (socket != null)
                    {
                        networkStream.BeginWrite(packetAndMetadata.packet.ToArray(), 0, packetAndMetadata.packet.Length(), null, null);
                    }
                    packetAndMetadata = new PacketAndMetadata();
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

                while (packetReadLengthRemaining <= receivePacket.UnreadLength() - sizeof(ushort))
                {
                    packetReadLengthRemaining = receivePacket.ReadUShort();
                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        using (Packet packet = new Packet(receivePacket.ReadBytes(packetReadLengthRemaining)))
                        {
                            byte packetId = packet.ReadByte();
                            Server.packetHandlers[packetId](id,packet);
                        }
                    });
                    packetReadLengthRemaining = receivePacket.ReadUShort(false);
                }
                if (receivePacket.UnreadLength() == 0)
                    return true;
                return false;
            }

            public void Disconnect()
            {
                socket.Close();
                /*
                networkStream = null;
                receivePacket = null;
                receiveBuffer = null;
                socket = null;
                */
            }
        }

        public class UDP
        {
            public UdpClient socket;
            public IPEndPoint endPoint;
            private readonly byte id;
            public voidVoidDelg receiveData;
            private PacketAckManager pam;
            private PacketAndMetadata sendPacketAndMetadata;
            private Packet receivePacket;
            private int packetReadLengthRemaining;

            public UDP(byte id,PacketAckManager pam)
            {
                this.id = id;
                this.pam = pam;
            }

            public void Connect(IPEndPoint endPoint)
            {
                this.endPoint = endPoint;
                receivePacket = new Packet();
                sendPacketAndMetadata = new PacketAndMetadata();
            }

            public void EnqueData(Packet packet, Action<bool> onACKorNACK)
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
                sendPacketAndMetadata.ACKedOrNacked += onACKorNACK;
            }
            public void FlushData()
            {
                try
                {
                    sendPacketAndMetadata.packet.WriteLength();
                    sendPacketAndMetadata.packet.InsertUShort(pam.AddPacketToInFlight(sendPacketAndMetadata));
                    sendPacketAndMetadata.packet.InsertByte(id);
                    if (socket != null)
                    {
                        Server.SendUDPData(endPoint, sendPacketAndMetadata.packet);
                    }
                    sendPacketAndMetadata = new PacketAndMetadata();
                }
                catch (Exception e)
                {
                    Debug.LogWarning(e);
                }
            }

            public bool HandleData(byte[] data)
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
                            Server.packetHandlers[packetId](id,packet);
                        }
                    });
                    packetReadLengthRemaining = receivePacket.ReadUShort(false);
                }
                if (receivePacket.UnreadLength() == 0)
                    receivePacket.Reset();
                return false;
            }

            /*
            public void Disconnect()
            {
                endPoint = null;
            }
            */

        }

        public async void DisconnectNonresponsive()
        {
            lastSuccessfulReceiveTime = Time.unscaledTimeAsDouble;

            while (Time.unscaledTimeAsDouble - lastSuccessfulReceiveTime < Server.timeoutDelay)
            {
                while (Time.unscaledTimeAsDouble - lastSuccessfulReceiveTime < Server.timeoutDelay)
                {
                    await System.Threading.Tasks.Task.Delay(Mathf.RoundToInt(Server.timeoutDelay));
                }
                ServerSend.SanityCheck(id);
                await System.Threading.Tasks.Task.Delay(Mathf.RoundToInt(Server.timeoutDelay/2));
            }
            Disconnect();
        }
    }
}
