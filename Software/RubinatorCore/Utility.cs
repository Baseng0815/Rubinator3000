using RubinatorCore.CubeRepresentation;
using System;
using System.Linq;

namespace RubinatorCore {
    public static class Utility {
        private static readonly int[] axisToFace = { 4, 3, 5 };

        public static byte[] MoveToByte(Move move) {
            //bool isPrime = move.IsPrime;
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
            CubeFace face = (CubeFace)((moveByte >> 1) - 1);
            int count = (moveByte & 0x01) == 1 ? -1 : 1;

            return new Move(face, count);
        }

        public static Move[] MultiTurnByteToMove(byte moveByte) {
            int axis = (moveByte & (1 << 3 | 1 << 2)) >> 2;

            int leftDir = (moveByte & (1 << 1)) >> 1;
            int rightDir = moveByte & (1 << 0);

            leftDir = leftDir == 0 ? 1 : -1;
            rightDir = rightDir == 0 ? 1 : -1;

            return new Move[] { new Move((CubeFace)axis, leftDir), new Move((CubeFace)axisToFace[axis], rightDir) };
        }
    }
}
