using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rubinator3000 {
    [Serializable]
    public class CubeMatrix {
        private CubeColor[,] arr;

        public CubeColor this[int tile] {
            get {
                if (tile < 0 || tile >= arr.Length)
                    throw new IndexOutOfRangeException();

                return arr[tile / 3, tile % 3];
            }
            set {
                if (tile < 0 || tile >= arr.Length)
                    throw new IndexOutOfRangeException();

                arr[tile / 3, tile % 3] = value;
            }
        }

        public CubeMatrix(CubeColor color) {
            arr = new CubeColor[3, 3];

            for (int i = 0; i < 3; i++) {
                for (int j = 0; j < 3; j++) {
                    arr[i, j] = color;
                }
            }
        }

        public CubeMatrix() : this(CubeColor.NONE) { }

        public void Rotate() {
            // clockwise
            List<RowMatrix> rows = new List<RowMatrix>(GetRows());
            rows.Reverse();

            SetColumns(from row in rows
                       select (ColumnMatrix)row.GetTranspose());
        }

        public RowMatrix GetRow(int index) {
            if (index < 0 || index >= 3)
                throw new IndexOutOfRangeException();

            CubeColor[] row = new CubeColor[3];
            for (int i = 0; i < 3; i++) {
                row[i] = arr[index, i];
            }

            return row;
        }

        public IEnumerable<RowMatrix> GetRows() {
            for (int i = 0; i < 3; i++) {
                yield return GetRow(i);
            }
        }

        public ColumnMatrix GetColumn(int index) {
            if (index < 0 || index >= 3)
                throw new IndexOutOfRangeException();

            CubeColor[] column = new CubeColor[3];
            for (int i = 0; i < 3; i++) {
                column[i] = arr[i, index];
            }

            return column;
        }

        public IEnumerable<ColumnMatrix> GetColumns() {
            for (int i = 0; i < 3; i++) {
                yield return GetColumn(i);
            }
        }

        public void SetRow(int index, RowMatrix rowMatrix) {
            if (index < 0 || index >= 3)
                throw new IndexOutOfRangeException();


            for (int i = 0; i < 3; i++) {
                arr[index, i] = rowMatrix[i];
            }
        }

        private void SetRows(IEnumerable<RowMatrix> rows) {
            if (rows.Count() != 3)
                throw new ArgumentOutOfRangeException(nameof(rows));

            for (int i = 0; i < 3; i++) {
                SetRow(i, rows.ElementAt(i));
            }
        }

        public void SetColumn(int index, ColumnMatrix columnMatrix) {
            if (index < 0 || index >= 3)
                throw new IndexOutOfRangeException();


            for (int i = 0; i < 3; i++) {
                arr[i, index] = columnMatrix[i];
            }
        }

        private void SetColumns(IEnumerable<ColumnMatrix> columns) {
            if (columns.Count() != 3)
                throw new ArgumentOutOfRangeException(nameof(columns));

            for (int i = 0; i < 3; i++) {
                SetColumn(i, columns.ElementAt(i));
            }
        }
    }

    public interface ISubmatrix {
        ISubmatrix GetReverse();
        ISubmatrix GetTranspose();
    }

    public struct RowMatrix : ISubmatrix {
        private CubeColor[] row;
        public CubeColor this[int i] {
            get {
                if (i < 0 || row.Length <= i)
                    throw new IndexOutOfRangeException();

                return row[i];
            }
            set {
                if (i < 0 || row.Length >= i)
                    throw new IndexOutOfRangeException();

                row[i] = value;
            }
        }

        public RowMatrix(CubeColor[] row) {
            this.row = row;
        }

        public ISubmatrix GetTranspose() {
            return new ColumnMatrix(row);
        }

        public ISubmatrix GetReverse() {
            return new RowMatrix(row.Reverse().ToArray());
        }

        public static implicit operator RowMatrix(CubeColor[] row) => new RowMatrix(row);
    }

    public struct ColumnMatrix : ISubmatrix {
        private CubeColor[] column;
        public CubeColor this[int i] {
            get {
                if (i < 0 || column.Length <= i)
                    throw new IndexOutOfRangeException();

                return column[i];
            }
            set {
                if (i < 0 || column.Length >= i)
                    throw new IndexOutOfRangeException();

                column[i] = value;
            }
        }

        public ColumnMatrix(CubeColor[] column) {
            this.column = column;
        }

        public ISubmatrix GetTranspose() {
            return new RowMatrix(column);
        }

        public ISubmatrix GetReverse() {
            return new ColumnMatrix(column.Reverse().ToArray());
        }

        public static implicit operator ColumnMatrix(CubeColor[] column) => new ColumnMatrix(column);
    }
}
