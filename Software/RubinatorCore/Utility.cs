﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RubinatorCore {
    public static class Utility {
        public static byte[] MoveToByte(Move move) {
            //bool isPrime = move.IsPrime;
            int face = (int)move.Face;

            byte moveData = (byte)((face + 1) << 1);

            if (move.Direction == -1) {
                moveData |= 0x01;
            }

            return Enumerable.Repeat(moveData, Math.Abs(move.Count)).ToArray();
        }

        public static byte[] MulitTurnMoveToByte(Move move1, Move move2) {
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
            int minCount = Math.Min(leftMove.CountPositive, rightMove.CountPositive);
            int maxCount = Math.Max(leftMove.CountPositive, rightMove.CountPositive);

            byte[] data = new byte[maxCount];

            // fill array with data
            for (int i = 0; i < maxCount; i++) {
                if (i < minCount)
                    data[i] = moveByte;
                else if (i < leftMove.CountPositive) {
                    data[i] = MoveToByte(leftMove)[0];
                } else if (i < rightMove.CountPositive) {
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
    }
}