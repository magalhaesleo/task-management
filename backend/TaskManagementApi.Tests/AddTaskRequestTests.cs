using System.ComponentModel.DataAnnotations;
using TaskManagementApi.Tasks;

namespace TaskManagementApi.Tests;

public class AddTaskRequestTests
{
    [Fact]
    public void Given_valid_request_should_return_no_errors()
    {
        // Arrange
        var request = new AddTaskRequest
        {
            Id = Guid.NewGuid(),
            Title = "Title",
            Content = "Content",
        };
        
        // Act
        var validationResults = ValidateModel(request);
        
        // Assert
        Assert.Empty(validationResults);
    }
    
    [Fact]
    public void Given_request_without_id_should_return_error()
    {
        // Arrange
        var request = new AddTaskRequest
        {
            Id = Guid.Empty,
            Title = "Title",
            Content = "Content",
        };
        
        // Act
        var validationResults = ValidateModel(request);
        
        // Assert
        Assert.Equal(nameof(AddTaskRequest.Id), Assert.Single(Assert.Single(validationResults).MemberNames));
    }
    
    [Fact]
    public void Given_request_without_title_should_return_error()
    {
        // Arrange
        var request = new AddTaskRequest
        {
            Id = Guid.NewGuid(),
            Title = null,
            Content = "Content",
        };
        
        // Act
        var validationResults = ValidateModel(request);
        
        // Assert
        Assert.Equal(nameof(AddTaskRequest.Title), Assert.Single(Assert.Single(validationResults).MemberNames));
    }
    
    [Fact]
    public void Given_request_without_content_should_have_no_errors()
    {
        // Arrange
        var request = new AddTaskRequest
        {
            Id = Guid.NewGuid(),
            Title = "Title",
            Content = null,
        };
        
        // Act
        var validationResults = ValidateModel(request);
        
        // Assert
        Assert.Empty(validationResults);
    }
    
    private static List<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var ctx = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, ctx, validationResults, true);
        return validationResults;
    }
}