using System;
using System.Collections.Generic;

namespace RubinatorCore.Communication
{
    // define a packet which is made up from an instruction and its data
    public struct Packet {
        public static int[] InstructionDataLength;

        public byte Instruction;
        public List<byte> Data;
    }

    // abstractly defines a bluetooth peer
    public abstract class BluetoothPeer {
        protected readonly string UUID_STRING = "053eaaaf-f981-4b64-a39e-ea4f5f44bb57";

        // maps instructions to their corresponding data length
        protected Dictionary<byte, int> InstructionDataLength;
        protected Packet currentPacket;

        protected event EventHandler<Packet> PacketReceived;
        protected void RaisePacketReceived(object sender, Packet packet) {
            PacketReceived?.Invoke(sender, packet);
        }

        protected abstract void WriteByte(byte b);
        protected abstract void WriteBytes(byte[] bytes);
        protected abstract void HandleReceivedByte(byte b);

        public abstract void SendPacket(Packet toSend);

        // try to connect to device
        // return true on success
        public abstract bool Connect(string address);

        // close the connection and dispose of streams and sockets
        public abstract void Disconnect();
    }
}