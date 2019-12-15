using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rubinator3000 {
    /// <summary>
    /// Stellt eine Seite des Würfels dar
    /// </summary>
    public struct CubeSide {
        /// <summary>
        /// Die Seite, die dargestellt wird
        /// </summary>
        public CubeFace Face { get; }

        /// <summary>
        /// Die Zeilen und Spalten auf den anderen Seiten, die zu dieser Seite gehören.
        /// </summary>
        public (int Face, int Index, bool IsColumn, int Direction)[] Submatices { get; }

        /// <summary>
        /// Erstellt eine neue Instanz der <see cref="CubeSide"/>
        /// </summary>
        /// <param name="face">Die Seite, die dargestellt wird</param>
        /// <param name="submatices">Die Zeilen und Spalten der zugehörigen Seiten im Uhrzeigersinn</param>
        public CubeSide(CubeFace face, params (int face, int index, bool isColumn, int direction)[] submatices) {
            if (submatices == null)
                throw new ArgumentNullException(nameof(submatices));

            if (submatices.Length != 4)
                throw new ArgumentOutOfRangeException(nameof(submatices));

            if (face == CubeFace.NONE || (int)face == 6)
                throw new ArgumentOutOfRangeException(nameof(face));

            Face = face;
            Submatices = submatices;
        }        

        public ISubmatrix GetSubmatrix(CubeMatrix matrix, int submatrixIndex) {
            if (submatrixIndex < 0 || submatrixIndex > 3)
                throw new IndexOutOfRangeException();

            var submatrix = Submatices[submatrixIndex];
            if (submatrix.IsColumn)
                return matrix.GetColumn(submatrix.Index);
            else
                return matrix.GetRow(submatrix.Index);
        }
    }
}
