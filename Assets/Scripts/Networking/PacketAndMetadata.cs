using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameServer {
    public class PacketAndMetadata
    {
        public Packet packet;
        public ushort id;
        public float timeSent;
        public Action<bool> ACKedOrNacked;
    }
}