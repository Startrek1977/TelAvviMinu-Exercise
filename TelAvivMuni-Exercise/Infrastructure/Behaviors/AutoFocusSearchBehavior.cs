using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TelAvivMuni_Exercise.Infrastructure.Behaviors
{
    /// <summary>
    /// Attached behavior that automatically focuses a target TextBox when the user starts typing
    /// text input keys (letters, numbers, space) in the attached element.
    /// Also handles Escape key to invoke a cancel command when the target is not focused.
    /// </summary>
    public static class AutoFocusSearchBehavior
    {
        /// <summary>
        /// The TextBox that should receive focus when typing starts.
        /// </summary>
        public static readonly DependencyProperty TargetTextBoxProperty =
            DependencyProperty.RegisterAttached(
                "TargetTextBox",
                typeof(TextBox),
                typeof(AutoFocusSearchBehavior),
                new PropertyMetadata(null, OnTargetTextBoxChanged));

        /// <summary>
        /// Command to execute when Escape is pressed and the target TextBox is not focused.
        /// </summary>
        public static readonly DependencyProperty EscapeCommandProperty =
            DependencyProperty.RegisterAttached(
                "EscapeCommand",
                typeof(ICommand),
                typeof(AutoFocusSearchBehavior),
                new PropertyMetadata(null));

        public static TextBox GetTargetTextBox(DependencyObject obj) =>
            (TextBox)obj.GetValue(TargetTextBoxProperty);

        public static void SetTargetTextBox(DependencyObject obj, TextBox value) =>
            obj.SetValue(TargetTextBoxProperty, value);

        public static ICommand GetEscapeCommand(DependencyObject obj) =>
            (ICommand)obj.GetValue(EscapeCommandProperty);

        public static void SetEscapeCommand(DependencyObject obj, ICommand value) =>
            obj.SetValue(EscapeCommandProperty, value);

        private static void OnTargetTextBoxChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                element.PreviewKeyDown -= OnPreviewKeyDown;

                if (e.NewValue != null)
                {
                    element.PreviewKeyDown += OnPreviewKeyDown;
                }
            }
        }

        private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is not DependencyObject d)
                return;

            var targetTextBox = GetTargetTextBox(d);
            if (targetTextBox == null)
                return;

            // Handle Escape key to invoke cancel command (if target is not focused)
            if (e.Key == Key.Escape && !targetTextBox.IsFocused)
            {
                var escapeCommand = GetEscapeCommand(d);
                if (escapeCommand?.CanExecute(null) == true)
                {
                    escapeCommand.Execute(null);
                    e.Handled = true;
                }
                return;
            }

            // Only handle text input keys (letters, numbers, space)
            if ((e.Key >= Key.A && e.Key <= Key.Z) ||
                (e.Key >= Key.D0 && e.Key <= Key.D9) ||
                (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) ||
                e.Key == Key.Space)
            {
                // Only focus if not already focused
                if (!targetTextBox.IsFocused)
                {
                    targetTextBox.Focus();
                    // Let the event continue so the character is typed in the search box
                }
            }
        }
    }
}
