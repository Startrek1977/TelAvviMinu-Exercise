using TelAvivMuni_Exercise.Infrastructure;
using Xunit;

namespace TelAvivMuni_Exercise.Tests.Infrastructure;

public class OperationResultTests
{
    #region Ok Method Tests

    [Fact]
    public void Ok_ReturnsSuccessResult()
    {
        // Act
        var result = OperationResult.Ok();

        // Assert
        Assert.True(result.Success);
    }

    [Fact]
    public void Ok_ErrorMessageIsNull()
    {
        // Act
        var result = OperationResult.Ok();

        // Assert
        Assert.Null(result.ErrorMessage);
    }

    #endregion

    #region Fail Method Tests

    [Fact]
    public void Fail_ReturnsFailedResult()
    {
        // Act
        var result = OperationResult.Fail();

        // Assert
        Assert.False(result.Success);
    }

    [Fact]
    public void Fail_WithErrorMessage_SetsErrorMessage()
    {
        // Arrange
        var errorMessage = "Something went wrong";

        // Act
        var result = OperationResult.Fail(errorMessage);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(errorMessage, result.ErrorMessage);
    }

    [Fact]
    public void Fail_WithNullErrorMessage_ErrorMessageIsNull()
    {
        // Act
        var result = OperationResult.Fail(null);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void Fail_WithEmptyErrorMessage_SetsEmptyErrorMessage()
    {
        // Act
        var result = OperationResult.Fail(string.Empty);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(string.Empty, result.ErrorMessage);
    }

    #endregion

    #region Implicit Bool Conversion Tests

    [Fact]
    public void ImplicitBoolConversion_SuccessResult_ReturnsTrue()
    {
        // Arrange
        var result = OperationResult.Ok();

        // Act
        bool boolValue = result;

        // Assert
        Assert.True(boolValue);
    }

    [Fact]
    public void ImplicitBoolConversion_FailedResult_ReturnsFalse()
    {
        // Arrange
        var result = OperationResult.Fail();

        // Act
        bool boolValue = result;

        // Assert
        Assert.False(boolValue);
    }

    [Fact]
    public void ImplicitBoolConversion_CanBeUsedInIfStatement()
    {
        // Arrange
        var successResult = OperationResult.Ok();
        var failResult = OperationResult.Fail();
        var successPath = false;
        var failPath = false;

        // Act
        if (successResult)
        {
            successPath = true;
        }

        if (!failResult)
        {
            failPath = true;
        }

        // Assert
        Assert.True(successPath);
        Assert.True(failPath);
    }

    #endregion
}
