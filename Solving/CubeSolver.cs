﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rubinator3000.Solving {
    public abstract class CubeSolver {

        protected Cube cube;
        protected MoveCollection moves;
        protected bool movesCalculated = false;

        /// <summary>
        /// Erstellt einen neuen CubeSolver und eine Kopie des aktuellen Cubes
        /// </summary>
        /// <param name="cube">Der zu lösende Cube</param>
        public CubeSolver(Cube cube) {
#if DEBUG
            this.cube = cube;
#else
            this.cube = Utility.DeepClone(cube);
#endif
            moves = new MoveCollection();
        }

        public MoveCollection GetMoves() {
            if (!movesCalculated)
                CalcMoves();

            return moves;
        }

        /// <summary>
        /// Brechnet in einer abgleiteten Klasse, die Züge zum lösen des Würfels
        /// </summary>
        protected abstract void CalcMoves();

        /// <summary>
        /// Gibt in einer abgeleiteten Klasse an, ob der Solver seine Arbeit erfolgreich vollendet hat
        /// </summary>
        public abstract bool Solved { get; }


        protected void DoMove(CubeFace face, int count = 1, bool addMove = true) {
            cube.DoMove(face, count);

            if (addMove)
                moves.Add(face, count);
        }

        /// <summary>
        /// Gibt zurück, ob der Würfel sich im gelösten Zustand befindet
        /// </summary>
        /// <returns>Den Wert, der angibt, ob der Würfel gelöst ist</returns>
        public bool GetCubeSolved() {
            for (int face = 0; face < 6; face++) {
                CubeColor faceColor = cube.At(face, 4);
                for (int tile = 0; tile < 9; tile++) {
                    if (faceColor != cube.At(face, tile))
                        return false;
                }
            }

            return true;
        }

        public static readonly Position[] EdgePositions;
        public static readonly CubeFace[] MiddleLayerFaces = { CubeFace.LEFT, CubeFace.FRONT, CubeFace.RIGHT, CubeFace.BACK };

        static CubeSolver() {
            EdgePositions = new Position[24];
            for (int face = 0; face < 6; face++) {
                for (int tile = 0; tile < 4; tile++) {
                    EdgePositions[4 * face + tile] = new Position((CubeFace)face, tile * 2 + 1);
                }
            }
        }
    }
}