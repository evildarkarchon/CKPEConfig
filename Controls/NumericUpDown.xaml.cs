using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CKPEConfig.Controls
{
    public partial class NumericUpDownControl : UserControl
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                nameof(Value),
                typeof(int),
                typeof(NumericUpDownControl),
                new FrameworkPropertyMetadata(0,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    OnValueChanged));

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register(
                nameof(Minimum),
                typeof(int),
                typeof(NumericUpDownControl),
                new PropertyMetadata(0));

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register(
                nameof(Maximum),
                typeof(int),
                typeof(NumericUpDownControl),
                new PropertyMetadata(999999));

        public static readonly DependencyProperty StepProperty =
            DependencyProperty.Register(
                nameof(Step),
                typeof(int),
                typeof(NumericUpDownControl),
                new PropertyMetadata(1));

        public int Value
        {
            get => (int)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public int Minimum
        {
            get => (int)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        public int Maximum
        {
            get => (int)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        public int Step
        {
            get => (int)GetValue(StepProperty);
            set => SetValue(StepProperty, value);
        }

        private static readonly Regex _numericRegex = new(@"^-?\d*$");

        public NumericUpDownControl()
        {
            InitializeComponent();
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is NumericUpDownControl control)
            {
                var newValue = (int)e.NewValue;

                if (newValue < control.Minimum)
                    control.Value = control.Minimum;
                else if (newValue > control.Maximum)
                    control.Value = control.Maximum;
            }
        }

        private void UpButton_Click(object sender, RoutedEventArgs e)
        {
            Value = Math.Min(Maximum, Value + Step);
        }

        private void DownButton_Click(object sender, RoutedEventArgs e)
        {
            Value = Math.Max(Minimum, Value - Step);
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                var text = textBox.Text.Insert(textBox.SelectionStart, e.Text);
                e.Handled = !_numericRegex.IsMatch(text);
            }
        }
    }
}