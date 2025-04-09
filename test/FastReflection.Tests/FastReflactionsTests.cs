using Shouldly;

namespace FastReflection.Tests;

public class FastReflectionsTests
{
    private class TestClass
    {
        public string Name { get; set; } = "Initial";
        public int Add(int a, int b) => a + b;
    }

    [Fact(DisplayName = "Should invoke method dynamically")]
    public void Invoke_ShouldCallMethodCorrectly()
    {
        // Arrange
        var instance = new TestClass();
        var method = typeof(TestClass).GetMethod(nameof(TestClass.Add));
        method.ShouldNotBeNull();

        // Act
        var result = FastReflections.Invoke<TestClass, int>(method, instance, 2, 3);

        // Assert
        result.ShouldBe(5);
    }

    [Fact(DisplayName = "Should retrieve property value dynamically")]
    public void GetPropertyValue_ShouldRetrieveValue()
    {
        // Arrange
        var instance = new TestClass { Name = "TestValue" };

        // Act
        var value = FastReflections.GetPropertyValue(instance, x => x.Name);

        // Assert
        value.ShouldBe("TestValue");
    }

    [Fact(DisplayName = "Should set property value dynamically")]
    public void SetPropertyValue_ShouldUpdateProperty()
    {
        // Arrange
        var instance = new TestClass();

        // Act
        FastReflections.SetPropertyValue(instance, x => x.Name, "UpdatedValue");

        // Assert
        instance.Name.ShouldBe("UpdatedValue");
    }

    [Fact(DisplayName = "Should retrieve type by name")]
    public void GetTypeByName_ShouldReturnCorrectType()
    {
        // Act
        var type = FastReflections.GetTypeByName("FastReflection.Tests.FastReflectionsTests+TestClass");

        // Assert
        type.ShouldNotBeNull();
        type.ShouldBe(typeof(TestClass));
    }
}
