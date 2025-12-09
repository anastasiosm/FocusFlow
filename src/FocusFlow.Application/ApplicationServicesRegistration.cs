using System.Reflection;
using FluentValidation;
using FocusFlow.Application.Common.Behaviours;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace FocusFlow.Application;

public static class ApplicationServicesRegistration
{
	public static IServiceCollection AddApplicationServices(this IServiceCollection services)
	{
		var assembly = Assembly.GetExecutingAssembly();

		// Register MediatR
		services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
		services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

		// Register AutoMapper
		services.AddAutoMapper(assembly);

		// Register FluentValidation
		services.AddValidatorsFromAssembly(assembly);

		return services;
	}
}