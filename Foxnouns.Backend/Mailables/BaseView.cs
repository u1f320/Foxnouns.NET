namespace Foxnouns.Backend.Mailables;

public abstract class BaseView
{
    public required string BaseUrl { get; init; }
    public required string To { get; init; }
}