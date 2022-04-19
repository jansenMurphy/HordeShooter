using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameServer {
    public class NetworkConnectionHealth
    {
        byte id;
        PacketAckManager pam;

        private float[] pingArray = new float[10];
        private int pingArrayIndex = 0;

        private float storedPing=0;
        private bool storedPingDirty = true;

        public NetworkConnectionHealth(byte id, PacketAckManager pam)
        {
            this.id = id;
            this.pam = pam;
        }

        public void AddPingToArray(float timeInMS)
        {
            storedPingDirty = true;
            pingArray[pingArrayIndex] = timeInMS;
            pingArrayIndex = (pingArrayIndex + 1) % 10;
        }

        public double GetEstimatePing()
        {
            if (storedPingDirty)
            {
                storedPing = pingArray.Average();
                storedPingDirty = false;
            }
            return storedPing;
        }

        /*
        public float GetDroppedPacketHealth()
        {

        }
        */
    }
}
