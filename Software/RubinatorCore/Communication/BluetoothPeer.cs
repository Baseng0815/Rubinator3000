using System.Collections.Generic;

namespace RubinatorCore.Communication
{
    // define a packet which is made up from an instruction and its data
    public struct Packet {
        public static int[] InstructionDataLength;

        public byte Instruction = 0x00;
        public List<byte> Data;
    }

    // abstractly defines a bluetooth peer
    public abstract class BluetoothPeer {
        protected readonly UUID SERVICE_UUID;
        // maps instructions to their corresponding data length
        protected readonly Dictionary<byte, int> InstructionDataLength;
        protected Packet currentPacket;

        public event EventHandler<Packet> PacketReceived;

        private void WriteByte(byte b);
        private void WriteBytes(byte[] bytes);
        private void HandleReceivedByte(byte b);

        public abstract void SendPacket(Packet toSend);

        // try to connect to device
        // return true on success
        public abstract bool Connect(string address);

        // close the connection and dispose of streams and sockets
        public abstract void Disconnect();
    }
}