namespace Infrastructure;

public class ErrorResult
{
    public string? Code { get; set; }
    
    public required string Message { get; set; }
}