using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Label = System.Windows.Forms.Label;
using MathFunction = System.Func<double, double>;

namespace _2lab2 {
  // Задание 2: поиск локального минимума методом дихотомии (половинного деления)
  public partial class task2 : Form {
    private TextBox textBoxLeftBound;
    private TextBox textBoxRightBound;
    private TextBox textBoxPrecision;
    private TextBox textBoxFormula;
    private Chart chartFunction;
    private Label labelResult;

    public task2() {
      InitializeComponent();
      BuildUserInterface();
    }

    // Обработчик, на который ссылается task2.Designer.cs
    private void task2_Load(object sender, EventArgs eventArgs) {

    }

    private void BuildUserInterface() {
      Text = "Задание 2 — Метод дихотомии";
      StartPosition = FormStartPosition.CenterScreen;
      ClientSize = new Size(900, 650);
      MinimumSize = new Size(700, 500);

      // Меню (кнопки действий — только в MenuStrip)
      MenuStrip mainMenu = new MenuStrip();
      ToolStripMenuItem menuItemCalculate = new ToolStripMenuItem("Рассчитать");
      ToolStripMenuItem menuItemClear = new ToolStripMenuItem("Очистить");
      ToolStripMenuItem menuItemExit = new ToolStripMenuItem("Выход");
      menuItemCalculate.Click += menuCalculate_Click;
      menuItemClear.Click += menuClear_Click;
      menuItemExit.Click += (sender, eventArgs) => Close();
      mainMenu.Items.AddRange(new ToolStripItem[] { menuItemCalculate, menuItemClear, menuItemExit });
      MainMenuStrip = mainMenu;

      // Панель ввода
      Panel inputPanel = new Panel { Dock = DockStyle.Top, Height = 100 };
      textBoxLeftBound = AddInputField(inputPanel, "a:", 15, 15, 90, "0");
      textBoxRightBound = AddInputField(inputPanel, "b:", 165, 15, 90, "5");
      textBoxPrecision = AddInputField(inputPanel, "e:", 315, 15, 90, "0,001");
      textBoxFormula = AddInputField(inputPanel, "f(x):", 15, 55, 520, "x^2 - 4*x + 3");

      Label labelHint = new Label {
        Text = "Допустимо: + - * / ^, скобки, x, pi, e,\nsin cos tan asin acos atan exp ln log sqrt abs",
        Location = new Point(560, 15),
        AutoSize = true,
        ForeColor = Color.DimGray
      };
      inputPanel.Controls.Add(labelHint);

      // Результат
      labelResult = new Label {
        Dock = DockStyle.Bottom,
        Height = 40,
        Font = new Font("Arial", 11f, FontStyle.Bold),
        ForeColor = Color.DarkGreen,
        TextAlign = ContentAlignment.MiddleLeft,
        Padding = new Padding(10, 0, 0, 0),
        Text = "Результат: —"
      };

      // График
      chartFunction = new Chart { Dock = DockStyle.Fill };
      ChartArea chartArea = new ChartArea("main");
      chartArea.AxisX.MajorGrid.LineColor = Color.Gainsboro;
      chartArea.AxisY.MajorGrid.LineColor = Color.Gainsboro;
      chartArea.AxisX.LabelStyle.Format = "0.##";
      chartArea.AxisY.LabelStyle.Format = "0.##";
      chartFunction.ChartAreas.Add(chartArea);
      chartFunction.Legends.Add(new Legend("legend") { Docking = Docking.Top });

      // Порядок добавления важен для Dock: сначала Fill, затем остальные
      Controls.Add(chartFunction);
      Controls.Add(labelResult);
      Controls.Add(inputPanel);
      Controls.Add(mainMenu);
    }

    private TextBox AddInputField(Panel parentPanel, string caption, int positionX, int positionY, int fieldWidth, string defaultText) {
      Label captionLabel = new Label { Text = caption, Location = new Point(positionX, positionY + 3), AutoSize = true };
      parentPanel.Controls.Add(captionLabel);

      TextBox textBox = new TextBox {
        Location = new Point(positionX + captionLabel.PreferredWidth + 8, positionY),
        Width = fieldWidth,
        Text = defaultText
      };
      parentPanel.Controls.Add(textBox);
      return textBox;
    }

    private void menuCalculate_Click(object sender, EventArgs eventArgs) {
      // 1. Валидация входных данных
      if (!TryParseDouble(textBoxLeftBound.Text, out double leftBound)) { ShowWarning("Параметр a должен быть вещественным числом.", textBoxLeftBound); return; }
      if (!TryParseDouble(textBoxRightBound.Text, out double rightBound)) { ShowWarning("Параметр b должен быть вещественным числом.", textBoxRightBound); return; }
      if (!TryParseDouble(textBoxPrecision.Text, out double precision)) { ShowWarning("Параметр e должен быть вещественным числом.", textBoxPrecision); return; }
      if (leftBound >= rightBound) { ShowWarning("Должно выполняться a < b.", textBoxLeftBound); return; }
      if (precision <= 0) { ShowWarning("Точность e должна быть больше нуля.", textBoxPrecision); return; }
      if (string.IsNullOrWhiteSpace(textBoxFormula.Text)) { ShowWarning("Введите формулу f(x).", textBoxFormula); return; }

      MathFunction function;
      try {
        function = FormulaParser.Compile(textBoxFormula.Text);
      }
      catch (FormatException formulaError) {
        ShowWarning("Ошибка в формуле: " + formulaError.Message, textBoxFormula);
        return;
      }

      // 2. Метод дихотомии
      double intervalStart = leftBound;
      double intervalEnd = rightBound;
      double probeOffset = precision / 4.0;   // расстояние от середины до пробных точек
      int iterationCount = 0;
      const int maxIterations = 200;

      try {
        while ((intervalEnd - intervalStart) / 2.0 > precision) {
          iterationCount++;
          if (iterationCount > maxIterations) {
            ShowWarning("Не удалось достичь заданной точности: значение e слишком мало для этого интервала.", textBoxPrecision);
            return;
          }

          double middle = (intervalStart + intervalEnd) / 2.0;
          double leftProbe = middle - probeOffset;
          double rightProbe = middle + probeOffset;
          double leftValue = EvaluateOrThrow(function, leftProbe);
          double rightValue = EvaluateOrThrow(function, rightProbe);

          if (leftValue < rightValue) intervalEnd = rightProbe;
          else intervalStart = leftProbe;
        }

        double minimumX = (intervalStart + intervalEnd) / 2.0;
        double minimumValue = EvaluateOrThrow(function, minimumX);

        // 3. Вывод результата и графика
        int decimalPlaces = Math.Min(12, Math.Max(2, (int)Math.Ceiling(-Math.Log10(precision)) + 1));
        string numberFormat = "F" + decimalPlaces;
        labelResult.Text = $"Минимум: x = {minimumX.ToString(numberFormat)},  f(x) = {minimumValue.ToString(numberFormat)}  (итераций: {iterationCount})";

        DrawChart(function, leftBound, rightBound, minimumX, minimumValue);
      }
      catch (InvalidOperationException domainError) {
        ShowWarning(domainError.Message, textBoxFormula);
      }
    }

    private void menuClear_Click(object sender, EventArgs eventArgs) {
      textBoxLeftBound.Clear();
      textBoxRightBound.Clear();
      textBoxPrecision.Clear();
      textBoxFormula.Clear();
      chartFunction.Series.Clear();
      labelResult.Text = "Результат: —";
      textBoxLeftBound.Focus();
    }

    private static double EvaluateOrThrow(MathFunction function, double xValue) {
      double result = function(xValue);
      if (double.IsNaN(result) || double.IsInfinity(result))
        throw new InvalidOperationException(
          $"Функция не определена в точке x = {xValue.ToString("G6", CultureInfo.CurrentCulture)}. Выберите другой интервал.");
      return result;
    }

    private void DrawChart(MathFunction function, double leftBound, double rightBound, double minimumX, double minimumValue) {
      chartFunction.Series.Clear();

      Series curveSeries = new Series("f(x)") {
        ChartType = SeriesChartType.Line,
        ChartArea = "main",
        Legend = "legend",
        Color = Color.SteelBlue,
        BorderWidth = 2
      };

      const int pointCount = 500;
      for (int pointIndex = 0; pointIndex <= pointCount; pointIndex++) {
        double xValue = leftBound + (rightBound - leftBound) * pointIndex / pointCount;
        double yValue = function(xValue);
        if (double.IsNaN(yValue) || double.IsInfinity(yValue)) continue; // точки разрыва пропускаем
        curveSeries.Points.AddXY(xValue, yValue);
      }

      Series minimumSeries = new Series("Минимум") {
        ChartType = SeriesChartType.Point,
        ChartArea = "main",
        Legend = "legend",
        Color = Color.Red,
        MarkerStyle = MarkerStyle.Circle,
        MarkerSize = 11
      };
      minimumSeries.Points.AddXY(minimumX, minimumValue);

      chartFunction.Series.Add(curveSeries);
      chartFunction.Series.Add(minimumSeries);

      chartFunction.ChartAreas["main"].AxisX.Minimum = leftBound;
      chartFunction.ChartAreas["main"].AxisX.Maximum = rightBound;
      chartFunction.ChartAreas["main"].RecalculateAxesScale();
    }

    private static bool TryParseDouble(string text, out double number) {
      number = 0;
      if (string.IsNullOrWhiteSpace(text)) return false;
      text = text.Trim().Replace(',', '.');
      if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out number)) return false;
      return !double.IsNaN(number) && !double.IsInfinity(number);
    }

    private void ShowWarning(string message, Control fieldToFocus) {
      MessageBox.Show(message, "Некорректные данные", MessageBoxButtons.OK, MessageBoxIcon.Warning);
      fieldToFocus?.Focus();
    }
  }

  // ---------- РАЗБОР ФОРМУЛЫ ----------
  // Превращает строку вида "x^2 - 4*sin(x)" в функцию double -> double
  internal static class FormulaParser {
    public static MathFunction Compile(string formulaText) {
      return new Parser(formulaText).ParseAll();
    }

    private class Parser {
      private readonly string formulaText;
      private int position;

      public Parser(string formulaText) {
        this.formulaText = formulaText;
      }

      public MathFunction ParseAll() {
        MathFunction expression = ParseExpression();
        SkipWhitespace();
        if (position < formulaText.Length)
          throw new FormatException($"неожиданный символ '{formulaText[position]}' в позиции {position + 1}.");
        return expression;
      }

      private void SkipWhitespace() {
        while (position < formulaText.Length && char.IsWhiteSpace(formulaText[position])) position++;
      }

      // Если дальше стоит ожидаемый символ — съедает его и возвращает true
      private bool TryConsume(char expectedChar) {
        SkipWhitespace();
        if (position < formulaText.Length && formulaText[position] == expectedChar) {
          position++;
          return true;
        }
        return false;
      }

      // выражение: слагаемые через + и -
      private MathFunction ParseExpression() {
        MathFunction result = ParseTerm();
        while (true) {
          if (TryConsume('+')) {
            MathFunction leftOperand = result;
            MathFunction rightOperand = ParseTerm();
            result = xValue => leftOperand(xValue) + rightOperand(xValue);
          }
          else if (TryConsume('-')) {
            MathFunction leftOperand = result;
            MathFunction rightOperand = ParseTerm();
            result = xValue => leftOperand(xValue) - rightOperand(xValue);
          }
          else return result;
        }
      }

      // слагаемое: множители через * и /
      private MathFunction ParseTerm() {
        MathFunction result = ParseUnary();
        while (true) {
          if (TryConsume('*')) {
            MathFunction leftOperand = result;
            MathFunction rightOperand = ParseUnary();
            result = xValue => leftOperand(xValue) * rightOperand(xValue);
          }
          else if (TryConsume('/')) {
            MathFunction leftOperand = result;
            MathFunction rightOperand = ParseUnary();
            result = xValue => leftOperand(xValue) / rightOperand(xValue);
          }
          else return result;
        }
      }

      // унарный минус / плюс
      private MathFunction ParseUnary() {
        if (TryConsume('-')) {
          MathFunction operand = ParseUnary();
          return xValue => -operand(xValue);
        }
        if (TryConsume('+')) return ParseUnary();
        return ParsePower();
      }

      // степень: основание ^ показатель (правоассоциативная)
      private MathFunction ParsePower() {
        MathFunction baseValue = ParsePrimary();
        if (TryConsume('^')) {
          MathFunction exponent = ParseUnary();
          return xValue => Math.Pow(baseValue(xValue), exponent(xValue));
        }
        return baseValue;
      }

      // число, переменная x, константа, скобки или вызов функции
      private MathFunction ParsePrimary() {
        SkipWhitespace();
        if (position >= formulaText.Length)
          throw new FormatException("формула оборвана, ожидалось выражение.");

        char currentChar = formulaText[position];

        if (currentChar == '(') {
          position++;
          MathFunction insideBrackets = ParseExpression();
          if (!TryConsume(')')) throw new FormatException("не хватает закрывающей скобки ')'.");
          return insideBrackets;
        }

        if (char.IsDigit(currentChar) || currentChar == '.' || currentChar == ',')
          return ParseNumber();

        if (char.IsLetter(currentChar)) {
          int nameStart = position;
          while (position < formulaText.Length && char.IsLetter(formulaText[position])) position++;
          string name = formulaText.Substring(nameStart, position - nameStart).ToLowerInvariant();

          switch (name) {
            case "x": return xValue => xValue;
            case "pi": return xValue => Math.PI;
            case "e": return xValue => Math.E;
          }

          if (!TryConsume('(')) throw new FormatException($"неизвестное имя '{name}'.");
          MathFunction argument = ParseExpression();
          if (!TryConsume(')')) throw new FormatException($"не хватает ')' после аргумента функции {name}.");

          switch (name) {
            case "sin": return xValue => Math.Sin(argument(xValue));
            case "cos": return xValue => Math.Cos(argument(xValue));
            case "tan": return xValue => Math.Tan(argument(xValue));
            case "asin": return xValue => Math.Asin(argument(xValue));
            case "acos": return xValue => Math.Acos(argument(xValue));
            case "atan": return xValue => Math.Atan(argument(xValue));
            case "exp": return xValue => Math.Exp(argument(xValue));
            case "ln": return xValue => Math.Log(argument(xValue));
            case "log": return xValue => Math.Log10(argument(xValue));
            case "sqrt": return xValue => Math.Sqrt(argument(xValue));
            case "abs": return xValue => Math.Abs(argument(xValue));
            default: throw new FormatException($"неизвестная функция '{name}'.");
          }
        }

        throw new FormatException($"неожиданный символ '{currentChar}' в позиции {position + 1}.");
      }

      private MathFunction ParseNumber() {
        int numberStart = position;
        while (position < formulaText.Length &&
               (char.IsDigit(formulaText[position]) || formulaText[position] == '.' || formulaText[position] == ','))
          position++;

        // экспоненциальная запись: 1e-3
        if (position < formulaText.Length && (formulaText[position] == 'e' || formulaText[position] == 'E')) {
          int exponentEnd = position + 1;
          if (exponentEnd < formulaText.Length && (formulaText[exponentEnd] == '+' || formulaText[exponentEnd] == '-'))
            exponentEnd++;
          if (exponentEnd < formulaText.Length && char.IsDigit(formulaText[exponentEnd])) {
            while (exponentEnd < formulaText.Length && char.IsDigit(formulaText[exponentEnd])) exponentEnd++;
            position = exponentEnd;
          }
        }

        string numberText = formulaText.Substring(numberStart, position - numberStart).Replace(',', '.');
        if (!double.TryParse(numberText, NumberStyles.Float, CultureInfo.InvariantCulture, out double numberValue))
          throw new FormatException($"некорректное число '{numberText}'.");
        return xValue => numberValue;
      }
    }
  }
}