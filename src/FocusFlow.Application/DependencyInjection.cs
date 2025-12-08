using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace FocusFlow.Application;

public static class DependencyInjection
{
	public static IServiceCollection AddApplication(this IServiceCollection services)
	{
		var assembly = Assembly.GetExecutingAssembly();

		// Register MediatR
		services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

		// Register AutoMapper
		services.AddAutoMapper(assembly);

		// Register FluentValidation
		services.AddValidatorsFromAssembly(assembly);

		return services;
	}
}