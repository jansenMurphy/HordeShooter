using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace GameServer {
    public class PacketAckManager
    {
        //TODO potentially write one ackRange to some packets as to not bloat the packet.
        //TODO track quantity and rate of failed packets to plan accoringly
        //TODO Notify other modules of packet failure
        private class AckRange { public ushort start, count; }

        List<PacketAndMetadata> inflightPackets = new List<PacketAndMetadata>();
        List<AckRange> ackRanges = new List<AckRange>();
        bool needNewAckRange=true;
        ushort nextInFlight = 0, nextExpectedReceived = 0;
        private const int wraparoundValue = 20;
        private NetworkConnectionHealth nch;

        public void SetNetworkConnectionHealth(NetworkConnectionHealth nch)
        {
            this.nch = nch;
        }

        public ushort AddPacketToInFlight(PacketAndMetadata pm)
        {
            inflightPackets.Add(pm);
            pm.timeSent = Time.timeSinceLevelLoad;
            return nextInFlight++;
        }

        public bool NotePacketReceived(PacketAndMetadata pm)
        {
            return NotePacketReceived(pm.id);
        }

        public bool NotePacketReceived(ushort packetId)//Returns whether it should process packet
        {
            if (nextExpectedReceived == packetId)
            {
                if (needNewAckRange)
                {
                    ackRanges.Add(new AckRange() { start = packetId, count = 1 });
                    needNewAckRange = false;
                }
                else
                {
                    ackRanges[ackRanges.Count - 1].count++;
                }
                nextExpectedReceived++;
                return true;
            } else if ((packetId < wraparoundValue && packetId+nextExpectedReceived > ushort.MaxValue) || packetId > nextExpectedReceived)
            {
                ackRanges.Add(new AckRange() { start = packetId, count = 1 });
                nextExpectedReceived = (ushort)(packetId + 1);
                return true;
            }
            else
            {
                return false;
            }
        }
        
        public void SerializeAckPackets(Packet packet)
        {
            byte count = (byte)ackRanges.Count;
            packet.Write(count);
            for(int i=0;i<count;i++)
            {
                packet.Write(ackRanges[i].start);
                packet.Write(ackRanges[i].count);
            }
            ackRanges.RemoveRange(0, count);
            if (ackRanges.Count > 0)
            {
                Debug.LogWarning("Tried to send too many (>255) ack ranges!");
                return;
            }
            needNewAckRange = true;
        }

        public void SerializeAnAckPacket(Packet packet)
        {
            packet.Write(ackRanges[0].start);
            packet.Write(ackRanges[0].count);
            ackRanges.RemoveAt(0);
        }

        public (ushort,ushort)? PopAnAckRangePacket()
        {
            if (ackRanges.Count == 0) return null;
            (ushort, ushort) retval = (ackRanges[0].start, ackRanges[0].count);
            ackRanges.RemoveAt(0);
            return retval;
        }

        public void DeserializeAckPackets(Packet packet)
        {
            byte count = packet.ReadByte();
            for (int i = 0; i < count; i++)
            {
                RemoveAckPacket(packet);
            }
        }
        public void DeserializeAnAckPacket(Packet packet)
        {
            RemoveAckPacket(packet);
        }

        private void RemoveAckPacket(Packet packet)
        {
            ushort start = packet.ReadUShort(), length = packet.ReadUShort();
            ushort j = 0;
            while (inflightPackets[j].id != start)
            {
                inflightPackets[j].ACKedOrNacked(false);
            }
            for (int i = 0; i < length; i++)
            {
                inflightPackets[i + j].ACKedOrNacked(true);
            }
            nch.AddPingToArray(Time.timeSinceLevelLoad - inflightPackets[inflightPackets.Count - 1].timeSent);
            inflightPackets.RemoveRange(j, length);
        }

        private void RemoveAckPacket(ushort start, ushort length)
        {
            ushort j = 0;
            while (inflightPackets[j].id != start)
            {
                inflightPackets[j].ACKedOrNacked(false);
            }
            for (int i = 0; i < length; i++)
            {
                inflightPackets[i + j].ACKedOrNacked(true);
            }
            nch.AddPingToArray(Time.timeSinceLevelLoad - inflightPackets[inflightPackets.Count - 1].timeSent);
            inflightPackets.RemoveRange(j, length);
        }

        public void TimeOutPackets(float timeoutDelay = 5f)//TODO Call this every frame
        {
            float maxTime = Time.realtimeSinceStartup - timeoutDelay;
            while (inflightPackets[0].timeSent > maxTime)
            {
                inflightPackets[0].ACKedOrNacked(false);
                inflightPackets.RemoveAt(0);
            }
        }
    }
}