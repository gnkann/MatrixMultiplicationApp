using System;
using System.Diagnostics;

namespace MatrixMultiplicationApp.Models
{
    /// <summary>
    /// Перелік методів множення матриць
    /// </summary>
    public enum MultiplicationMethod
    {
        Traditional,
        Strassen,
        WinogradStrassen
    }

    /// <summary>
    /// Результат множення матриць з інформацією про продуктивність
    /// </summary>
    public class MultiplicationResult
    {
        /// <summary>
        /// Результуюча матриця
        /// </summary>
        public Matrix ResultMatrix { get; }

        /// <summary>
        /// Час виконання в мілісекундах
        /// </summary>
        public long ExecutionTimeMs { get; }

        /// <summary>
        /// Використаний метод множення
        /// </summary>
        public MultiplicationMethod Method { get; }

        /// <summary>
        /// Кількість операцій множення
        /// </summary>
        public long MultiplicationCount { get; }

        /// <summary>
        /// Кількість операцій додавання/віднімання
        /// </summary>
        public long AdditionCount { get; }

        /// <summary>
        /// Оригінальний розмір матриць (до розширення)
        /// </summary>
        public int OriginalSize { get; }

        /// <summary>
        /// Фактичний розмір матриць (після розширення для Штрассена)
        /// </summary>
        public int ActualSize { get; }

        public MultiplicationResult(Matrix resultMatrix, long executionTimeMs, MultiplicationMethod method,
            long multiplicationCount, long additionCount, int originalSize, int actualSize)
        {
            ResultMatrix = resultMatrix;
            ExecutionTimeMs = executionTimeMs;
            Method = method;
            MultiplicationCount = multiplicationCount;
            AdditionCount = additionCount;
            OriginalSize = originalSize;
            ActualSize = actualSize;
        }
    }

    /// <summary>
    /// Клас для множення матриць різними методами
    /// </summary>
    public static class MatrixMultiplier
    {
        private static long _multiplicationCount;
        private static long _additionCount;

        /// <summary>
        /// Множить матриці вибраним методом і повертає результат з інформацією про продуктивність
        /// </summary>
        public static MultiplicationResult Multiply(Matrix a, Matrix b, MultiplicationMethod method)
        {
            if (a == null || b == null)
                throw new ArgumentNullException(a == null ? nameof(a) : nameof(b));

            if (a.Columns != b.Rows)
                throw new ArgumentException("Кількість стовпців першої матриці повинна дорівнювати кількості рядків другої матриці");

            // Запам'ятовуємо оригінальний розмір
            int originalSize = a.Rows;

            // Скидаємо лічильники операцій
            _multiplicationCount = 0;
            _additionCount = 0;

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            Matrix result;

            switch (method)
            {
                case MultiplicationMethod.Traditional:
                    result = MultiplyTraditional(a, b);
                    break;
                case MultiplicationMethod.Strassen:
                    result = MultiplyStrassen(a, b);
                    break;
                case MultiplicationMethod.WinogradStrassen:
                    result = MultiplyWinogradStrassen(a, b);
                    break;
                default:
                    throw new ArgumentException($"Невідомий метод множення: {method}");
            }

            stopwatch.Stop();

            // Витягуємо результат оригінального розміру
            Matrix finalResult = result;
            if (result.Rows > originalSize || result.Columns > originalSize)
            {
                finalResult = result.Submatrix(0, 0, originalSize, originalSize);
            }

            return new MultiplicationResult(finalResult, stopwatch.ElapsedMilliseconds, method,
                _multiplicationCount, _additionCount, originalSize, result.Rows);
        }

        /// <summary>
        /// Традиційний метод множення матриць
        /// </summary>
        private static Matrix MultiplyTraditional(Matrix a, Matrix b)
        {
            int rows = a.Rows;
            int columns = b.Columns;
            int innerDimension = a.Columns;

            Matrix result = new Matrix(rows, columns);

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    double sum = 0;
                    for (int k = 0; k < innerDimension; k++)
                    {
                        sum += a[i, k] * b[k, j];
                        _multiplicationCount++; // Підраховуємо множення
                        if (k > 0) _additionCount++; // Підраховуємо додавання (крім першої ітерації)
                    }
                    result[i, j] = sum;
                }
            }

            return result;
        }

        /// <summary>
        /// Метод Штрассена для множення матриць
        /// </summary>
        private static Matrix MultiplyStrassen(Matrix a, Matrix b)
        {
            // Базовий випадок: матриці 1x1
            if (a.Rows == 1 && a.Columns == 1 && b.Rows == 1 && b.Columns == 1)
            {
                _multiplicationCount++; // Одне множення
                return new Matrix(new double[,] { { a[0, 0] * b[0, 0] } });
            }

            // Перевіряємо, чи є матриці квадратними
            int n = a.Rows;
            if (n != a.Columns || n != b.Rows || n != b.Columns)
            {
                return MultiplyTraditional(a, b);
            }

            // Якщо розмір не є степенем 2, розширюємо матриці
            int powerOf2Size = 1;
            while (powerOf2Size < n)
                powerOf2Size *= 2;

            Matrix expandedA = a;
            Matrix expandedB = b;

            if (n != powerOf2Size)
            {
                expandedA = new Matrix(powerOf2Size, powerOf2Size);
                expandedB = new Matrix(powerOf2Size, powerOf2Size);

                // Копіюємо оригінальні матриці в розширені
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        expandedA[i, j] = a[i, j];
                        expandedB[i, j] = b[i, j];
                    }
                }
                n = powerOf2Size;
            }

            // Для невеликих матриць використовуємо традиційний метод
            if (n <= 64)
            {
                return MultiplyTraditional(expandedA, expandedB);
            }

            // Розбиваємо матриці на 4 підматриці
            int halfSize = n / 2;

            Matrix a11 = expandedA.Submatrix(0, 0, halfSize, halfSize);
            Matrix a12 = expandedA.Submatrix(0, halfSize, halfSize, halfSize);
            Matrix a21 = expandedA.Submatrix(halfSize, 0, halfSize, halfSize);
            Matrix a22 = expandedA.Submatrix(halfSize, halfSize, halfSize, halfSize);

            Matrix b11 = expandedB.Submatrix(0, 0, halfSize, halfSize);
            Matrix b12 = expandedB.Submatrix(0, halfSize, halfSize, halfSize);
            Matrix b21 = expandedB.Submatrix(halfSize, 0, halfSize, halfSize);
            Matrix b22 = expandedB.Submatrix(halfSize, halfSize, halfSize, halfSize);

            // Обчислюємо 7 допоміжних матриць
            Matrix p1 = MultiplyStrassen(AddMatrices(a11, a22), AddMatrices(b11, b22));
            Matrix p2 = MultiplyStrassen(AddMatrices(a21, a22), b11);
            Matrix p3 = MultiplyStrassen(a11, SubtractMatrices(b12, b22));
            Matrix p4 = MultiplyStrassen(a22, SubtractMatrices(b21, b11));
            Matrix p5 = MultiplyStrassen(AddMatrices(a11, a12), b22);
            Matrix p6 = MultiplyStrassen(SubtractMatrices(a21, a11), AddMatrices(b11, b12));
            Matrix p7 = MultiplyStrassen(SubtractMatrices(a12, a22), AddMatrices(b21, b22));

            // Обчислюємо підматриці результату
            Matrix c11 = AddMatrices(SubtractMatrices(AddMatrices(p1, p4), p5), p7);
            Matrix c12 = AddMatrices(p3, p5);
            Matrix c21 = AddMatrices(p2, p4);
            Matrix c22 = AddMatrices(SubtractMatrices(AddMatrices(p1, p3), p2), p6);

            // Складаємо результат з підматриць
            return Matrix.Combine(c11, c12, c21, c22);
        }

        /// <summary>
        /// Метод Винограда-Штрассена для множення матриць
        /// </summary>
        private static Matrix MultiplyWinogradStrassen(Matrix a, Matrix b)
        {
            // Базовий випадок: матриці 1x1
            if (a.Rows == 1 && a.Columns == 1 && b.Rows == 1 && b.Columns == 1)
            {
                _multiplicationCount++; // Одне множення
                return new Matrix(new double[,] { { a[0, 0] * b[0, 0] } });
            }

            // Перевіряємо, чи є матриці квадратними
            int n = a.Rows;
            if (n != a.Columns || n != b.Rows || n != b.Columns)
            {
                return MultiplyTraditional(a, b);
            }

            // Якщо розмір не є степенем 2, розширюємо матриці
            int powerOf2Size = 1;
            while (powerOf2Size < n)
                powerOf2Size *= 2;

            Matrix expandedA = a;
            Matrix expandedB = b;

            if (n != powerOf2Size)
            {
                expandedA = new Matrix(powerOf2Size, powerOf2Size);
                expandedB = new Matrix(powerOf2Size, powerOf2Size);

                // Копіюємо оригінальні матриці в розширені
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        expandedA[i, j] = a[i, j];
                        expandedB[i, j] = b[i, j];
                    }
                }
                n = powerOf2Size;
            }

            // Для малих матриць використовуємо стандартний алгоритм
            if (n <= 32)
            {
                return MultiplyTraditional(expandedA, expandedB);
            }

            // Розбиваємо матриці на 4 підматриці
            int halfSize = n / 2;

            Matrix a11 = expandedA.Submatrix(0, 0, halfSize, halfSize);
            Matrix a12 = expandedA.Submatrix(0, halfSize, halfSize, halfSize);
            Matrix a21 = expandedA.Submatrix(halfSize, 0, halfSize, halfSize);
            Matrix a22 = expandedA.Submatrix(halfSize, halfSize, halfSize, halfSize);

            Matrix b11 = expandedB.Submatrix(0, 0, halfSize, halfSize);
            Matrix b12 = expandedB.Submatrix(0, halfSize, halfSize, halfSize);
            Matrix b21 = expandedB.Submatrix(halfSize, 0, halfSize, halfSize);
            Matrix b22 = expandedB.Submatrix(halfSize, halfSize, halfSize, halfSize);

            // Обчислюємо проміжні суми (оптимізація Винограда)
            Matrix s1 = AddMatrices(a21, a22);
            Matrix s2 = SubtractMatrices(s1, a11);
            Matrix s3 = SubtractMatrices(a11, a21);
            Matrix s4 = SubtractMatrices(a12, s2);

            Matrix t1 = SubtractMatrices(b12, b11);
            Matrix t2 = SubtractMatrices(b22, t1);
            Matrix t3 = SubtractMatrices(b22, b12);
            Matrix t4 = SubtractMatrices(b21, t2);

            // Обчислюємо 7 допоміжних добутків
            Matrix p1 = MultiplyWinogradStrassen(a11, b11);
            Matrix p2 = MultiplyWinogradStrassen(a12, b21);
            Matrix p3 = MultiplyWinogradStrassen(s1, t1);
            Matrix p4 = MultiplyWinogradStrassen(s2, t2);
            Matrix p5 = MultiplyWinogradStrassen(s3, t3);
            Matrix p6 = MultiplyWinogradStrassen(s4, b22);
            Matrix p7 = MultiplyWinogradStrassen(a22, t4);

            // Обчислюємо підматриці результату за оптимізованими формулами
            Matrix c11 = AddMatrices(p1, p2);
            Matrix c12 = AddMatrices(AddMatrices(p1, p3), p6);
            Matrix c21 = AddMatrices(AddMatrices(p1, p4), p7);
            Matrix c22 = AddMatrices(AddMatrices(AddMatrices(p1, p3), p4), p5);

            // Складаємо результат з підматриць
            return Matrix.Combine(c11, c12, c21, c22);
        }

        /// <summary>
        /// Додавання матриць з підрахунком операцій
        /// </summary>
        private static Matrix AddMatrices(Matrix a, Matrix b)
        {
            _additionCount += a.Rows * a.Columns; // Підраховуємо операції додавання
            return a + b;
        }

        /// <summary>
        /// Віднімання матриць з підрахунком операцій
        /// </summary>
        private static Matrix SubtractMatrices(Matrix a, Matrix b)
        {
            _additionCount += a.Rows * a.Columns; // Віднімання також вважаємо як додавання
            return a - b;
        }
    }
}