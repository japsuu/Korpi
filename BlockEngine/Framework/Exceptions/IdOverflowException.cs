namespace BlockEngine.Framework.Exceptions;

public class IdOverflowException : Exception
{
    public IdOverflowException(string message) : base(message) { }
}