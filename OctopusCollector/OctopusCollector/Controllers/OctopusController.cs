using Microsoft.AspNetCore.Mvc;
using OctopusCollector.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OctopusCollector.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OctopusController : ControllerBase
    {
        private readonly OctopusService _octopusService;

        public OctopusController(OctopusService octopusService)
        {
            _octopusService = octopusService;
        }

        [HttpGet("projects")]
        public async Task<IActionResult> GetAllProjectsAsync()
        {
            try
            {
                return Ok(await _octopusService.GetAllProjectsAsync());
            }
            catch(Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpGet("projects/{projectId}/releases")]
        public async Task<IActionResult> GetReleases(string projectId)
        {
            try
            {
                return Ok(await _octopusService.GetAllProjectReleasesAsync(projectId));
            }
            catch(Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpGet("releases/{releaseId}/deployments")]
        public async Task<IActionResult> GetDeploymentsAsync(string releaseId)
        {
            try
            {
                return Ok(await _octopusService.GetAllProjectDeployments(releaseId));
            }
            catch(Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpGet("artifacts/{deploymentId}")]
        public async Task<IActionResult> GetArtifactsAsync(string deploymentId)
        {
            try
            {
                return Ok(await _octopusService.GetAllProjectArtifacts(deploymentId));
            }
            catch (Exception e)
            {
                return BadRequest(e);
            }
        }

        [HttpGet("artifacts/{artifactId}/download")]
        public async Task<IActionResult> GetArtifactAsync(string artifactId)
        {
            try
            {
                var fileDto = await _octopusService.GetArtifactAsync(artifactId);
                return File(fileDto.Content.ToArray(), fileDto.ContentType, fileDto.FileName.Trim(new[] { '/', '\\', '"' }));
            }
            catch(Exception e)
            {
                return BadRequest(e);
            }
        }
    }
}
