namespace Foxnouns.Backend.Database.Models;

public class Field
{
    public required string Name { get; set; }
    public FieldEntry[] Entries { get; set; } = [];
}

public class FieldEntry
{
    public required string Value { get; set; }
    public required string Status { get; set; }
}

public class Pronoun : FieldEntry
{
    public string? DisplayText { get; set; }
}
