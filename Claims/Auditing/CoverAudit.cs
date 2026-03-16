using System.ComponentModel.DataAnnotations;

namespace Claims.Auditing
{
    public class CoverAudit
    {
        public int Id { get; set; }

        [MaxLength(36)]
        public required string CoverId { get; set; }

        public DateTime Created { get; set; }

        [MaxLength(10)]
        public required string HttpRequestType { get; set; }
    }
}
