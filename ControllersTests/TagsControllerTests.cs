using Microsoft.AspNetCore.Mvc;
using Moq;
using ZadanieRekrutacyjne_WojciechGanobis.Controllers;
using ZadanieRekrutacyjne_WojciechGanobis.Interfaces;
using ZadanieRekrutacyjne_WojciechGanobis.Models;
using ZadanieRekrutacyjne_WojciechGanobis.Models.Enums;

namespace ControllersTests;

public class TagsControllerTests
{
    #region Private fields

    private readonly Mock<IStackOverflowTagService> _mockStackOverflowTagService;
    private readonly Mock<ITagService> _mockTagService;
    private readonly TagsController _controller;

    #endregion
    #region Contructors

    public TagsControllerTests()
    {
        _mockStackOverflowTagService = new Mock<IStackOverflowTagService>();
        _mockTagService = new Mock<ITagService>();
        _controller = new TagsController(_mockStackOverflowTagService.Object, _mockTagService.Object);
    }

    #endregion
    #region Test methods

    [Fact]
    public void GetTags_ReturnsOkResult_WithListOfTags()
    {
        var tags = new List<TagModel>
        {
            new TagModel { Id = 1, Name = "C#", Count = 100, Percentage = 50 },
            new TagModel { Id = 2, Name = "ASP.NET", Count = 50, Percentage = 25 }
        };

        _mockTagService.Setup(s => s.GetTags(1, 10, SortBy.NameAscending)).Returns(tags);

        var result = _controller.GetTags();

        var res = Assert.IsType<OkObjectResult>(result);
        var resTags = Assert.IsType<List<TagModel>>(res.Value);
        Assert.Equal(2, resTags.Count);
    }

    [Fact]
    public void GetTags_ReturnsEmptyList_WhenNoTags()
    {
        _mockTagService.Setup(s => s.GetTags(1, 10, SortBy.NameAscending)).Returns(new List<TagModel>());

        var result = _controller.GetTags(1, 10, SortBy.NameAscending) as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        var returnedTags = Assert.IsType<List<TagModel>>(result.Value);
        Assert.Empty(returnedTags);
    }

    [Fact]
    public async Task RefreshTags_ReturnsOkResult()
    {
        var tags = new List<TagModel>
        {
            new TagModel { Name = "C#", Count = 100, Percentage = 50 },
            new TagModel { Name = "ASP.NET", Count = 50, Percentage = 25 }
        };

        _mockStackOverflowTagService.Setup(s => s.GetTagsAsyncTask()).ReturnsAsync(tags);
        _mockTagService.Setup(s => s.InitializeTagsAsync(tags)).Returns(Task.CompletedTask);

        var result = await _controller.RefreshTags();

        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task RefreshTags_ReturnsInternalServerError_OnException()
    {
        _mockStackOverflowTagService.Setup(s => s.GetTagsAsyncTask()).ThrowsAsync(new System.Exception("B³¹d API"));

        var result = await _controller.RefreshTags() as ObjectResult;

        Assert.NotNull(result);
        Assert.Equal(500, result.StatusCode);
        Assert.Equal("B³¹d API", result.Value);
    }

    [Theory]
    [InlineData(SortBy.NameAscending, "ASP.NET")]
    [InlineData(SortBy.NameDescending, "C#")]
    public void GetTags_UsesCorrectSorting(SortBy sortBy, string expectedFirstTag)
    {
        var tags = new List<TagModel>
        {
            new TagModel { Name = "C#", Count = 100, Percentage = 50 },
            new TagModel { Name = "ASP.NET", Count = 50, Percentage = 25 }
        };

        _mockTagService
            .Setup(s => s.GetTags(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<SortBy>()))
            .Returns((int page, int pageSize, SortBy sort) =>
            {
                return sort switch
                {
                    SortBy.NameAscending => tags.OrderBy(t => t.Name).ToList(),
                    SortBy.NameDescending => tags.OrderByDescending(t => t.Name).ToList(),
                    _ => tags
                };
            });

        var result = _controller.GetTags(1, 10, sortBy) as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        var returnedTags = Assert.IsType<List<TagModel>>(result.Value);
        Assert.Equal(expectedFirstTag, returnedTags[0].Name);
    }

    #endregion
}