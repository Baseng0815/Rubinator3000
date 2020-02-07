using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static RubinatorCore.CubeColor;
using static RubinatorCore.CubeFace;

namespace RubinatorCore.Solving {
    public abstract class CubeSolver {
        /// <summary>
        /// Eine Kopie des Würfels, welcher gelöst wird
        /// </summary>
        protected readonly Cube cube;

        /// <summary>
        /// Die Züge, die zum Lösen des Würfels benötigt werden
        /// </summary>
        public MoveCollection SolvingMoves { get; private set; }

        /// <summary>
        /// Erstellt einen neuen CubeSolver mit einer Kopie des aktuellen Würfels
        /// </summary>
        /// <param name="cube">Der zu lösende Würfel</param>
        public CubeSolver(Cube cube) {
            // überprüfen, ob der Würfel den Anforderungen des Solvers entspricht
            if (!CheckCube(cube)) {
                throw new ArgumentException();
            }

            this.cube = cube;
            SolvingMoves = new MoveCollection();
        }

        /// <summary>
        /// Brechnet in einer abgleiteten Klasse, die Züge zum Lösen des Würfels
        /// </summary>
        public abstract void SolveCube();

        /// <summary>
        /// Berechnet die Züge zum Lösen asynchron
        /// </summary>        
        /// <returns>Die Task, die den Vorgang des asynchrone Lösen darstellt</returns>
        public virtual Task SolveCubeAsync() {
            return Task.Run(SolveCube);
        }

        protected abstract bool CheckCube(Cube cube);

        /// <summary>
        /// Gibt in einer abgeleiteten Klasse an, ob der Solver seine Arbeit erfolgreich vollendet hat
        /// </summary>
        public abstract bool Solved { get; }

        /// <summary>
        /// Führt einen Move aus und fügt ihn den SolvingMoves hinzu
        /// </summary>
        /// <param name="move">Der Zug, der ausgeführt werden soll</param>
        protected void DoMove(Move move) {
            // die Methode verlassen, wenn die Anzahl der Drehnungen 0 ist
            if (move.Count == 0)
                return;

            // den Zug ausführen und ihn den SolvingMoves hinzufügen
            cube.DoMove(move);
            SolvingMoves.Add(move);
        }

        /// <summary>
        /// Führt mehrere Züge aus und fügt diese den SolvingMoves hinzu
        /// </summary>
        /// <param name="moves"></param>
        protected void DoMoves(IEnumerable<Move> moves) {
            foreach (var move in moves) {
                DoMove(move);
            }
        }

        /// <summary>
        /// Gibt zurück, ob der Würfel sich im gelösten Zustand befindet
        /// </summary>
        /// <returns>Einen Wert, der angibt, ob der Würfel gelöst ist</returns>
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

        /// <summary>
        /// Die Seiten des Würfels, die sich auf der mittleren Ebene befinden
        /// </summary>
        public static readonly CubeFace[] MiddleLayerFaces = { LEFT, FRONT, RIGHT, BACK };

        /// <summary>
        /// Die Farbenpaare der Kantensteine der mittleren Ebene
        /// </summary>
        public static readonly Tuple<CubeColor, CubeColor>[] MiddleLayerEdgesColors = new Tuple<CubeColor, CubeColor>[4] {
            new Tuple<CubeColor, CubeColor>(ORANGE, GREEN),
            new Tuple<CubeColor, CubeColor>(GREEN, RED),
            new Tuple<CubeColor, CubeColor>(RED, BLUE),
            new Tuple<CubeColor, CubeColor>(BLUE, ORANGE)
        };
    }
}
