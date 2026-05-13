namespace VolcanicTransport.Model.Exceptions;
public class PersistanceException : Exception
{
    public PersistanceException() { }
    public PersistanceException(string message) : base(message) { }
}
