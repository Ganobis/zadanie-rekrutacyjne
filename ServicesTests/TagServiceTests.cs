using Microsoft.EntityFrameworkCore;
using ZadanieRekrutacyjne_WojciechGanobis.Data;
using ZadanieRekrutacyjne_WojciechGanobis.Models;
using ZadanieRekrutacyjne_WojciechGanobis.Models.Enums;
using ZadanieRekrutacyjne_WojciechGanobis.Services;

namespace ServicesTests;

public class TagServiceTests
{
    #region Private fields

    private readonly ApplicationDbContext _context;
    private readonly TagService _service;

    #endregion
    #region Contructors

    public TagServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        _context = new ApplicationDbContext(options);
        _service = new TagService(_context);
    }

    #endregion
    #region Test methods

    [Fact]
    public async Task InitializeTagsAsync_ShouldRemoveExistingAndAddNewTags()
    {
        ClearDatabase();

        _context.Tags.Add(new TagModel { Name = "OldTag", Count = 100, Percentage = 10 });
        await _context.SaveChangesAsync();

        var newTags = new List<TagModel>
        {
            new TagModel { Name = "C#", Count = 500, Percentage = 50 },
            new TagModel { Name = "ASP.NET", Count = 300, Percentage = 30 }
        };

        await _service.InitializeTagsAsync(newTags);
        var tags = _context.Tags.ToList();

        Assert.Equal(2, tags.Count);
        Assert.Contains(tags, t => t.Name == "C#" && t.Count == 500);
        Assert.Contains(tags, t => t.Name == "ASP.NET" && t.Count == 300);
    }

    [Fact]
    public void GetTags_ShouldReturnPagedAndSortedTags()
    {
        ClearDatabase();

        _context.Tags.AddRange(new List<TagModel>
        {
            new TagModel { Name = "ASP.NET", Count = 400, Percentage = 40 },
            new TagModel { Name = "JavaScript", Count = 200, Percentage = 20 },
            new TagModel { Name = "C#", Count = 100, Percentage = 10 }
        });
        _context.SaveChanges();

        var sortedTags = _service.GetTags(1, 2, SortBy.NameAscending);

        Assert.Equal(2, sortedTags.Count);
        Assert.Equal("ASP.NET", sortedTags[0].Name);
        Assert.Equal("C#", sortedTags[1].Name);
    }

    [Fact]
    public void GetTags_ShouldHandlePaginationCorrectly()
    {
        ClearDatabase();

        _context.Tags.AddRange(new List<TagModel>
        {
            new TagModel { Name = "ASP.NET", Count = 150, Percentage = 15 },
            new TagModel { Name = "C#", Count = 250, Percentage = 25 },
            new TagModel { Name = "JavaScript", Count = 350, Percentage = 35 }
        });
        _context.SaveChanges();

        var pagedTags = _service.GetTags(2, 1, SortBy.NameAscending);

        Assert.Single(pagedTags);
        Assert.Equal("C#", pagedTags[0].Name);
    }

    #endregion
    #region Private methods

    private void ClearDatabase()
    {
        _context.Tags.RemoveRange(_context.Tags);
        _context.SaveChanges();
    }

    #endregion
}