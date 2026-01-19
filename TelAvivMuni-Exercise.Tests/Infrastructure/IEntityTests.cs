using TelAvivMuni_Exercise.Infrastructure;
using TelAvivMuni_Exercise.Models;
using Xunit;

namespace TelAvivMuni_Exercise.Tests.Infrastructure;

public class IEntityTests
{
    [Fact]
    public void Product_ImplementsIEntity()
    {
        // Arrange
        var product = new Product { Id = 42 };

        // Act & Assert
        Assert.IsAssignableFrom<IEntity>(product);
        Assert.Equal(42, ((IEntity)product).Id);
    }

    [Fact]
    public void IEntity_IdCanBeSetAndGet()
    {
        // Arrange
        IEntity entity = new Product();

        // Act
        entity.Id = 123;

        // Assert
        Assert.Equal(123, entity.Id);
    }
}
