using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace MatrixMultiplicationApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Обробник для валідації вводу числових значень з обмеженням до 4 символів
        /// </summary>
        private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as System.Windows.Controls.TextBox;

            if (textBox == null)
                return;

            // Отримуємо поточний текст разом з новим символом
            string currentText = textBox.Text;
            string newText = currentText.Insert(textBox.SelectionStart, e.Text);

            // Перевіряємо довжину - максимум 4 символи
            if (newText.Length > 4)
            {
                e.Handled = true;
                return;
            }

            // Регулярний вираз для валідації числових значень
            // Дозволяє: цілі числа, від'ємні числа, десяткові числа
            // Формати: 123, -123, 12.3, -12.3, .5, -.5
            Regex regex = new Regex(@"^-?(\d+\.?\d*|\.\d+)$|^-?$|^-?\.$");

            // Перевіряємо, чи відповідає новий текст дозволеному формату
            if (!regex.IsMatch(newText))
            {
                e.Handled = true;
                return;
            }

            // Додаткові перевірки для коректності десяткових чисел
            if (newText.Contains("."))
            {
                // Не дозволяємо більше однієї крапки
                int dotCount = 0;
                foreach (char c in newText)
                {
                    if (c == '.') dotCount++;
                }

                if (dotCount > 1)
                {
                    e.Handled = true;
                    return;
                }
            }

            // Перевіряємо, чи не більше одного знаку мінус на початку
            if (newText.Contains("-"))
            {
                if (newText.IndexOf('-') != 0 || newText.LastIndexOf('-') != 0)
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        /// <summary>
        /// Обробник для валідації вводу розмірності матриці
        /// Дозволяє тільки цілі числа від 64
        /// </summary>
        private void MatrixSizeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as System.Windows.Controls.TextBox;

            if (textBox == null)
                return;

            // Отримуємо поточний текст разом з новим символом
            string currentText = textBox.Text;
            string newText = currentText.Insert(textBox.SelectionStart, e.Text);

            // Дозволяємо тільки цифри
            if (!char.IsDigit(e.Text[0]))
            {
                e.Handled = true;
                return;
            }

            // Перевіряємо, чи можна перетворити в число
            if (int.TryParse(newText, out int value))
            {
                // Перевіряємо максимальне значення (розумне обмеження)
                if (value > 9999)
                {
                    e.Handled = true;
                    return;
                }
            }
        }

        /// <summary>
        /// Обробник втрати фокуса для поля розмірності матриці
        /// Перевіряє мінімальне значення 64
        /// </summary>
        private void MatrixSizeTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as System.Windows.Controls.TextBox;

            if (textBox == null)
                return;

            if (int.TryParse(textBox.Text, out int value))
            {
                if (value < 64)
                {
                    textBox.Text = "64";
                }
            }
            else
            {
                // Якщо не можна перетворити в число, встановлюємо мінімальне значення
                textBox.Text = "64";
            }
        }
    }
}