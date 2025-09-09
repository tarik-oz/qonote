namespace Qonote.Core.Application.Exceptions;

public class NotFoundException : BaseException
{
    public NotFoundException(string message) : base(message) { }
}