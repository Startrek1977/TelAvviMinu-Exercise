using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TelAvivMuni_Exercise.Infrastructure.Behaviors
{
    /// <summary>
    /// Attached behavior that executes a command when Enter/Return key is pressed in a DataGrid.
    /// This prevents the default DataGrid row navigation behavior and allows custom actions.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class DataGridEnterBehavior
    {
        /// <summary>
        /// The command to execute when Enter/Return is pressed in the DataGrid.
        /// </summary>
        public static readonly DependencyProperty EnterCommandProperty =
            DependencyProperty.RegisterAttached(
                "EnterCommand",
                typeof(ICommand),
                typeof(DataGridEnterBehavior),
                new PropertyMetadata(null, OnEnterCommandChanged));

        public static ICommand GetEnterCommand(DependencyObject obj) =>
            (ICommand)obj.GetValue(EnterCommandProperty);

        public static void SetEnterCommand(DependencyObject obj, ICommand value) =>
            obj.SetValue(EnterCommandProperty, value);

        private static void OnEnterCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid dataGrid)
            {
                dataGrid.PreviewKeyDown -= OnPreviewKeyDown;

                if (e.NewValue != null)
                {
                    dataGrid.PreviewKeyDown += OnPreviewKeyDown;
                }
            }
        }

        private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Return || e.Key == Key.Enter) && sender is DataGrid dataGrid)
            {
                var command = GetEnterCommand(dataGrid);
                if (command?.CanExecute(null) == true)
                {
                    command.Execute(null);
                    e.Handled = true;
                }
            }
        }
    }
}
