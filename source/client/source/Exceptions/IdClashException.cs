namespace Korpi.Client.Exceptions;

public class IdClashException : Exception
{
    public IdClashException(string message) : base(message) { }
}