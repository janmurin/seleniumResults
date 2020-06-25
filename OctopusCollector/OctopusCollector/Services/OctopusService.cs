using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OctopusCollector.Models.Artifacts;
using OctopusCollector.Models.Deployments;
using OctopusCollector.Models.Projects;
using OctopusCollector.Models.Releases;
using OctopusCollector.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace OctopusCollector.Services
{
    public class OctopusService
    {
        private readonly OctopusOptions _options;
        private readonly IHttpClientFactory _httpClientFactory;

        public OctopusService(IOptions<OctopusOptions> optionsAccessor, IHttpClientFactory httpClientFactory)
        {
            _options = optionsAccessor.Value;
            _httpClientFactory = httpClientFactory;
        }

        // api/projects/all
        public Task<IList<ProjectResourceDto>> GetAllProjectsAsync()
        {
            return ExecuteRequestAsync<IList<ProjectResourceDto>>("api/projects/all");
        }

        // api/projects/{id}/releases // Releases-79070
        public Task<ReleaseResourceDto> GetAllProjectReleasesAsync(string projectId)
        {
            return ExecuteRequestAsync<ReleaseResourceDto>($"api/projects/{projectId}/releases"); 
        }

        // api/releases/{id}/deployments // Deployments-164319
        public Task<DeploymentResourceDto> GetAllProjectDeployments(string releaseId)
        {
            return ExecuteRequestAsync<DeploymentResourceDto>($"api/releases/{releaseId}/deployments");
        }

        // api/artifacts?regarding={deploymentId}
        public Task<ArtifactResourceDto> GetAllProjectArtifacts(string deploymentId)
        {
            return ExecuteRequestAsync<ArtifactResourceDto>($"api/artifacts?regarding={deploymentId}&skip=0&take=1000");
        }

        // api/artifacts/{id}/content
        public async Task<ArtifactFileDto> GetArtifactAsync(string artifactId)
        {
            using var httpClient = _httpClientFactory.CreateClient();
            var httpRequest = GetRequest($"api/artifacts/{artifactId}/content");
            var httpResponse = await httpClient.SendAsync(httpRequest);

            if (httpResponse.IsSuccessStatusCode)
            {
                var responseData = await httpResponse.Content.ReadAsByteArrayAsync();
                return new ArtifactFileDto
                {
                    Content = responseData,
                    ContentType = httpResponse.Content?.Headers?.ContentType?.MediaType ?? "application/octet-stream",
                    FileName = httpResponse.Content?.Headers?.ContentDisposition?.FileName ?? "file"
                };
            }

            var responseString = await httpResponse.Content.ReadAsStringAsync();
            throw new Exception($"Error occured while fetching projects: {httpResponse.ReasonPhrase}, {responseString}");
        }

        private async Task<T> ExecuteRequestAsync<T>(string apiUri)
        {
            using var httpClient = _httpClientFactory.CreateClient();
            var httpRequest = GetRequest(apiUri);
            var httpResponse = await httpClient.SendAsync(httpRequest);
            var responseString = await httpResponse.Content.ReadAsStringAsync();

            if (httpResponse.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<T>(responseString);
            }

            throw new Exception($"Error occured while fetching projects: {httpResponse.ReasonPhrase}, {responseString}");
        }

        private HttpRequestMessage GetRequest(string apiUri)
        {
            var uri = $"{_options.BaseUrl.TrimEnd('/')}/{apiUri}";
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, uri);
            AddHeaders(httpRequest);
            return httpRequest;
        }

        private void AddHeaders(HttpRequestMessage request)
        {
            const string mediaType = "application/json";
            const string octopusApiHeader = "X-Octopus-ApiKey";
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaType));
            request.Headers.Add(octopusApiHeader, _options.ApiKey);
        }
    }
}
