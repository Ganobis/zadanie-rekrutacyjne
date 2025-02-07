using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ZadanieRekrutacyjne_WojciechGanobis.Interfaces;
using ZadanieRekrutacyjne_WojciechGanobis.Models.Enums;
using ZadanieRekrutacyjne_WojciechGanobis.Services;

namespace ZadanieRekrutacyjne_WojciechGanobis.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TagsController : ControllerBase
{
    private readonly IStackOverflowTagService _stackOverflowTagService;
    private readonly ITagService _tagService;

    public TagsController(IStackOverflowTagService stackOverflowTagService, ITagService tagService)
    {
        _stackOverflowTagService = stackOverflowTagService;
        _tagService = tagService;
    }

    [HttpGet]
    public IActionResult GetTags(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] SortBy sortBy = SortBy.NameAscending)
    {
        try
        {
            var tags = _tagService.GetTags(page, pageSize, sortBy);
            return Ok(tags);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshTags()
    {
        try
        {
            var tags = await _stackOverflowTagService.GetTagsAsyncTask();
            await _tagService.InitializeTagsAsync(tags);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}