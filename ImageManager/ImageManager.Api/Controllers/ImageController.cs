using ImageManager.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ImageManager.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImageController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ImageController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // POST: api/image/load
        [HttpPost("load")]
        public async Task<IActionResult> LoadImage([FromBody] string tarFileName)
        {
            var tarPath = Path.Combine(AppContext.BaseDirectory, "Resources", "Images", tarFileName);

            var imageName = await _mediator.Send(new LoadImageQuery(tarPath));

            if (string.IsNullOrWhiteSpace(imageName))
                return StatusCode(500, "❌ Failed to load image or extract name.");

            return Ok(new { Image = imageName });
        }

        // GET: api/image/list
        [HttpGet("list")]
        public async Task<IActionResult> ListTarImages()
        {
            var images = await _mediator.Send(new ListTarImagesQuery());
            return Ok(images);
        }
    }
}
