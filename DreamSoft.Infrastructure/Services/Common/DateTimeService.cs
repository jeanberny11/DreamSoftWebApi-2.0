using DreamSoft.Application.Common.Interfaces;

namespace DreamSoft.Infrastructure.Services.Common;

public class DateTimeService : IDateTime
{
    public DateTime Now => DateTime.Now;
    public DateTime UtcNow => DateTime.UtcNow;
}