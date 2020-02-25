﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RubinatorCore;
using RubinatorCore.Communication;
using RubinatorCore.Solving;

namespace Rubinator3000 {
    public enum ArduinoLEDs : byte { DOWN = 0x01, UP = 0x02, STRIPES = 0x04, ALL = UP | DOWN | STRIPES }
    public abstract class Arduino : IDisposable {
        public bool Connected { get => connected; }

        private bool connected = false;

        public void Connect() {
            SendPacket(new Packet(0x07, 0x01));

            Packet response = ReceivePacket();
            connected = response.Instruction == 0x07 && response.Data[0] == 0x11;
        }

        public void Disconnect() {
            SendPacket(new Packet(0x07, 0x00));

            Packet response = ReceivePacket();
            connected = response.Instruction == 0x07 && response.Data[0] == 0x10;
        }

        public void SendMove(Move move) {
            if (connected) {
                SendPacket(move.GetPacketData());
            }
            else
                throw new InvalidOperationException("Arduino not connected or port closed");
        }

        public Task SendMoveAsync(Move move) {
            return Task.Factory.StartNew(() => SendMove(move));
        }

        public void SendMultiTurnMove(Move move1, Move move2) {
            if (connected) {
                SendPacket(Utility.GetMultiturnPacketData(move1, move2));
            }
            else
                throw new InvalidOperationException("Arduino not connected or port closed");
        }

        public Task SendMultiTurnMoveAsync(Move move1, Move move2) {
            return Task.Factory.StartNew(() => SendMultiTurnMove(move1, move2));
        }

        public void SendLedCommand(ArduinoLEDs leds, byte brightness) {
            if (connected) {
                SendPacket(new Packet(0x05, new byte[] { (byte)leds, brightness }));
            }
            else
                throw new InvalidOperationException("Arduino not connected or port closed");
        }

        protected abstract Packet SendPacket(Packet packet);

        public abstract void Dispose();
    }
}
