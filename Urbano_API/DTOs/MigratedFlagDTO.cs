namespace Urbano_API.DTOs;

public class SetMigratedFlagDTO
{
    public string UserId { get; set; } = null!;
    public bool Migrated { get; set; }
}
