using System.Collections.Generic;

namespace OctopusCollector.Models.Artifacts
{
    public class ArtifactFileDto
    {
        public IReadOnlyList<byte> Content { get; set; }
        public string ContentType { get; set; }
        public string FileName { get; set; }
    }
}
