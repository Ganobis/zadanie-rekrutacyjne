using ZadanieRekrutacyjne_WojciechGanobis.Models;
using ZadanieRekrutacyjne_WojciechGanobis.Models.Enums;

namespace ZadanieRekrutacyjne_WojciechGanobis.Interfaces;

public interface ITagService
{
    Task InitializeTagsAsync(List<TagModel> tags);
    List<TagModel> GetTags(int page, int pageSize, SortBy sortBy);
}