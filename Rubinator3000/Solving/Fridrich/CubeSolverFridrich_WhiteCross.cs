using System;
using System.Collections.Generic;
using System.Linq;
using EdgeStone = Rubinator3000.Cube.EdgeStone;

namespace Rubinator3000{
    partial class CubeSolverFridrich {
        private void CalcCrossMoves() {
            if (CheckCross())
                return;

            RotateWhiteFace();

            while (!CheckCross()) {
                EdgeStone stone = (from edge in Cube.EdgeStones
                                   where cube.GetColors(edge).Contains(CubeColor.WHITE) && !cube.InRightPosition(edge)
                                   select edge).First();

                // fall 1: stein auf der gelben Ebene
                if (stone.Positions.Any(e => e.Face == CubeFace.DOWN)) {
                    // a) weiße Fläche auf der gelben Seite
                    if (stone.ColorOnFace(cube, CubeColor.WHITE, CubeFace.DOWN))
                        Cross_WhiteTileOnYellowFace(stone);

                    // b) weiße Fläche seitlich
                    else {
                        Cross_WhiteTileYellowLayer(stone);
                    }
                }
                // fall 3: stein auf der weißen Ebene
                else if (stone.Positions.Any(e => e.Face == CubeFace.UP)) {
                    // a) weiße Fläche auf der weißen Seite
                    if (stone.ColorOnFace(cube, CubeColor.WHITE, CubeFace.UP)) {
                        Cross_WhiteTileOnWhiteFace(stone);
                    }
                    // b) weiße Fläche seitlich
                    else {
                        Cross_WhiteTileWhiteLayer(stone);
                    }
                }
                //fall 2: stein auf der mittleren Ebene
                else {
                    Cross_WhiteTileMiddleLayer(stone);
                }
            }

        }

        /// <summary>
        /// Überprüft, ob das weiße Kreuz gelöst ist
        /// </summary>
        /// <param name="cube">Der zu überprüfende Würfel</param>
        /// <returns>Ob das weiße Kreuz gelöst ist</returns>
        private bool CheckCross() {
            return cube.At(CubeFace.UP, 1) == CubeColor.WHITE && cube.At(CubeFace.LEFT, 1) == CubeColor.ORANGE
                && cube.At(CubeFace.UP, 3) == CubeColor.WHITE && cube.At(CubeFace.FRONT, 1) == CubeColor.GREEN
                && cube.At(CubeFace.UP, 5) == CubeColor.WHITE && cube.At(CubeFace.RIGHT, 1) == CubeColor.RED
                && cube.At(CubeFace.UP, 7) == CubeColor.WHITE && cube.At(CubeFace.BACK, 1) == CubeColor.BLUE;
        }

        /// <summary>
        /// Rotiert die weiße Seite so, dass möglichst viele weißen Steine in der richtigen Position sind
        /// </summary>
        /// <param name="cube"></param>
        private void RotateWhiteFace() {
            // get all white stones on the white face
            IEnumerable<Cube.EdgeStone> whiteStonesUpperLayer = from edge in Cube.EdgeStones
                                                                where cube.GetColors(edge).Contains(CubeColor.WHITE)
                                                                    && cube.GetPosition(edge, CubeColor.WHITE).Face == CubeFace.UP
                                                                select edge;

            if (whiteStonesUpperLayer.Count() == 0)
                return;

            // do U Moves and count the right stones
            int[] rightStonesCounts = new int[4];

            for (int i = 0; i < 4; i++) {
                rightStonesCounts[i] = (from edge in Cube.EdgeStones
                                        where cube.GetColors(edge).Contains(CubeColor.WHITE) && cube.InRightPosition(edge)
                                        select edge).Count();

                cube.DoMove(CubeFace.UP);
            }

            // rotate the white face in the right position where the most white stones are right
            int movesCount = Array.IndexOf(rightStonesCounts, rightStonesCounts.Max());

            DoMove(CubeFace.UP, count: movesCount);
        }

        private int GetOffset(CubeColor color, CubeFace face) {
            int[] faces = { 0, 2, 4, 5 };

            int offset = Array.IndexOf(faces, (int)color) - Array.IndexOf(faces, (int)face);
            if(offset < 0) {
                offset += 4;
            }
            return offset;
        }

        // Fall 1a (RubinatorReference.docx)
        private void Cross_WhiteTileOnYellowFace(EdgeStone edge) {
            CubeColor color = cube.GetColors(edge).First(e => e != CubeColor.WHITE);

            // 1
            while (!edge.ColorOnFace(cube, color, (CubeFace)(int)color)) {
                DoMove(CubeFace.DOWN);

                edge = cube.GetEdge(CubeColor.WHITE, color);
            }

            // 2
            CubeFace faceToRot = edge.Positions.First(e => cube.At(e) != CubeColor.WHITE).Face;

            DoMove(faceToRot, count: 2);
        }

        // Fall 1b
        private void Cross_WhiteTileYellowLayer(EdgeStone edge) {
            CubeColor color = cube.GetColors(edge).First(e => e != CubeColor.WHITE);

            // 1
            while (!edge.ColorOnFace(cube, CubeColor.WHITE, (CubeFace)(int)color)) {
                DoMove(CubeFace.DOWN);

                edge = cube.GetEdge(CubeColor.WHITE, color);
            }

            // 2
            CubeFace faceToRot = (CubeFace)(int)color;
            DoMove(faceToRot);
            DoMove(CubeFace.UP);

            // 3
            edge = cube.GetEdge(CubeColor.WHITE, color);

            CubeFace neighborFace = cube.GetPosition(edge, color).Face;
            DoMove(neighborFace, true);

            // 4
            DoMove(CubeFace.UP, true);
        }

        // Fall 2a
        private void Cross_WhiteTileMiddleLayer(EdgeStone edge) {
            CubeColor color = cube.GetColors(edge).First(e => e != CubeColor.WHITE);
            Position colorPosition = cube.GetPosition(edge, color);

            // 1
            int offset = GetOffset(color, colorPosition.Face);

            for (int i = 0; i < offset; i++) {
                DoMove(CubeFace.UP);
            }

            // 2
            bool isPrime = colorPosition.Tile == 5;

            DoMove(colorPosition.Face, isPrime);

            // 3
            for (int i = 0; i < offset; i++) {
                DoMove(CubeFace.UP, true);
            }
        }

        // Fall 3a
        private void Cross_WhiteTileOnWhiteFace(EdgeStone edge) {
            CubeColor color = cube.GetColors(edge).First(e => e != CubeColor.WHITE);
            Position colorPosition = cube.GetPosition(edge, color);

            // 1
            DoMove(colorPosition.Face);


            // 2
            int offset = GetOffset(color, colorPosition.Face);

            for (int i = 0; i < offset; i++) {
                DoMove(CubeFace.UP);
            }

            // 3
            DoMove(colorPosition.Face, true);

            //4
            for (int i = 0; i < offset; i++) {
                DoMove(CubeFace.UP, true);
            }
        }

        // Fall 3b
        private void Cross_WhiteTileWhiteLayer(EdgeStone edge) {
            CubeColor color = cube.GetColors(edge).First(e => e != CubeColor.WHITE);
            Position whitePosition = cube.GetPosition(edge, CubeColor.WHITE);

            // 1
            DoMove(whitePosition.Face);


            // 2
            edge = cube.GetEdge(CubeColor.WHITE, color);
            Position colorPosition = cube.GetPosition(edge, color);

            int offset = GetOffset(color, colorPosition.Face);

            for (int i = 0; i < offset; i++) {
                DoMove(CubeFace.UP);
            }

            // 3
            DoMove(colorPosition.Face, true);

            //4
            for (int i = 0; i < offset; i++) {
                DoMove(CubeFace.UP, true);
            }
        }
    }
}