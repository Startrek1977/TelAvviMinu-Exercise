# TelAviv Municipality Exercise

A WPF application demonstrating a reusable data browser control for selecting items from collections with search and filtering capabilities.

## Project Overview

This project is a home exercise created as part of an interview for the Software Developer position at Tel-Aviv Municipality. It showcases a custom WPF control (`DataBrowserBox`) that provides a modern, user-friendly interface for browsing and selecting items from data collections.

## Features

### Custom DataBrowserBox Control
- **Reusable WPF Custom Control** - Can be used anywhere in the application with any data type
- **Display Member Path** - Configure which property to display in the control
- **Two-Way Data Binding** - Full support for MVVM pattern with `SelectedItem` binding
- **Visual Feedback** - Watermark text in italic, selected items in bold
- **Modern UI** - Styled browse button with hover effects

### Browse Dialog
- **Modern Design** - Clean, professional interface with Material Design-inspired colors
- **Search & Filter** - Real-time search across all item properties
- **Clear Filter Button** - Appears when search text is entered, clears with one click
- **Sortable Columns** - Custom column definitions (Id, Code, Name, Category, Price)
- **Selection Highlighting** - Bold text and blue background for selected items
- **Row Hover Effects** - Visual feedback on mouse over
- **Item Counter** - Displays total filtered items count
- **Responsive Design** - Resizable dialog with proper scrolling

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
├── Models/
│   └── Product.cs                 # Product data model
├── Themes/
│   └── Generic.xaml               # Control template and styles
├── Data/
│   └── Products.json              # Sample product data
├── MainWindow.xaml                # Main application window
└── App.xaml                       # Application entry point
```

## How to Use

### 1. Using the DataBrowserBox Control

```xaml
<controls:DataBrowserBox
    Height="30"
    ItemsSource="{Binding Products}"
    SelectedItem="{Binding SelectedProduct, Mode=TwoWay}"
    DisplayMemberPath="Name"
    DialogTitle="Select Product"
    DialogService="{StaticResource DialogService}" />
```

**Properties:**
- `ItemsSource` - The collection of items to browse
- `SelectedItem` - The currently selected item (two-way binding)
- `DisplayMemberPath` - Property name to display in the control
- `DialogTitle` - Title for the browse dialog
- `DialogService` - Service for showing the dialog

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

### Search Functionality
- Searches across all object properties
- Case-insensitive matching
- Real-time filtering as you type
- Clear button appears automatically when text is entered

### Visual States
- **Watermark** - Gray italic text: "Click to select..."
- **Selected Item** - Black bold text with item name
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

### MVVM Pattern
- Clean separation of concerns
- Testable business logic
- Reusable ViewModels
- Proper data binding

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

### Error Handling Strategy
- Defensive programming approach
- Always initialize collections to empty (never null)
- User-friendly error messages
- Silent fallback for missing data files

## Future Enhancements

Potential improvements for production use:
- Add virtualization for large datasets
- Implement column sorting
- Add export functionality (CSV, Excel)
- Multi-selection support
- Custom column templates
- Keyboard navigation improvements
- Accessibility features (screen reader support)
- Localization support

## License

This project is created for educational and interview purposes.

## Author

Created as part of the Tel-Aviv Municipality interview process for Software Developer position.

## Contact

For questions or feedback about this project, please contact through the interview process.