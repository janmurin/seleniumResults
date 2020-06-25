using System;

namespace OctopusCollector.Models.Deployments
{
    public class DeploymentResourceItemDto : OctopusDomainDto
    {
        public string ReleaseId { get; set; }
        public string Name { get; set; }
        public DateTime? Created { get; set; }
        public string DeployedBy { get; set; }
        public string ProjectId { get; set; }
        public string EnvironmentId { get; set; }
    }
}
