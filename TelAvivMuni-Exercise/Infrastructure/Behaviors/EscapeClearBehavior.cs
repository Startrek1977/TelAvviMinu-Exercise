using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TelAvivMuni_Exercise.Infrastructure.Behaviors
{
    /// <summary>
    /// Attached behavior that clears a TextBox's text when the Escape key is pressed.
    /// </summary>
    public static class EscapeClearBehavior
    {
        /// <summary>
        /// When set to true, enables the Escape-to-clear behavior on the attached TextBox.
        /// </summary>
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(EscapeClearBehavior),
                new PropertyMetadata(false, OnIsEnabledChanged));

        public static bool GetIsEnabled(DependencyObject obj) =>
            (bool)obj.GetValue(IsEnabledProperty);

        public static void SetIsEnabled(DependencyObject obj, bool value) =>
            obj.SetValue(IsEnabledProperty, value);

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                textBox.KeyDown -= OnKeyDown;

                if (e.NewValue is true)
                {
                    textBox.KeyDown += OnKeyDown;
                }
            }
        }

        private static void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && sender is TextBox textBox)
            {
                textBox.Text = string.Empty;
                e.Handled = true;
            }
        }
    }
}
