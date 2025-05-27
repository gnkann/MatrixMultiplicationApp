using System;
using System.Text;

namespace MatrixMultiplicationApp.Models
{
    /// <summary>
    /// Клас для роботи з матрицями
    /// </summary>
    public partial class Matrix
    {
        private readonly double[,] _data;

        /// <summary>
        /// Кількість рядків у матриці
        /// </summary>
        public int Rows { get; }

        /// <summary>
        /// Кількість стовпців у матриці
        /// </summary>
        public int Columns { get; }

        /// <summary>
        /// Створює нову матрицю заданого розміру
        /// </summary>
        public Matrix(int rows, int columns)
        {
            if (rows <= 0 || columns <= 0)
                throw new ArgumentException("Розмірність матриці повинна бути додатньою");

            Rows = rows;
            Columns = columns;
            _data = new double[rows, columns];
        }

        /// <summary>
        /// Створює нову матрицю на основі двовимірного масиву
        /// </summary>
        public Matrix(double[,] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            Rows = data.GetLength(0);
            Columns = data.GetLength(1);
            _data = (double[,])data.Clone();
        }

        /// <summary>
        /// Індексатор для доступу до елементів матриці
        /// </summary>
        public double this[int row, int column]
        {
            get
            {
                ValidateIndices(row, column);
                return _data[row, column];
            }
            set
            {
                ValidateIndices(row, column);
                _data[row, column] = value;
            }
        }

        /// <summary>
        /// Перевіряє коректність індексів
        /// </summary>
        private void ValidateIndices(int row, int column)
        {
            if (row < 0 || row >= Rows)
                throw new IndexOutOfRangeException($"Індекс рядка {row} виходить за межі матриці розміром {Rows}x{Columns}");

            if (column < 0 || column >= Columns)
                throw new IndexOutOfRangeException($"Індекс стовпця {column} виходить за межі матриці розміром {Rows}x{Columns}");
        }

        /// <summary>
        /// Створює підматрицю
        /// </summary>
        public Matrix Submatrix(int startRow, int startColumn, int rows, int columns)
        {
            if (startRow < 0 || startRow + rows > Rows || startColumn < 0 || startColumn + columns > Columns)
                throw new ArgumentException("Неприпустимі межі підматриці");

            Matrix result = new Matrix(rows, columns);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    result[i, j] = this[startRow + i, startColumn + j];
                }
            }

            return result;
        }

        /// <summary>
        /// Об'єднує чотири підматриці в одну матрицю розміром 2n x 2n
        /// </summary>
        public static Matrix Combine(Matrix topLeft, Matrix topRight, Matrix bottomLeft, Matrix bottomRight)
        {
            int rows = topLeft.Rows + bottomLeft.Rows;
            int columns = topLeft.Columns + topRight.Columns;

            Matrix result = new Matrix(rows, columns);

            // Копіювання верхнього лівого квадранта
            for (int i = 0; i < topLeft.Rows; i++)
            {
                for (int j = 0; j < topLeft.Columns; j++)
                {
                    result[i, j] = topLeft[i, j];
                }
            }

            // Копіювання верхнього правого квадранта
            for (int i = 0; i < topRight.Rows; i++)
            {
                for (int j = 0; j < topRight.Columns; j++)
                {
                    result[i, topLeft.Columns + j] = topRight[i, j];
                }
            }

            // Копіювання нижнього лівого квадранта
            for (int i = 0; i < bottomLeft.Rows; i++)
            {
                for (int j = 0; j < bottomLeft.Columns; j++)
                {
                    result[topLeft.Rows + i, j] = bottomLeft[i, j];
                }
            }

            // Копіювання нижнього правого квадранта
            for (int i = 0; i < bottomRight.Rows; i++)
            {
                for (int j = 0; j < bottomRight.Columns; j++)
                {
                    result[topLeft.Rows + i, topLeft.Columns + j] = bottomRight[i, j];
                }
            }

            return result;
        }

        /// <summary>
        /// Додавання матриць
        /// </summary>
        public static Matrix operator +(Matrix a, Matrix b)
        {
            if (a.Rows != b.Rows || a.Columns != b.Columns)
                throw new ArgumentException("Розміри матриць повинні співпадати для додавання");

            Matrix result = new Matrix(a.Rows, a.Columns);

            for (int i = 0; i < a.Rows; i++)
            {
                for (int j = 0; j < a.Columns; j++)
                {
                    result[i, j] = a[i, j] + b[i, j];
                }
            }

            return result;
        }

        /// <summary>
        /// Віднімання матриць
        /// </summary>
        public static Matrix operator -(Matrix a, Matrix b)
        {
            if (a.Rows != b.Rows || a.Columns != b.Columns)
                throw new ArgumentException("Розміри матриць повинні співпадати для віднімання");

            Matrix result = new Matrix(a.Rows, a.Columns);

            for (int i = 0; i < a.Rows; i++)
            {
                for (int j = 0; j < a.Columns; j++)
                {
                    result[i, j] = a[i, j] - b[i, j];
                }
            }

            return result;
        }

        /// <summary>
        /// Отримує рядок з текстовим представленням матриці
        /// </summary>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    sb.Append(_data[i, j].ToString("F2").PadLeft(8));

                    if (j < Columns - 1)
                        sb.Append(" ");
                }

                if (i < Rows - 1)
                    sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <summary>
        /// Отримує скорочене текстове представлення матриці
        /// (для великих матриць показує тільки кутові елементи)
        /// </summary>
        public string ToShortString(int maxElements = 10)
        {
            if (Rows <= maxElements && Columns <= maxElements)
                return ToString();

            StringBuilder sb = new StringBuilder();

            int showRows = Math.Min(maxElements / 2, Rows / 2);
            int showCols = Math.Min(maxElements / 2, Columns / 2);

            // Верхні рядки
            for (int i = 0; i < showRows; i++)
            {
                // Ліві стовпці
                for (int j = 0; j < showCols; j++)
                {
                    sb.Append(_data[i, j].ToString("F2").PadLeft(8));
                    sb.Append(" ");
                }

                sb.Append(" ... ");

                // Праві стовпці
                for (int j = Columns - showCols; j < Columns; j++)
                {
                    sb.Append(_data[i, j].ToString("F2").PadLeft(8));

                    if (j < Columns - 1)
                        sb.Append(" ");
                }

                sb.AppendLine();
            }

            sb.AppendLine(" ...  ...  ... ");

            // Нижні рядки
            for (int i = Rows - showRows; i < Rows; i++)
            {
                // Ліві стовпці
                for (int j = 0; j < showCols; j++)
                {
                    sb.Append(_data[i, j].ToString("F2").PadLeft(8));
                    sb.Append(" ");
                }

                sb.Append(" ... ");

                // Праві стовпці
                for (int j = Columns - showCols; j < Columns; j++)
                {
                    sb.Append(_data[i, j].ToString("F2").PadLeft(8));

                    if (j < Columns - 1)
                        sb.Append(" ");
                }

                if (i < Rows - 1)
                    sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}