namespace VolcanicTransport.Model.Exceptions;

public class GenerationErrorException : Exception 
{
    public GenerationErrorException(string message) : base(message) { }
    public GenerationErrorException() : base() { }
}