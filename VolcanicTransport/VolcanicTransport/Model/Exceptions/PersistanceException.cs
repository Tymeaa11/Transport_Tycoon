namespace VolcanicTransport.Model.Exceptions;
public class PersistanceException : Exception {
    public PersistanceException() : base() { }
    public PersistanceException(string message) : base(message) { }
}
