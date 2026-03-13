using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;

namespace Claims
{
    public class Claim
    {
        [BsonId]
        public required string Id { get; set; }

        [BsonElement("coverId")]
        public required string CoverId { get; set; }

        [BsonElement("created")]
        public DateTime Created { get; set; }

        [BsonElement("name")]
        public required string Name { get; set; }

        [BsonElement("claimType")]
        public ClaimType Type { get; set; }

        [BsonElement("damageCost")]
        public decimal DamageCost { get; set; }
    }

    public enum ClaimType
    {
        Collision = 0,
        Grounding = 1,
        BadWeather = 2,
        Fire = 3
    }
}
