using ZadanieRekrutacyjne_WojciechGanobis.Data;
using ZadanieRekrutacyjne_WojciechGanobis.Interfaces;
using ZadanieRekrutacyjne_WojciechGanobis.Models;
using ZadanieRekrutacyjne_WojciechGanobis.Models.Enums;

namespace ZadanieRekrutacyjne_WojciechGanobis.Services;

public class TagService : ITagService
{
    #region Private fields

    private readonly ApplicationDbContext _context;

    #endregion
    #region Constructors

    public TagService(ApplicationDbContext context)
    {
        _context = context;
    }

    #endregion
    #region Public methods

    public async Task InitializeTagsAsync(List<TagModel> tags)
    {
        _context.Tags.RemoveRange(_context.Tags);
        await _context.Tags.AddRangeAsync(tags);
        await _context.SaveChangesAsync();
    }

    public List<TagModel> GetTags(int page, int pageSize, SortBy sortBy)
    {
        var tags = _context.Tags.ToList();
        switch (sortBy)
        {
            case SortBy.NameAscending:
                tags = tags.OrderBy(t => t.Name).ToList();
                break;

            case SortBy.NameDescending:
                tags = tags.OrderByDescending(t => t.Name).ToList();
                break;

            case SortBy.PercentageAscending:
                tags = tags.OrderBy(t => t.Percentage).ToList();
                break;

            case SortBy.PercentageDescending:
                tags = tags.OrderByDescending(t => t.Percentage).ToList();
                break;

            default:
                tags = tags.OrderBy(t => t.Name).ToList();
                break;
        }
        return tags.Skip((page - 1) * pageSize).Take(pageSize).ToList();
    }

    #endregion
}