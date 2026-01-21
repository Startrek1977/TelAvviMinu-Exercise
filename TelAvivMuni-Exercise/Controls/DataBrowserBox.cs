using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using TelAvivMuni_Exercise.Models;
using TelAvivMuni_Exercise.Services;

namespace TelAvivMuni_Exercise.Controls
{
    /// <summary>
    /// A custom WPF control that combines a read-only textbox with browse and clear buttons.
    /// Displays the selected item and allows browsing through a collection via a dialog.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class DataBrowserBox : Control
    {
        /// <summary>
        /// Dependency property for the collection of items to browse.
        /// </summary>
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(DataBrowserBox),
                new PropertyMetadata(null));

        /// <summary>
        /// Dependency property for the currently selected item.
        /// Supports two-way binding and triggers SelectionChanged event when modified.
        /// </summary>
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(DataBrowserBox),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));

        /// <summary>
        /// Dependency property for specifying which property of the selected item to display in the textbox.
        /// If not specified, ToString() is used.
        /// </summary>
        public static readonly DependencyProperty DisplayMemberPathProperty =
            DependencyProperty.Register(nameof(DisplayMemberPath), typeof(string), typeof(DataBrowserBox),
                new PropertyMetadata(null));

        /// <summary>
        /// Dependency property for the title of the browse dialog window.
        /// </summary>
        public static readonly DependencyProperty DialogTitleProperty =
            DependencyProperty.Register(nameof(DialogTitle), typeof(string), typeof(DataBrowserBox),
                new PropertyMetadata(null));

        /// <summary>
        /// Dependency property for the dialog service used to show the browse dialog.
        /// </summary>
        public static readonly DependencyProperty DialogServiceProperty =
            DependencyProperty.Register(nameof(DialogService), typeof(IDialogService), typeof(DataBrowserBox),
                new PropertyMetadata(null));

        /// <summary>
        /// Dependency property for custom column configurations in the browse dialog's DataGrid.
        /// </summary>
        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.Register(nameof(Columns), typeof(ObservableCollection<BrowserColumn>), typeof(DataBrowserBox),
                new PropertyMetadata(null));

        /// <summary>
        /// Routed event fired when the SelectedItem changes.
        /// </summary>
        public static readonly RoutedEvent SelectionChangedEvent =
            EventManager.RegisterRoutedEvent(nameof(SelectionChanged), RoutingStrategy.Bubble,
                typeof(SelectionChangedEventHandler), typeof(DataBrowserBox));

        /// <summary>
        /// Event raised when the selected item changes.
        /// </summary>
        public event SelectionChangedEventHandler SelectionChanged
        {
            add => AddHandler(SelectionChangedEvent, value);
            remove => RemoveHandler(SelectionChangedEvent, value);
        }

        /// <summary>
        /// Property change callback for SelectedItem.
        /// Updates the display text and raises the SelectionChanged event.
        /// </summary>
        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (DataBrowserBox)d;
            // Update the textbox display to reflect the new selection
            control.UpdateDisplayText();

            // Raise the SelectionChanged event for consumers of this control
            var removedItems = e.OldValue != null ? new[] { e.OldValue } : Array.Empty<object>();
            var addedItems = e.NewValue != null ? new[] { e.NewValue } : Array.Empty<object>();
            control.RaiseEvent(new SelectionChangedEventArgs(SelectionChangedEvent, removedItems, addedItems));
        }

        /// <summary>
        /// Gets or sets the collection of items available for browsing.
        /// </summary>
        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        /// <summary>
        /// Gets or sets the currently selected item from the collection.
        /// </summary>
        public object? SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        /// <summary>
        /// Gets or sets the property path to use for displaying the selected item.
        /// </summary>
        public string DisplayMemberPath
        {
            get => (string)GetValue(DisplayMemberPathProperty);
            set => SetValue(DisplayMemberPathProperty, value);
        }

        /// <summary>
        /// Gets or sets the title for the browse dialog.
        /// </summary>
        public string DialogTitle
        {
            get => (string)GetValue(DialogTitleProperty);
            set => SetValue(DialogTitleProperty, value);
        }

        /// <summary>
        /// Gets or sets the dialog service for showing the browse dialog.
        /// </summary>
        public IDialogService DialogService
        {
            get => (IDialogService)GetValue(DialogServiceProperty);
            set => SetValue(DialogServiceProperty, value);
        }

        /// <summary>
        /// Gets or sets the custom column configurations for the browse dialog's DataGrid.
        /// </summary>
        public ObservableCollection<BrowserColumn> Columns
        {
            get => (ObservableCollection<BrowserColumn>)GetValue(ColumnsProperty);
            set => SetValue(ColumnsProperty, value);
        }

        // Template part references
        private Button? _browseButton;
        private TextBox? _textBox;
        private Button? _clearButton;

        /// <summary>
        /// Initializes a new instance of the DataBrowserBox control.
        /// </summary>
        public DataBrowserBox()
        {
            Columns = new ObservableCollection<BrowserColumn>();
        }

        /// <summary>
        /// Static constructor to override default style key.
        /// This ensures the control uses the style defined in Generic.xaml.
        /// </summary>
        static DataBrowserBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DataBrowserBox),
                new FrameworkPropertyMetadata(typeof(DataBrowserBox)));
        }

        /// <summary>
        /// Called when the control template is applied.
        /// Retrieves template parts and subscribes to their events.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // Unsubscribe from previous template parts to prevent memory leaks
            if (_browseButton != null)
            {
                _browseButton.Click -= OnBrowseButtonClick;
            }

            if (_clearButton != null)
            {
                _clearButton.Click -= OnClearButtonClick;
            }

            // Retrieve new template parts from the control template
            _browseButton = GetTemplateChild("PART_BrowseButton") as Button;
            _textBox = GetTemplateChild("PART_TextBox") as TextBox;
            _clearButton = GetTemplateChild("PART_ClearButton") as Button;

            // Subscribe to template part events
            if (_browseButton != null)
            {
                _browseButton.Click += OnBrowseButtonClick;
            }

            if (_clearButton != null)
            {
                _clearButton.Click += OnClearButtonClick;
            }

            // Update the display to reflect the current SelectedItem
            UpdateDisplayText();
        }

        /// <summary>
        /// Handles the Browse button click event.
        /// Opens a dialog to allow the user to select an item from the collection.
        /// </summary>
        private void OnBrowseButtonClick(object sender, RoutedEventArgs e)
        {
            // Validate that a dialog service is configured
            if (DialogService == null)
                return;

            // Validate that items are available to browse
            if (ItemsSource == null)
            {
                MessageBox.Show("No items available to browse.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Show the browse dialog with the current selection
            var title = DialogTitle ?? "Select Item";
            var result = DialogService.ShowBrowseDialog(ItemsSource, title, SelectedItem, Columns);

            // Update SelectedItem if user confirmed the dialog
            if (result != null)
            {
                SelectedItem = result;
            }
        }

        /// <summary>
        /// Handles the Clear button click event.
        /// Clears the currently selected item.
        /// </summary>
        private void OnClearButtonClick(object sender, RoutedEventArgs e)
        {
            SelectedItem = null;
        }

        /// <summary>
        /// Updates the textbox display text based on the current SelectedItem.
        /// Shows placeholder text when no item is selected, or the item's display value otherwise.
        /// </summary>
        private void UpdateDisplayText()
        {
            if (_textBox == null) return;

            if (SelectedItem == null)
            {
                // Show placeholder text with reduced opacity when no item is selected
                _textBox.Text = "Click to select...";
                _textBox.Opacity = 0.5;
            }
            else
            {
                // Show the selected item's display value with full opacity
                _textBox.Text = GetDisplayValue(SelectedItem);
                _textBox.Opacity = 1.0;
            }
        }

        /// <summary>
        /// Retrieves the display value for the given item.
        /// Uses DisplayMemberPath if specified, otherwise uses ToString().
        /// </summary>
        /// <param name="item">The item to get the display value for</param>
        /// <returns>The display string for the item</returns>
        private string GetDisplayValue(object item)
        {
            // If no DisplayMemberPath is specified, use ToString()
            if (string.IsNullOrEmpty(DisplayMemberPath))
            {
                return item.ToString() ?? string.Empty;
            }

            // Use reflection to get the specified property value
            var property = item.GetType().GetProperty(DisplayMemberPath);
            if (property != null)
            {
                var value = property.GetValue(item);
                return value?.ToString() ?? string.Empty;
            }

            // Fallback to ToString() if the property is not found
            return item.ToString() ?? string.Empty;
        }
    }
}
