using Ardalis.Result;
using FluentValidation;
using MediatR;

namespace RespectLedger.Application.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        ValidationContext<TRequest> context = new(request);

        FluentValidation.Results.ValidationResult[] validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        List<FluentValidation.Results.ValidationFailure> failures = validationResults
            .Where(r => r.Errors.Any())
            .SelectMany(r => r.Errors)
            .ToList();

        if (failures.Any())
        {
            // If TResponse is Result or Result<T>, return validation errors
            if (typeof(TResponse).IsGenericType && 
                typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
            {
                Type resultType = typeof(Result<>).MakeGenericType(typeof(TResponse).GetGenericArguments()[0]);
                System.Reflection.MethodInfo? invalidMethod = resultType.GetMethod("Invalid", new[] { typeof(List<ValidationError>) });
                
                if (invalidMethod != null)
                {
                    List<ValidationError> validationErrors = failures.Select(f => 
                        new ValidationError
                        {
                            Identifier = f.PropertyName,
                            ErrorMessage = f.ErrorMessage
                        }).ToList();
                    
                    return (TResponse)invalidMethod.Invoke(null, new object[] { validationErrors })!;
                }
            }
            else if (typeof(TResponse) == typeof(Result))
            {
                List<ValidationError> validationErrors = failures.Select(f => 
                    new ValidationError
                    {
                        Identifier = f.PropertyName,
                        ErrorMessage = f.ErrorMessage
                    }).ToList();
                
                return (TResponse)(object)Result.Invalid(validationErrors);
            }

            throw new ValidationException(failures);
        }

        return await next();
    }
}
