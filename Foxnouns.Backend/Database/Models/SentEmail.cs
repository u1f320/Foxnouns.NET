using System.ComponentModel.DataAnnotations.Schema;
using NodaTime;

namespace Foxnouns.Backend.Database.Models;

public class SentEmail
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    public required string Email { get; init; }
    public required Instant SentAt { get; init; }
}
