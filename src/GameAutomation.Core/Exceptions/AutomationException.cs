namespace GameAutomation.Core;

public class AutomationException : Exception
{
    public AutomationException() : base()
    {
    }

    public AutomationException(string message) : base(message)
    {
    }

    public AutomationException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
