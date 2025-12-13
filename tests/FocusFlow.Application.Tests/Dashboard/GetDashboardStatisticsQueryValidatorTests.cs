using FluentAssertions;
using FluentValidation.TestHelper;
using FocusFlow.Application.Features.Dashboard.GetDashboardStatistics;

namespace FocusFlow.Application.Tests.Dashboard;

public class GetDashboardStatisticsQueryValidatorTests
{
	private readonly GetDashboardStatisticsQueryValidator _validator;

	public GetDashboardStatisticsQueryValidatorTests()
	{
		_validator = new GetDashboardStatisticsQueryValidator();
	}

	[Fact]
	public void Validate_WithValidUserId_ShouldNotHaveValidationErrors()
	{
		// Arrange
		var query = new GetDashboardStatisticsQuery("user123");

		// Act
		var result = _validator.TestValidate(query);

		// Assert
		result.ShouldNotHaveAnyValidationErrors();
	}

	[Theory]
	[InlineData("")]
	[InlineData(" ")]
	[InlineData(null)]
	public void Validate_WithEmptyUserId_ShouldHaveValidationError(string userId)
	{
		// Arrange
		var query = new GetDashboardStatisticsQuery(userId);

		// Act
		var result = _validator.TestValidate(query);

		// Assert
		result.ShouldHaveValidationErrorFor(x => x.UserId)
			.WithErrorMessage("User ID is required");
	}
}
