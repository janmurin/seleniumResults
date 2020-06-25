using System.Collections.Generic;

namespace OctopusCollector.Models.Artifacts
{
    public class ArtifactResourceDto : OctopusDomainDto
    {
        public IReadOnlyList<ArtifactResourceItemDto> Items { get; set; }
    }
}
