using System.Collections.Generic;

namespace OctopusCollector.Models.Deployments
{
    public class DeploymentResourceDto
    {
        public IReadOnlyList<DeploymentResourceItemDto> Items { get; set; }
    }
}
