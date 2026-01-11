# TelAviv Municipality Exercise

A WPF application demonstrating a reusable data browser control for selecting items from collections with search and filtering capabilities.

## Project Overview

This project is a home exercise created as part of an interview for the Software Developer position at Tel-Aviv Municipality. It showcases a custom WPF control (`DataBrowserBox`) that provides a modern, user-friendly interface for browsing and selecting items from data collections.

## Features

### Custom DataBrowserBox Control
- **Reusable WPF Custom Control** - Can be used anywhere in the application with any data type
- **Display Member Path** - Configure which property to display in the control
- **Two-Way Data Binding** - Full support for MVVM pattern with `SelectedItem` binding
- **Custom Column Configuration** - Define columns with custom headers, widths, formats, and alignment
- **Auto-Generated Columns** - Automatically generates columns from data type (default behavior)
- **Visual Feedback** - Watermark text in italic, selected items in bold
- **Modern UI** - Styled browse button with hover effects
- **Selection Persistence** - Currently selected item is highlighted when dialog reopens

### Browse Dialog
- **Modern Design** - Clean, professional interface with Material Design-inspired colors
- **MVVM Architecture** - Pure MVVM implementation using WPF Attached Behaviors
- **Vector Search Icon** - Clean magnifying glass icon using SVG path for crisp rendering at any size
- **Search & Filter** - Real-time search across all item properties
- **Clear Filter Button** - Appears when search text is entered, clears with one click
- **Flexible Column Display** - Auto-generated or custom column definitions
- **Column Formatting** - Support for currency, date, and custom string formats
- **Column Alignment** - Configure horizontal alignment (Left, Right, Center)
- **Selection Highlighting** - Bold text and blue background for selected items
- **Selection Persistence** - Previously selected item is automatically highlighted and scrolled into view
- **Row Hover Effects** - Visual feedback on mouse over
- **Item Counter** - Displays total filtered items count
- **Responsive Design** - Resizable dialog with proper scrolling
- **Keyboard Navigation** - Type to search, Enter to select, Escape to clear/cancel

### Error Handling
- **File Not Found** - Gracefully handles missing Products.json file
- **Empty File** - Handles empty or whitespace-only JSON files
- **Invalid JSON** - Catches and handles JSON deserialization errors
- **IO Errors** - Handles file access and permission issues
- **Null Collections** - Validates data before opening dialogs with user-friendly messages

## Technology Stack

- **Framework:** .NET 8.0 (WPF)
- **Language:** C# 12
- **UI Pattern:** MVVM (Model-View-ViewModel)
- **Libraries:**
  - CommunityToolkit.Mvvm - For MVVM infrastructure
  - System.Text.Json - For JSON serialization

## Project Structure

```
TelAvivMuni-Exercise/
├── Controls/
│   ├── DataBrowserBox.cs          # Custom control implementation
│   └── DataBrowserDialog.xaml     # Browse dialog UI
├── ViewModels/
│   ├── MainWindowViewModel.cs     # Main window view model
│   └── DataBrowserDialogViewModel.cs # Dialog view model
├── Services/
│   ├── IDialogService.cs          # Dialog service interface
│   └── DialogService.cs           # Dialog service implementation
├── Infrastructure/
│   └── Behaviors/                 # WPF Attached Behaviors (MVVM pattern)
│       ├── AutoFocusSearchBehavior.cs    # Auto-focus on typing
│       ├── DataGridEnterBehavior.cs      # Handle Enter key in DataGrid
│       ├── DataGridScrollIntoViewBehavior.cs # Scroll to selected item
│       ├── DialogCloseBehavior.cs        # MVVM-friendly dialog closing
│       └── EscapeClearBehavior.cs        # Clear text on Escape key
├── Models/
│   ├── Product.cs                 # Product data model
│   └── BrowserColumn.cs           # Column configuration model
├── Themes/
│   └── Generic.xaml               # Control template and styles
├── Data/
│   └── Products.json              # Sample product data
├── MainWindow.xaml                # Main application window
└── App.xaml                       # Application entry point
```

## How to Use

### 1. Using the DataBrowserBox Control

#### Basic Usage (Auto-Generated Columns)

```xaml
<controls:DataBrowserBox
    Height="30"
    ItemsSource="{Binding Products}"
    SelectedItem="{Binding SelectedProduct, Mode=TwoWay}"
    DisplayMemberPath="Name"
    DialogTitle="Select Product"
    DialogService="{StaticResource DialogService}" />
```

#### Advanced Usage (Custom Columns)

```xaml
<controls:DataBrowserBox
    Height="30"
    ItemsSource="{Binding Products}"
    SelectedItem="{Binding SelectedProduct, Mode=TwoWay}"
    DisplayMemberPath="Name"
    DialogTitle="Select Product"
    DialogService="{StaticResource DialogService}">
    <controls:DataBrowserBox.Columns>
        <local:BrowserColumn DataField="Name" Header="שם מוצר" Width="200"/>
        <local:BrowserColumn DataField="Price" Header="מחיר" Width="100" Format="C2" HorizontalAlignment="Right"/>
        <local:BrowserColumn DataField="Category" Header="קטגוריה" Width="150"/>
        <local:BrowserColumn DataField="Code" Header="קוד" Width="120"/>
    </controls:DataBrowserBox.Columns>
</controls:DataBrowserBox>
```

**Properties:**
- `ItemsSource` - The collection of items to browse
- `SelectedItem` - The currently selected item (two-way binding)
- `DisplayMemberPath` - Property name to display in the control
- `DialogTitle` - Title for the browse dialog
- `DialogService` - Service for showing the dialog
- `Columns` - Optional custom column definitions (if not specified, columns are auto-generated)

**BrowserColumn Properties:**
- `DataField` - The property name to bind to
- `Header` - Column header text (supports Hebrew and other languages)
- `Width` - Column width in pixels (optional)
- `Format` - String format (e.g., "C2" for currency, "N0" for numbers)
- `HorizontalAlignment` - Text alignment: "Left", "Right", or "Center"

### 2. Setting Up Dialog Service

In `App.xaml`:
```xaml
<Application.Resources>
    <services:DialogService x:Key="DialogService" />
</Application.Resources>
```

### 3. Data Format

Products.json example:
```json
[
  {
    "Id": 1,
    "Code": "LAP-001",
    "Name": "Laptop Pro 15",
    "Category": "Computers",
    "Price": 1299.99
  }
]
```

## Key Features Implementation

### Column Configuration
The control supports two modes for column display:

1. **Auto-Generated Columns** (Default)
   - Automatically creates columns for all public properties
   - Simple to use, no configuration needed
   - Good for quick prototypes and simple scenarios

2. **Custom Columns**
   - Define exactly which columns to display and in what order
   - Support for Hebrew headers and RTL languages
   - Custom formatting (currency, numbers, dates)
   - Custom column widths and alignment
   - Only displays specified columns

### Search Functionality
- Searches across all object properties
- Case-insensitive matching
- Real-time filtering as you type
- Clear button appears automatically when text is entered
- Maintains selection after filtering (if visible)

### Selection Behavior
- **Initial Selection** - Previously selected item is highlighted when dialog opens
- **Auto-Scroll** - Selected item is automatically scrolled into view
- **Filter Persistence** - Selection remains when typing filter text (if item is visible)
- **Clear Filter Persistence** - Selection is restored when clearing the filter
- **Dialog Reopening** - Selection is maintained and highlighted when dialog reopens
- **Visual Feedback** - Bold blue text on light blue background (#E3F2FD, #1976D2)

### Visual States
- **Watermark** - Gray italic text: "Click to select..." when no item is selected
- **Selected Item** - Black bold text with item name displayed in control
- **No Selection Label** - Gray "No selection" text (hidden but preserves space when item is selected)
- **Grid Selection** - Blue background (#E3F2FD) with bold blue text (#1976D2)
- **Row Hover** - Light gray background (#F5F5F5)

### Button Styles
- **OK Button** - Blue (#2196F3) with white text, darker on hover
- **Cancel Button** - Light gray (#F5F5F5) with dark text, darker on hover
- **Browse Button** - Matches Cancel button style
- **Clear Button** - Circular, transparent, appears on hover when search text exists

## Building and Running

### Prerequisites
- .NET 8.0 SDK or later
- Visual Studio 2022 (or any IDE with .NET 8 support)

### Build
```bash
dotnet build TelAvivMuni-Exercise.csproj
```

### Run
```bash
dotnet run --project TelAvivMuni-Exercise.csproj
```

Or simply press F5 in Visual Studio.

## Design Decisions

### MVVM Pattern with Attached Behaviors
- **Pure MVVM Implementation** - No code-behind event handlers for UI logic
- **Attached Behaviors** - Reusable, declarative UI behaviors defined in XAML
- **Clean Separation** - View logic separated from business logic
- **Testable** - Behaviors and ViewModels can be unit tested independently
- **Reusable** - Behaviors can be applied to any WPF control across the application

#### Implemented Behaviors:
1. **AutoFocusSearchBehavior** - Automatically focuses search box when typing starts anywhere in the dialog
2. **DataGridEnterBehavior** - Executes a command when Enter key is pressed in DataGrid
3. **DataGridScrollIntoViewBehavior** - Scrolls DataGrid to show the selected item when selection changes
4. **DialogCloseBehavior** - Closes window when ViewModel's DialogResult changes (MVVM-friendly)
5. **EscapeClearBehavior** - Clears TextBox content when Escape key is pressed

### Custom Control vs UserControl
- Chose Custom Control for better reusability
- Template-based approach for flexible styling
- Dependency properties for WPF-style usage
- Follows WPF best practices

### Dialog Service
- Abstraction layer for showing dialogs
- Easier to test and mock
- Centralized dialog logic
- Supports dependency injection
- Passes column configuration to dialog

### Error Handling Strategy
- Defensive programming approach
- Always initialize collections to empty (never null)
- User-friendly error messages
- Silent fallback for missing data files

### Selection Synchronization
- Uses `ContentRendered` event to apply selection after the view is fully rendered
- Pending selection stored in ViewModel until view is ready (`ApplyPendingSelection()`)
- Two-way XAML binding between DataGrid.SelectedItem and ViewModel.SelectedItem
- `ICollectionView.MoveCurrentTo()` synchronizes the collection view's current item
- `DataGridScrollIntoViewBehavior` scrolls to selected item (MVVM-friendly, no code-behind)
- Clean separation: ViewModel stores pending selection, View triggers application after render

### Keyboard Interaction (via Attached Behaviors)
- **Type-to-Search** - Start typing anywhere to focus search box automatically
- **Enter to Select** - Press Enter in DataGrid to select item and close dialog
- **Escape to Clear** - Press Escape in search box to clear filter text
- **Escape to Cancel** - Press Escape elsewhere to cancel and close dialog
- **No Code-Behind** - All keyboard handling implemented via reusable attached behaviors

## Recent Improvements

### MVVM Refactoring (v2.0)
- **Removed ~80 lines** of keyboard event handling code from code-behind
- **Added 4 reusable attached behaviors** for declarative UI logic
- **Improved maintainability** - Behaviors are independently testable and reusable
- **Better separation of concerns** - View logic moved from code-behind to XAML
- **Cleaner architecture** - True MVVM pattern with no UI logic in code-behind

## Future Enhancements

Potential improvements for production use:
- Add virtualization for large datasets (currently loads all items)
- Implement column sorting (click column headers to sort)
- Add column resizing (drag column borders)
- Add export functionality (CSV, Excel)
- Multi-selection support (Ctrl+Click, Shift+Click)
- Custom column templates (images, buttons, checkboxes)
- Additional behaviors (double-click to select, arrow key navigation)
- Accessibility features (screen reader support, high contrast themes)
- Full localization support (resources for all strings)
- Column reordering (drag-and-drop columns)
- Save/restore column configurations
- Advanced filtering (dropdown filters per column)

## License

This project is created for educational and interview purposes.

## Author

Created as part of the Tel-Aviv Municipality interview process for Software Developer position.

## Contact

For questions or feedback about this project, please contact through the interview process.