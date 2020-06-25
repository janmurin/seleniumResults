namespace OctopusCollector.Models.Projects
{
    public class ProjectResourceDto : OctopusDomainDto
    {
        public string Name { get; set; }
        public string Slug { get; set; }
        public bool IsDisabled { get; set; }
    }
}
