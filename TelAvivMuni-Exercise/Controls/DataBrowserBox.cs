using System.Collections;
using System.Windows;
using System.Windows.Controls;

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

        private Button? _browseButton;

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

            if (_browseButton != null)
            {
                _browseButton.Click += OnBrowseButtonClick;
            }
        }

        private void OnBrowseButtonClick(object sender, RoutedEventArgs e)
        {
            var dialog = new DataBrowserDialog
            {
                Owner = Window.GetWindow(this),
                Title = DialogTitle ?? "Select Product"
            };

            dialog.SetItemsSource(ItemsSource);

            if (dialog.ShowDialog() == true && dialog.SelectedItem != null)
            {
                SelectedItem = dialog.SelectedItem;
            }
        }
    }
}
