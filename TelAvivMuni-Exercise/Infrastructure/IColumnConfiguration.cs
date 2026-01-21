using System.Collections.ObjectModel;
using TelAvivMuni_Exercise.Models;

namespace TelAvivMuni_Exercise.Infrastructure
{
    /// <summary>
    /// Interface for ViewModels that provide custom column configuration for DataGrids.
    /// </summary>
    public interface IColumnConfiguration
    {
        /// <summary>
        /// Gets the collection of custom column definitions.
        /// </summary>
        ObservableCollection<BrowserColumn>? Columns { get; }

        /// <summary>
        /// Gets whether custom columns are defined.
        /// </summary>
        bool HasCustomColumns { get; }
    }
}
