using System;

namespace MatrixMultiplicationApp.Models
{
    /// <summary>
    /// Клас для генерації матриць з випадковими значеннями
    /// </summary>
    public static class MatrixGenerator
    {
        private static readonly Random _random = new Random();

        /// <summary>
        /// Генерує матрицю заданого розміру з випадковими значеннями
        /// </summary>
        /// <param name="rows">Кількість рядків</param>
        /// <param name="columns">Кількість стовпців</param>
        /// <param name="minValue">Мінімальне значення</param>
        /// <param name="maxValue">Максимальне значення</param>
        /// <returns>Згенерована матриця</returns>
        public static Matrix Generate(int rows, int columns, double minValue = -10, double maxValue = 10)
        {
            if (rows <= 0 || columns <= 0)
                throw new ArgumentException("Розмірність матриці повинна бути додатньою");

            if (minValue >= maxValue)
                throw new ArgumentException("Мінімальне значення повинно бути менше за максимальне");

            Matrix matrix = new Matrix(rows, columns);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    matrix[i, j] = minValue + _random.NextDouble() * (maxValue - minValue);
                }
            }

            return matrix;
        }

        /// <summary>
        /// Генерує квадратну матрицю заданого розміру з випадковими значеннями
        /// </summary>
        /// <param name="size">Розмір матриці</param>
        /// <param name="minValue">Мінімальне значення</param>
        /// <param name="maxValue">Максимальне значення</param>
        /// <returns>Згенерована квадратна матриця</returns>
        public static Matrix GenerateSquare(int size, double minValue = -10, double maxValue = 10)
        {
            return Generate(size, size, minValue, maxValue);
        }

        /// <summary>
        /// Генерує квадратну матрицю з розміром, що є степенем 2
        /// (необхідно для алгоритму Штрассена)
        /// </summary>
        /// <param name="minSize">Мінімальний розмір (буде розширено до найближчого степеня 2)</param>
        /// <param name="minValue">Мінімальне значення</param>
        /// <param name="maxValue">Максимальне значення</param>
        /// <returns>Згенерована квадратна матриця з розміром, що є степенем 2</returns>
        public static Matrix GeneratePowerOfTwo(int minSize, double minValue = -10, double maxValue = 10)
        {
            // Знаходимо найближчий більший степінь 2
            int size = 1;
            while (size < minSize)
                size *= 2;

            return GenerateSquare(size, minValue, maxValue);
        }
    }
}
