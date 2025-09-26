namespace Devplus.Security.OAuth.Contracts;

public class ExchangeCodeRequestDto
{
    public string Code { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
}
