using ContainerManager.Api.Models;
using ContainerManager.Infrastructure.Docker;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ContainerManager.Application.Commands;

namespace ContainerManager.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContainersController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly DockerService _docker; //temp

        public ContainersController(IMediator mediator, DockerService docker)
        {
            _mediator = mediator;
            _docker = docker; //temp
        }

        [HttpPost("start")]
        public async Task<IActionResult> Start([FromBody] StartContainerCommand command)
        {
            var containerId = await _mediator.Send(command);
            return Ok(new { ContainerId = containerId });
        }

        [HttpPost("stop")]
        public async Task<IActionResult> Stop([FromBody] string id)
        {
            var success = await _docker.StopContainerAsync(id);
            return Ok(new { Stopped = success });
        }

        [HttpGet("list")]
        public async Task<IActionResult> List()
        {
            var containers = await _docker.ListContainersAsync();
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