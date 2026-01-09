using System.Windows;
using System.Windows.Controls;

namespace TelAvivMuni_Exercise.Controls
{
    public class DataBrowserBox : Control
    {
        static DataBrowserBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DataBrowserBox),
                new FrameworkPropertyMetadata(typeof(DataBrowserBox)));
        }
    }
}
