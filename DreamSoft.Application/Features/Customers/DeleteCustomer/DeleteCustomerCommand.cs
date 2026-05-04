using MediatR;

namespace DreamSoft.Application.Features.Customers.DeleteCustomer;

public record DeleteCustomerCommand(int CustomerId) : IRequest;
