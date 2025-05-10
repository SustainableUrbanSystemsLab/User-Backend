namespace Urbano_API.DTOs;

using Urbano_API.Models;

public class RegistrationsDTO
{
    public string? Id { get; set; }
    public int RegistrationsCount { get; set; } = 0;
    public CommunityType Community { get; set; }
}