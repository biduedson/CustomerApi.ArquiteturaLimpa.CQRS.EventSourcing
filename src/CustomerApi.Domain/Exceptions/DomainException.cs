namespace CustomerApi.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException() { }
    public DomainException(string message)
        : base(message) { }

    public DomainException(string message, Exception innerException)
    : base(message, innerException) { }

    public static void ThrowIf(bool condition, string message)
    {
        if(condition) throw new DomainException(message);
    }
    public static void ThrowIfNull(object? obj, string message)
    { 
        if(obj is  null) throw new DomainException(message);
    }

}
