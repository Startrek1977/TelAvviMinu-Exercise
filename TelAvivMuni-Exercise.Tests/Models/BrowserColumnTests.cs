using TelAvivMuni_Exercise.Models;
using Xunit;

namespace TelAvivMuni_Exercise.Tests.Models;

public class BrowserColumnTests
{
    #region Default Values Tests

    [Fact]
    public void Constructor_DataField_DefaultsToEmptyString()
    {
        // Act
        var column = new BrowserColumn();

        // Assert
        Assert.Equal(string.Empty, column.DataField);
    }

    [Fact]
    public void Constructor_Header_DefaultsToEmptyString()
    {
        // Act
        var column = new BrowserColumn();

        // Assert
        Assert.Equal(string.Empty, column.Header);
    }

    [Fact]
    public void Constructor_Width_DefaultsToNaN()
    {
        // Act
        var column = new BrowserColumn();

        // Assert
        Assert.True(double.IsNaN(column.Width));
    }

    [Fact]
    public void Constructor_Format_DefaultsToNull()
    {
        // Act
        var column = new BrowserColumn();

        // Assert
        Assert.Null(column.Format);
    }

    [Fact]
    public void Constructor_HorizontalAlignment_DefaultsToNull()
    {
        // Act
        var column = new BrowserColumn();

        // Assert
        Assert.Null(column.HorizontalAlignment);
    }

    #endregion

    #region Property Setter Tests

    [Fact]
    public void DataField_CanBeSetAndRetrieved()
    {
        // Arrange
        var column = new BrowserColumn();

        // Act
        column.DataField = "ProductName";

        // Assert
        Assert.Equal("ProductName", column.DataField);
    }

    [Fact]
    public void Header_CanBeSetAndRetrieved()
    {
        // Arrange
        var column = new BrowserColumn();

        // Act
        column.Header = "Product Name";

        // Assert
        Assert.Equal("Product Name", column.Header);
    }

    [Fact]
    public void Width_CanBeSetAndRetrieved()
    {
        // Arrange
        var column = new BrowserColumn();

        // Act
        column.Width = 150.5;

        // Assert
        Assert.Equal(150.5, column.Width);
    }

    [Fact]
    public void Format_CanBeSetAndRetrieved()
    {
        // Arrange
        var column = new BrowserColumn();

        // Act
        column.Format = "{0:C2}";

        // Assert
        Assert.Equal("{0:C2}", column.Format);
    }

    [Fact]
    public void HorizontalAlignment_CanBeSetAndRetrieved()
    {
        // Arrange
        var column = new BrowserColumn();

        // Act
        column.HorizontalAlignment = "Right";

        // Assert
        Assert.Equal("Right", column.HorizontalAlignment);
    }

    #endregion

    #region Object Initializer Tests

    [Fact]
    public void ObjectInitializer_SetsAllProperties()
    {
        // Act
        var column = new BrowserColumn
        {
            DataField = "Price",
            Header = "Unit Price",
            Width = 100.0,
            Format = "{0:C}",
            HorizontalAlignment = "Right"
        };

        // Assert
        Assert.Equal("Price", column.DataField);
        Assert.Equal("Unit Price", column.Header);
        Assert.Equal(100.0, column.Width);
        Assert.Equal("{0:C}", column.Format);
        Assert.Equal("Right", column.HorizontalAlignment);
    }

    #endregion
}
