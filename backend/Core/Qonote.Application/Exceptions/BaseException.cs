namespace Qonote.Core.Application.Exceptions;

public abstract class BaseException : Exception
{
    protected BaseException(string message) : base(message) { }
}
