using OpenTK;
using RubinatorCore.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RubinatorCore {
    public static class Utility {
        private static readonly int[] axisToFace = { 4, 3, 5 };

        public static byte[] MoveToByte(Move move) {
            int face = (int)move.Face;

            byte moveData = (byte)((face + 1) << 1);

            if (move.Direction == -1) {
                moveData |= 0x01;
            }

            return Enumerable.Repeat(moveData, Math.Abs(move.Count)).ToArray();
        }

        public static byte[] MultiTurnToByte(Move move1, Move move2) {
            if (!Cube.IsOpponentFace(move1.Face, move2.Face))
                throw new InvalidOperationException();

            Move leftMove = move1.Face < move2.Face ? move1 : move2;
            Move rightMove = move2.Face < move1.Face ? move1 : move2;

            // calc parallel move byte
            byte moveByte = 0;
            moveByte |= (byte)((int)leftMove.Face << 2);

            byte leftDir = (byte)(leftMove.Direction > 0 ? 0x00 : 0x01);
            byte rightDir = (byte)(rightMove.Direction > 0 ? 0x00 : 0x01);
            moveByte |= (byte)(leftDir << 1);
            moveByte |= (byte)(rightDir << 0);
            moveByte |= 0x10;

            // create data array
            int minCount = Math.Min(Math.Abs(leftMove.Count), Math.Abs(rightMove.Count));
            int maxCount = Math.Max(Math.Abs(leftMove.Count), Math.Abs(rightMove.Count));

            byte[] data = new byte[maxCount];

            // fill array with data
            for (int i = 0; i < maxCount; i++) {
                if (i < minCount)
                    data[i] = moveByte;
                else if (i < Math.Abs(leftMove.Count)) {
                    data[i] = MoveToByte(leftMove)[0];
                }
                else if (i < Math.Abs(rightMove.Count)) {
                    data[i] = MoveToByte(rightMove)[0];
                }
            }

            return data;
        }

        public static Move ByteToMove(byte moveByte) {
            CubeFace face = (CubeFace)((moveByte >> 2) - 1);

            int count = 1;
            if ((moveByte & 0x01) == 0x01) count = -1;
            else if ((moveByte & 0x02) == 0x02) count = 2;

            return new Move(face, count);
        }

        public static Move[] MultiTurnByteToMove(byte moveByte) {
            int axis = ((moveByte & 0x30) >> 4) - 1;

            Move leftMove = new Move((CubeFace)axis);
            Move rightMove = new Move((CubeFace)(axisToFace[axis]));

            // left move count
            if ((moveByte & 0x01) == 0x01) leftMove.Count = -1;
            else if ((moveByte & 0x04) == 0x04) leftMove.Count = 2;

            // right move count
            if ((moveByte & 0x02) == 0x02) rightMove.Count = -1;
            else if ((moveByte & 0x08) == 0x08) rightMove.Count = 2;

            return new Move[] { leftMove, rightMove };
        }

        public static float ToRad(float deg) {
            return (float)(Math.PI * deg / 180f);
        }

        public static Packet GetPacketData(this Move move) {
            int face = (int)move.Face;

            byte data = (byte)((face + 1) << 2);

            switch (move.CountPositive) {
                case 3:
                    // inverted
                    data |= 0x01;
                    break;
                case 2:
                    // half move
                    data |= 0x02;
                    break;
                default: break;
            }

            return new Packet(0x01, data);
        }

        public static Packet GetMultiturnPacketData(this Move move1, Move move2) {
            if (!Cube.IsOpponentFace(move1.Face, move2.Face))
                throw new InvalidOperationException();

            Move leftMove = move1.Face < move2.Face ? move1 : move2;
            Move rightMove = move2.Face < move1.Face ? move1 : move2;

            // axis 0 = LR, 1 = UD, 2 = FB
            int axis = (int)leftMove.Face;

            byte data = (byte)((axis + 1) << 4);

            switch (leftMove.CountPositive) {
                case 3:
                    // inverted
                    data |= 0x01;
                    break;
                case 2:
                    // half move
                    data |= 0x04;
                    break;
                default: break;
            }

            switch (rightMove.CountPositive) {
                case 3:
                    // inverted
                    data |= 0x02;
                    break;
                case 2:
                    // half move
                    data |= 0x08;
                    break;
                default: break;
            }

            return new Packet(0x02, data);
        }
    }
}
