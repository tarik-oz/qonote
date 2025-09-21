namespace Qonote.Infrastructure.Messaging;

public class EmailSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string SenderAddress { get; set; } = string.Empty;
}
