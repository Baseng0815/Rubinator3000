using System;
using System.Collections.Generic;

namespace RubinatorCore.Communication
{
    // define a packet which is made up from an instruction and its data
    public struct Packet {
        public byte Instruction;
        public List<byte> Data;

        // maps instructions to their corresponding data length
        public static readonly Dictionary<byte, int> InstructionLengths;

        public Packet(byte instruction) {
            Instruction = instruction;
            Data = new List<byte>();
        }

        public Packet(byte instruction, byte data) {
            Instruction = instruction;
            Data = new List<byte> { data };
        }

        public Packet(byte instruction, byte[] data) {
            Instruction = instruction;
            Data = new List<byte>(data);
        }

        static Packet() {
            InstructionLengths = new Dictionary<byte, int>();

            InstructionLengths.Add(0x01, 1);
            InstructionLengths.Add(0x02, 1);
            InstructionLengths.Add(0x03, 54);
            InstructionLengths.Add(0x04, 0);
            InstructionLengths.Add(0x05, 1);
            InstructionLengths.Add(0x06, 1);
            InstructionLengths.Add(0x07, 1);
            InstructionLengths.Add(0x08, 1);
        }

        public override string ToString() {
            return BitConverter.ToString(new byte[] { Instruction }) + " / " + BitConverter.ToString(Data.ToArray());
        }

        public byte[] GetData() {
            List<byte> data = new List<byte>();
            data.Add(Instruction);
            data.AddRange(Data);

            return data.ToArray();
        }
    }

    // abstractly defines a bluetooth peer
    public abstract class BluetoothPeer {
        protected readonly string UUID_STRING = "053eaaaf-f981-4b64-a39e-ea4f5f44bb57";

        protected Packet currentPacket = new Packet(0x00);

        public event EventHandler<Packet> PacketReceived;

        protected abstract void ReceiveDataThread();

        protected void RaisePacketReceived(object sender, Packet packet) {
            PacketReceived?.Invoke(sender, packet);
        }

        protected abstract void WriteByte(byte b);
        protected abstract void WriteBytes(byte[] bytes);
        protected void HandleReceivedByte(byte b) {
            System.Diagnostics.Debug.WriteLine("CURRENT PACKET PRE" + currentPacket.ToString());
            System.Diagnostics.Debug.WriteLine("RECEIVE BYTES " + BitConverter.ToString(new byte[] { b }));

            // new instruction
            if (currentPacket.Instruction == 0x00) {
                currentPacket.Instruction = b;

            // there is still data to add
            } else if (currentPacket.Data.Count < Packet.InstructionLengths[currentPacket.Instruction]) {
                currentPacket.Data.Add(b);
            }

            // instruction and data received, invoke callback
            if (currentPacket.Data.Count == Packet.InstructionLengths[currentPacket.Instruction]) {
                System.Diagnostics.Debug.WriteLine("PACKET FINISHED" + currentPacket.ToString());
                RaisePacketReceived(this, currentPacket);
                currentPacket.Instruction = 0x00;
                currentPacket.Data.Clear();
            }

            System.Diagnostics.Debug.WriteLine("CURRENT PACKET POST" + currentPacket.ToString());
        }

        public void SendPacket(Packet toSend) {
            try {
                WriteByte(toSend.Instruction);
                WriteBytes(toSend.Data.ToArray());
            } catch (Exception e) {
                Log.LogMessage(e.Message);
            }
        }

        // try to connect to device
        // return true on success
        public abstract bool Connect(string address);

        // close the connection and dispose of streams and sockets
        public abstract void Disconnect();
    }
}