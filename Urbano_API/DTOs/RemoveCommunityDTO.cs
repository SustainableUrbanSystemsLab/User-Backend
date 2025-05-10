using Urbano_API.Models;

namespace Urbano_API.DTOs;

public class RemoveCommunityDTO
{
    public string UserId { get; set; } = null!;
    public CommunityType Community { get; set; }
}