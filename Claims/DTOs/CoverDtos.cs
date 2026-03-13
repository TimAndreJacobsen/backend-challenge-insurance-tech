using Claims.Models;

namespace Claims.DTOs;

public record CreateCoverRequest(
    DateTime StartDate,
    DateTime EndDate,
    CoverType Type
);

public record CoverResponse(
    string Id,
    DateTime StartDate,
    DateTime EndDate,
    CoverType Type,
    decimal Premium
)
{
    public static CoverResponse FromEntity(Cover cover) =>
        new(cover.Id, cover.StartDate, cover.EndDate, cover.Type, cover.Premium);
}
