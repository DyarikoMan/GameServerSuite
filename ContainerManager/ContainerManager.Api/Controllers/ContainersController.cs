using ContainerManager.Api.Models;
using ContainerManager.Application.Commands;
using ContainerManager.Application.Dtos;
using ContainerManager.Application.Queries;
using ContainerManager.Application.Queries.GetContainerStats;
using ContainerManager.Domain.Interfaces;
using ContainerManager.Infrastructure.Docker;
using ImageManager.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

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

        [HttpPost("start")]
        public async Task<IActionResult> StartContainer([FromBody] StartContainerRequest request)
        {
            var command = new StartContainerCommand(request);

            string containerId = await _mediator.Send(command);

            return Ok(new { ContainerId = containerId });
        }

        [HttpPost("resume/{id}")]
        public async Task<IActionResult> ResumeContainer(string id)
        {
            var containerId = await _mediator.Send(new ResumeContainerCommand(id));
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
        public async Task<IActionResult> GetStats(string id)
        {
            var stats = await _mediator.Send(new GetContainerStatsQuery(id));

            var dto = new ContainerStatsDto
            {
                MemoryUsage = stats.MemoryUsage,
                MemoryLimit = stats.MemoryLimit,
                CpuTotalUsage = stats.CpuTotalUsage,
                CpuSystemUsage = stats.CpuSystemUsage,
                CpuCores = stats.CpuCores,
                NetworkRxBytes = stats.NetworkRxBytes,
                NetworkTxBytes = stats.NetworkTxBytes
            };

            return Ok(dto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveContainer(string id)
        {
            var containerId = await _mediator.Send(new RemoveContainerCommand(id));
            return Ok(new { containerId });
        }

        [HttpPost("load")]
        public async Task<IActionResult> LoadImage([FromBody] string tarFile)
        {
            var imagesPath = Path.Combine(AppContext.BaseDirectory, "Resources", "Images");
            var tarPath = Path.Combine(imagesPath, tarFile);

            if (!System.IO.File.Exists(tarPath))
                return NotFound($"TAR file not found: {tarFile}");

            var name = await _mediator.Send(new LoadImageQuery(tarPath));
            return Ok(new { ImageName = name });
        }

        //[HttpGet("images/files")]
        //public IActionResult GetImageFiles()
        //{
        //    //var imageFiles = _docker.ListAvailableTarImages();
        //    //return Ok(imageFiles);
        //}


    }
}