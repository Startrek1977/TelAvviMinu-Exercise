using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using TelAvivMuni_Exercise.Models;
using TelAvivMuni_Exercise.Services;

namespace TelAvivMuni_Exercise.Controls
{
    public class DataBrowserBox : Control
    {
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(DataBrowserBox),
                new PropertyMetadata(null));

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(DataBrowserBox),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));

        public static readonly DependencyProperty DisplayMemberPathProperty =
            DependencyProperty.Register(nameof(DisplayMemberPath), typeof(string), typeof(DataBrowserBox),
                new PropertyMetadata(null));

        public static readonly DependencyProperty DialogTitleProperty =
            DependencyProperty.Register(nameof(DialogTitle), typeof(string), typeof(DataBrowserBox),
                new PropertyMetadata(null));

        public static readonly DependencyProperty DialogServiceProperty =
            DependencyProperty.Register(nameof(DialogService), typeof(IDialogService), typeof(DataBrowserBox),
                new PropertyMetadata(null));

        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.Register(nameof(Columns), typeof(ObservableCollection<BrowserColumn>), typeof(DataBrowserBox),
                new PropertyMetadata(null));

        public static readonly RoutedEvent SelectionChangedEvent =
            EventManager.RegisterRoutedEvent(nameof(SelectionChanged), RoutingStrategy.Bubble,
                typeof(SelectionChangedEventHandler), typeof(DataBrowserBox));

        public event SelectionChangedEventHandler SelectionChanged
        {
            add => AddHandler(SelectionChangedEvent, value);
            remove => RemoveHandler(SelectionChangedEvent, value);
        }

        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (DataBrowserBox)d;
            control.UpdateDisplayText();
            var removedItems = e.OldValue != null ? new[] { e.OldValue } : Array.Empty<object>();
            var addedItems = e.NewValue != null ? new[] { e.NewValue } : Array.Empty<object>();
            control.RaiseEvent(new SelectionChangedEventArgs(SelectionChangedEvent, removedItems, addedItems));
        }

        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public string DisplayMemberPath
        {
            get => (string)GetValue(DisplayMemberPathProperty);
            set => SetValue(DisplayMemberPathProperty, value);
        }

        public string DialogTitle
        {
            get => (string)GetValue(DialogTitleProperty);
            set => SetValue(DialogTitleProperty, value);
        }

        public IDialogService DialogService
        {
            get => (IDialogService)GetValue(DialogServiceProperty);
            set => SetValue(DialogServiceProperty, value);
        }

        public ObservableCollection<BrowserColumn> Columns
        {
            get => (ObservableCollection<BrowserColumn>)GetValue(ColumnsProperty);
            set => SetValue(ColumnsProperty, value);
        }

        private Button? _browseButton;
        private TextBox? _textBox;

        public DataBrowserBox()
        {
            Columns = new ObservableCollection<BrowserColumn>();
        }

        static DataBrowserBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DataBrowserBox),
                new FrameworkPropertyMetadata(typeof(DataBrowserBox)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_browseButton != null)
            {
                _browseButton.Click -= OnBrowseButtonClick;
            }

            _browseButton = GetTemplateChild("PART_BrowseButton") as Button;
            _textBox = GetTemplateChild("PART_TextBox") as TextBox;

            if (_browseButton != null)
            {
                _browseButton.Click += OnBrowseButtonClick;
            }

            UpdateDisplayText();
        }

        private void OnBrowseButtonClick(object sender, RoutedEventArgs e)
        {
            if (DialogService == null)
                return;

            if (ItemsSource == null)
            {
                MessageBox.Show("No items available to browse.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var title = DialogTitle ?? "Select Item";
            var result = DialogService.ShowBrowseDialog(ItemsSource, title, SelectedItem, Columns);

            if (result != null)
            {
                SelectedItem = result;
            }
        }

        private void UpdateDisplayText()
        {
            if (_textBox == null) return;

            if (SelectedItem == null)
            {
                _textBox.Text = "Click to select...";
                _textBox.Opacity = 0.5;
            }
            else
            {
                var displayValue = GetDisplayValue(SelectedItem);
                _textBox.Text = displayValue;
                _textBox.Opacity = 1.0;
            }
        }

        private string GetDisplayValue(object item)
        {
            if (string.IsNullOrEmpty(DisplayMemberPath))
            {
                return item.ToString() ?? string.Empty;
            }

            var property = item.GetType().GetProperty(DisplayMemberPath);
            if (property != null)
            {
                var value = property.GetValue(item);
                return value?.ToString() ?? string.Empty;
            }

            return item.ToString() ?? string.Empty;
        }
    }
}
