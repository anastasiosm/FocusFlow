using FluentValidation;

namespace FocusFlow.Application.Features.Dashboard.GetDashboardStatistics;

/// <summary>
/// Validator for GetDashboardStatisticsQuery
/// </summary>
public class GetDashboardStatisticsQueryValidator : AbstractValidator<GetDashboardStatisticsQuery>
{
	public GetDashboardStatisticsQueryValidator()
	{
		RuleFor(x => x.UserId)
			.NotEmpty()
			.WithMessage("User ID is required");
	}
}
