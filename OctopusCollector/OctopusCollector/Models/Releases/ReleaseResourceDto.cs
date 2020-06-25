using System.Collections.Generic;

namespace OctopusCollector.Models.Releases
{
    public class ReleaseResourceDto : OctopusDomainDto
    {
        public IReadOnlyList<ReleaseResourceItemDto> Items { get; set; }
    }
}
