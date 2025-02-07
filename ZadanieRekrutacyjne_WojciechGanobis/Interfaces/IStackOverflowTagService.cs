using ZadanieRekrutacyjne_WojciechGanobis.Models;

namespace ZadanieRekrutacyjne_WojciechGanobis.Interfaces;

public interface IStackOverflowTagService
{
    Task<List<TagModel>> GetTagsAsyncTask();
}