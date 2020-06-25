using System;

namespace OctopusCollector.Models.Artifacts
{
    public class ArtifactResourceItemDto : OctopusDomainDto
    {
        public string Filename { get; set; }
        public string Source { get; set; }
        public DateTime? Created { get; set; }
    }
}
