using System.Collections;
using System.Collections.ObjectModel;
using TelAvivMuni_Exercise.Models;

namespace TelAvivMuni_Exercise.Services
{
    public interface IDialogService
    {
        object? ShowBrowseDialog(IEnumerable items, string title, object? currentSelection, ObservableCollection<BrowserColumn>? columns = null);
    }
}
