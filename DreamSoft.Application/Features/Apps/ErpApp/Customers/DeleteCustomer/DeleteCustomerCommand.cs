using MediatR;

namespace DreamSoft.Application.Features.Apps.ErpApp.Customers.DeleteCustomer;

public record DeleteCustomerCommand(int CustomerId) : IRequest;
