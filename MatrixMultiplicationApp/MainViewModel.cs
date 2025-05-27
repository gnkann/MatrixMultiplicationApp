using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MatrixMultiplicationApp.Models;
using Microsoft.Win32;

namespace MatrixMultiplicationApp.ViewModels
{
    /// <summary>
    /// ViewModel для головного вікна програми
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        #region Приватні поля

        private int _matrixRows = 64;
        private int _matrixColumns = 64;
        private double _minValue = -10;
        private double _maxValue = 10;
        private MultiplicationMethod _selectedMethod = MultiplicationMethod.Traditional;
        private Matrix _matrixA;
        private Matrix _matrixB;
        private Matrix _resultMatrix;
        private string _matrixAText = string.Empty;
        private string _matrixBText = string.Empty;
        private string _resultMatrixText = string.Empty;
        private string _performanceResultsText = string.Empty;
        private string _statusText = "Готово до роботи";
        private string _executionTimeText = string.Empty;
        private Dictionary<MultiplicationMethod, MultiplicationResult> _executionResults = new Dictionary<MultiplicationMethod, MultiplicationResult>();

        #endregion

        #region Властивості

        public int MatrixRows
        {
            get => _matrixRows;
            set
            {
                if (value < 64)
                    value = 64;

                if (_matrixRows != value)
                {
                    _matrixRows = value;
                    // Синхронізуємо з MatrixColumns для квадратної матриці
                    if (_matrixColumns != value)
                    {
                        _matrixColumns = value;
                        OnPropertyChanged(nameof(MatrixColumns));
                    }
                    OnPropertyChanged();
                }
            }
        }

        public int MatrixColumns
        {
            get => _matrixColumns;
            set
            {
                if (value < 64)
                    value = 64;

                if (_matrixColumns != value)
                {
                    _matrixColumns = value;
                    // Синхронізуємо з MatrixRows для квадратної матриці
                    if (_matrixRows != value)
                    {
                        _matrixRows = value;
                        OnPropertyChanged(nameof(MatrixRows));
                    }
                    OnPropertyChanged();
                }
            }
        }

        public double MinValue
        {
            get => _minValue;
            set
            {
                if (value < -999)
                    value = -999;
                if (value > 9999)
                    value = 9999;

                if (_minValue != value)
                {
                    _minValue = value;
                    OnPropertyChanged();
                }
            }
        }

        public double MaxValue
        {
            get => _maxValue;
            set
            {
                if (value < -999)
                    value = -999;
                if (value > 9999)
                    value = 9999;

                if (_maxValue != value)
                {
                    _maxValue = value;
                    OnPropertyChanged();
                }
            }
        }

        public MultiplicationMethod SelectedMethod
        {
            get => _selectedMethod;
            set
            {
                if (_selectedMethod != value)
                {
                    _selectedMethod = value;
                    OnPropertyChanged();
                }
            }
        }

        public MultiplicationMethod[] MultiplicationMethods { get; } = (MultiplicationMethod[])Enum.GetValues(typeof(MultiplicationMethod));

        public string MatrixAText
        {
            get => _matrixAText;
            private set
            {
                if (_matrixAText != value)
                {
                    _matrixAText = value;
                    OnPropertyChanged();
                }
            }
        }

        public string MatrixBText
        {
            get => _matrixBText;
            private set
            {
                if (_matrixBText != value)
                {
                    _matrixBText = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ResultMatrixText
        {
            get => _resultMatrixText;
            private set
            {
                if (_resultMatrixText != value)
                {
                    _resultMatrixText = value;
                    OnPropertyChanged();
                }
            }
        }

        public string PerformanceResultsText
        {
            get => _performanceResultsText;
            private set
            {
                if (_performanceResultsText != value)
                {
                    _performanceResultsText = value;
                    OnPropertyChanged();
                }
            }
        }

        public string StatusText
        {
            get => _statusText;
            private set
            {
                if (_statusText != value)
                {
                    _statusText = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ExecutionTimeText
        {
            get => _executionTimeText;
            private set
            {
                if (_executionTimeText != value)
                {
                    _executionTimeText = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        #region Команди

        public ICommand GenerateMatricesCommand { get; }
        public ICommand MultiplyMatricesCommand { get; }
        public ICommand SaveResultsCommand { get; }
        public ICommand ClearCommand { get; }

        #endregion

        public MainViewModel()
        {
            GenerateMatricesCommand = new RelayCommand(GenerateMatrices, CanGenerateMatrices);
            MultiplyMatricesCommand = new RelayCommand(MultiplyMatrices, CanMultiplyMatrices);
            SaveResultsCommand = new RelayCommand(SaveResults, CanSaveResults);
            ClearCommand = new RelayCommand(Clear, CanClear);
        }

        #region Методи команд

        private void GenerateMatrices(object parameter)
        {
            try
            {
                if (MinValue >= MaxValue)
                {
                    StatusText = "Помилка: мінімальне значення повинно бути менше за максимальне";
                    return;
                }

                int rows = MatrixRows;
                int columns = MatrixColumns;

                bool isSquare = rows == columns;
                bool needsPowerOf2 = isSquare && (SelectedMethod == MultiplicationMethod.Strassen ||
                                                SelectedMethod == MultiplicationMethod.WinogradStrassen);

                if (needsPowerOf2)
                {
                    int powerOf2Size = 64;
                    while (powerOf2Size < Math.Max(rows, columns))
                        powerOf2Size *= 2;

                    if (rows != powerOf2Size || columns != powerOf2Size)
                    {
                        StatusText = $"Для методів Штрассена розмірність розширено до {powerOf2Size}×{powerOf2Size}";
                        rows = columns = powerOf2Size;
                    }
                }

                _matrixA = MatrixGenerator.Generate(rows, columns, MinValue, MaxValue);
                _matrixB = MatrixGenerator.Generate(columns, rows, MinValue, MaxValue);

                MatrixAText = _matrixA.ToShortString();
                MatrixBText = _matrixB.ToShortString();
                ResultMatrixText = string.Empty;
                PerformanceResultsText = string.Empty;

                StatusText = $"Згенеровано матриці розміром {_matrixA.Rows}×{_matrixA.Columns} та {_matrixB.Rows}×{_matrixB.Columns}";
                ExecutionTimeText = string.Empty;

                _resultMatrix = null;
                _executionResults.Clear();
            }
            catch (Exception ex)
            {
                StatusText = $"Помилка: {ex.Message}";
            }
        }

        private bool CanGenerateMatrices(object parameter)
        {
            return MatrixRows >= 64 && MatrixColumns >= 64;
        }

        private void MultiplyMatrices(object parameter)
        {
            try
            {
                if (_matrixA == null || _matrixB == null)
                {
                    StatusText = "Помилка: матриці не згенеровані";
                    return;
                }

                if (_matrixA.Columns != _matrixB.Rows)
                {
                    StatusText = "Помилка: кількість стовпців матриці A повинна дорівнювати кількості рядків матриці B";
                    return;
                }

                MultiplicationResult result = MatrixMultiplier.Multiply(_matrixA, _matrixB, SelectedMethod);

                _resultMatrix = result.ResultMatrix;
                _executionResults[SelectedMethod] = result;

                ResultMatrixText = _resultMatrix.ToShortString();

                UpdatePerformanceResults();

                StatusText = $"Множення виконано методом: {GetMethodName(SelectedMethod)}";
                ExecutionTimeText = $"Час виконання: {result.ExecutionTimeMs} мс";
            }
            catch (Exception ex)
            {
                StatusText = $"Помилка: {ex.Message}";
            }
        }

        private bool CanMultiplyMatrices(object parameter)
        {
            return _matrixA != null && _matrixB != null && _matrixA.Columns == _matrixB.Rows;
        }

        private void SaveResults(object parameter)
        {
            try
            {
                if (_matrixA == null || _matrixB == null || _executionResults.Count == 0)
                {
                    StatusText = "Помилка: немає даних для збереження";
                    return;
                }

                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Текстові файли (*.txt)|*.txt",
                    Title = "Зберегти результати",
                    FileName = $"MatrixMultiplication_{_matrixA.Rows}x{_matrixA.Columns}_{SelectedMethod}.txt"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName))
                    {
                        writer.WriteLine("=== Швидкісні методи множення матриць ===");
                        writer.WriteLine($"Дата: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                        writer.WriteLine();

                        writer.WriteLine($"Розмірність матриць: A({_matrixA.Rows}×{_matrixA.Columns}), B({_matrixB.Rows}×{_matrixB.Columns})");
                        writer.WriteLine($"Метод множення: {GetMethodName(SelectedMethod)}");
                        writer.WriteLine($"Діапазон значень: від {MinValue} до {MaxValue}");
                        writer.WriteLine();

                        writer.WriteLine("=== Матриця A (повна) ===");
                        writer.WriteLine(_matrixA.ToString());
                        writer.WriteLine();

                        writer.WriteLine("=== Матриця B (повна) ===");
                        writer.WriteLine(_matrixB.ToString());
                        writer.WriteLine();

                        if (_resultMatrix != null)
                        {
                            writer.WriteLine("=== Результат C = A × B (повна) ===");
                            writer.WriteLine(_resultMatrix.ToString());
                            writer.WriteLine();
                        }

                        writer.WriteLine("=== РЕЗУЛЬТАТИ ПРОДУКТИВНОСТІ ===");
                        writer.WriteLine();

                        // Записуємо результати всіх виконаних методів, сортуючи за кількістю операцій
                        var sortedResults = _executionResults.OrderBy(x => x.Value.MultiplicationCount + x.Value.AdditionCount);
                        foreach (var kvp in sortedResults)
                        {
                            var methodResult = kvp.Value;
                            long totalOperations = methodResult.MultiplicationCount + methodResult.AdditionCount;

                            writer.WriteLine($"{GetMethodName(kvp.Key)}:");
                            writer.WriteLine($"  Час виконання: {methodResult.ExecutionTimeMs} мс");
                            writer.WriteLine($"  Кількість множень: {methodResult.MultiplicationCount:N0}");
                            writer.WriteLine($"  Кількість додавань: {methodResult.AdditionCount:N0}");
                            writer.WriteLine($"  Загальна кількість операцій: {totalOperations:N0}");
                            writer.WriteLine($"  Розмір матриці: {methodResult.OriginalSize}×{methodResult.OriginalSize}");

                            if (methodResult.ActualSize != methodResult.OriginalSize)
                            {
                                writer.WriteLine($"  Фактичний розмір: {methodResult.ActualSize}×{methodResult.ActualSize} (розширено)");
                            }
                            writer.WriteLine();
                        }

                        // Порівняння методів, якщо є більше одного результату
                        if (_executionResults.Count > 1)
                        {
                            writer.WriteLine("=== ПОРІВНЯННЯ МЕТОДІВ ===");
                            writer.WriteLine();

                            var mostEfficient = sortedResults.First();
                            writer.WriteLine($"Найефективніший метод: {GetMethodName(mostEfficient.Key)} ({mostEfficient.Value.MultiplicationCount + mostEfficient.Value.AdditionCount:N0} операцій)");

                            foreach (var kvp in sortedResults.Skip(1))
                            {
                                long mostEfficientOps = mostEfficient.Value.MultiplicationCount + mostEfficient.Value.AdditionCount;
                                long currentOps = kvp.Value.MultiplicationCount + kvp.Value.AdditionCount;
                                double ratio = (double)currentOps / mostEfficientOps;
                                writer.WriteLine($"{GetMethodName(kvp.Key)} виконує у {ratio:F2} разів більше операцій");
                            }
                            writer.WriteLine();
                        }
                    }

                    StatusText = $"Результати збережено у файл: {saveFileDialog.FileName}";
                }
            }
            catch (Exception ex)
            {
                StatusText = $"Помилка збереження: {ex.Message}";
            }
        }

        private bool CanSaveResults(object parameter)
        {
            return _matrixA != null && _matrixB != null && _executionResults.Count > 0;
        }

        private void Clear(object parameter)
        {
            _matrixA = null;
            _matrixB = null;
            _resultMatrix = null;
            _executionResults.Clear();

            MatrixAText = string.Empty;
            MatrixBText = string.Empty;
            ResultMatrixText = string.Empty;
            PerformanceResultsText = string.Empty;

            StatusText = "Дані очищено";
            ExecutionTimeText = string.Empty;
        }

        private bool CanClear(object parameter)
        {
            return _matrixA != null || _matrixB != null || _resultMatrix != null || _executionResults.Count > 0;
        }

        #endregion

        #region Допоміжні методи

        private void UpdatePerformanceResults()
        {
            if (_executionResults.Count == 0)
            {
                PerformanceResultsText = string.Empty;
                return;
            }

            string result = "Практична складність:\n\n";

            if (_executionResults.ContainsKey(SelectedMethod))
            {
                var methodResult = _executionResults[SelectedMethod];
                long totalOperations = methodResult.MultiplicationCount + methodResult.AdditionCount;

                result += $"{GetMethodName(SelectedMethod)}:\n";
                result += $"  Час виконання: {methodResult.ExecutionTimeMs} мс\n";
                result += $"  Кількість множень: {methodResult.MultiplicationCount:N0}\n";
                result += $"  Кількість додавань: {methodResult.AdditionCount:N0}\n";
                result += $"  Загальна кількість операцій: {totalOperations:N0}\n";
                result += $"  Розмір матриці: {methodResult.OriginalSize}×{methodResult.OriginalSize}";

                if (methodResult.ActualSize != methodResult.OriginalSize)
                {
                    result += $" (розширено до {methodResult.ActualSize}×{methodResult.ActualSize})";
                }
                result += "\n\n";
            }

            // Додаємо порівняння методів, якщо є більше одного результату
            if (_executionResults.Count > 1)
            {
                result += "=== ПОРІВНЯННЯ МЕТОДІВ ===\n";

                // Сортуємо за кількістю операцій (найефективніший = найменше операцій)
                var sortedResults = _executionResults.OrderBy(x => x.Value.MultiplicationCount + x.Value.AdditionCount).ToList();
                var mostEfficient = sortedResults.First();

                result += $"Найефективніший: {GetMethodName(mostEfficient.Key)} ({mostEfficient.Value.MultiplicationCount + mostEfficient.Value.AdditionCount:N0} операцій)\n\n";

                result += "Відносна ефективність:\n";
                foreach (var kvp in sortedResults)
                {
                    long mostEfficientOps = mostEfficient.Value.MultiplicationCount + mostEfficient.Value.AdditionCount;
                    long currentOps = kvp.Value.MultiplicationCount + kvp.Value.AdditionCount;
                    double ratio = (double)currentOps / mostEfficientOps;
                    string status = kvp.Key == mostEfficient.Key ? " (найефективніший)" : $" ({ratio:F2}x більше операцій)";
                    result += $"  {GetMethodName(kvp.Key)}: {currentOps:N0} операцій{status}\n";
                }
                result += "\n";

                result += "Час виконання:\n";
                foreach (var kvp in sortedResults)
                {
                    result += $"  {GetMethodName(kvp.Key)}: {kvp.Value.ExecutionTimeMs} мс\n";
                }
            }

            PerformanceResultsText = result;
        }

        private string GetMethodName(MultiplicationMethod method)
        {
            return MethodNameConverter.Instance.Convert(method, typeof(string), null, null).ToString();
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}