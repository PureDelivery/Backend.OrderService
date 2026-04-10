namespace OrderService.Application.Exceptions;

public class OrderException : Exception
{
    public string ErrorCode { get; }
    public string UserMessage { get; }
    public int StatusCode { get; }

    public OrderException(string errorCode, string userMessage, int statusCode = 400)
        : base(userMessage)
    {
        ErrorCode = errorCode;
        UserMessage = userMessage;
        StatusCode = statusCode;
    }

    public OrderException(string errorCode, string userMessage, Exception innerException, int statusCode = 400)
        : base(userMessage, innerException)
    {
        ErrorCode = errorCode;
        UserMessage = userMessage;
        StatusCode = statusCode;
    }
}


