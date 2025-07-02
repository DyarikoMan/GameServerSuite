using ContainerManager.Api.Models;
using ContainerManager.Infrastructure.Docker;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ContainerManager.Application.Commands;
using ContainerManager.Domain.Interfaces;
using ContainerManager.Application.Queries;

namespace ContainerManager.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContainersController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly DockerService _docker; //temp

        public ContainersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("start/{id}")]
        public async Task<IActionResult> StartContainer(string id)
        {
            var containerId = await _mediator.Send(new StartContainerCommand(id));
            return Ok(new { containerId });
        }

        [HttpPost("stop/{id}")]
        public async Task<IActionResult> StopContainer(string id)
        {
            var stopped = await _mediator.Send(new StopContainerCommand(id));
            return Ok(new { stopped });
        }

        [HttpGet("list")]
        public async Task<IActionResult> ListContainers([FromQuery] string? imageFilter)
        {
            var containers = await _mediator.Send(new ListContainersQuery(imageFilter));
            return Ok(containers);
        }


        [HttpGet("stats/{id}")]
        public async Task<IActionResult> Stats(string id)
        {
            var stats = await _docker.GetContainerStatsAsync(id);

            return Ok(new
            {
                MemoryUsageMB = stats.MemoryStats.Usage / 1024 / 1024,
                MemoryLimitMB = stats.MemoryStats.Limit / 1024 / 1024,
                stats.Read,
                stats.PidsStats,
                stats.CPUStats
            });
        }

        [HttpGet("images/files")]
        public IActionResult GetImageFiles()
        {
            var imageFiles = _docker.ListAvailableTarImages();
            return Ok(imageFiles);
        }

        [HttpPost("images/run")]
        public async Task<IActionResult> LoadImageAndStartContainer([FromBody] StartImageRequest req)
        {
            var imagesPath = Path.Combine(AppContext.BaseDirectory, "Resources", "Images");
            var tarPath = Path.Combine(imagesPath, req.TarFile);

            if (!System.IO.File.Exists(tarPath))
                return NotFound($"TAR file not found: {req.TarFile}");

            var containerBaseDir = Path.Combine(AppContext.BaseDirectory, "Resources", "Containers");

            var containerId = await _docker.LoadImageAndStartContainerAsync(
                tarPath,
                containerBaseDir,
                req.Name,
                req.Port,
                req.RamMb,
                req.Cpu,
                req.AutoRemove,
                req.RestartPolicy
            );

            return Ok(new { ContainerId = containerId });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContainer(string id)
        {
            var result = await _docker.RemoveContainerAndCleanupAsync(id);

            if (result)
                return Ok(new { message = "Container deleted successfully." });
            else
                return NotFound(new { error = "Container not found." });
        }


    }
}