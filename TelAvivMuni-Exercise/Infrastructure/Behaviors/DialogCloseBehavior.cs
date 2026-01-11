using System.ComponentModel;
using System.Windows;

namespace TelAvivMuni_Exercise.Infrastructure.Behaviors
{
    /// <summary>
    /// Attached behavior that closes a Window when a bound DialogResult property changes.
    /// This enables MVVM-friendly dialog closing by monitoring a ViewModel's DialogResult property.
    /// </summary>
    public static class DialogCloseBehavior
    {
        /// <summary>
        /// Attached property for the bound DialogResult value from the ViewModel.
        /// When this value changes to non-null, the Window's DialogResult is set accordingly.
        /// </summary>
        public static readonly DependencyProperty DialogResultProperty =
            DependencyProperty.RegisterAttached(
                "DialogResult",
                typeof(bool?),
                typeof(DialogCloseBehavior),
                new PropertyMetadata(null, OnDialogResultChanged));

        public static bool? GetDialogResult(DependencyObject obj) =>
            (bool?)obj.GetValue(DialogResultProperty);

        public static void SetDialogResult(DependencyObject obj, bool? value) =>
            obj.SetValue(DialogResultProperty, value);

        private static void OnDialogResultChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Window window && e.NewValue is bool dialogResult)
            {
                window.DialogResult = dialogResult;
            }
        }
    }
}
