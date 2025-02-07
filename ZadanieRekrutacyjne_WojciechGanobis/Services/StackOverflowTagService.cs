using Azure;
using System.Text.Json;
using System.Text.Json.Serialization;
using ZadanieRekrutacyjne_WojciechGanobis.Models;
using ZadanieRekrutacyjne_WojciechGanobis.Interfaces;

namespace ZadanieRekrutacyjne_WojciechGanobis.Services;

public class StackOverflowTagService : IStackOverflowTagService
{
    #region Const

    private const string ERROR_HTTP_REQUEST_MESSAGE = "Błąd podczas pobierania danych z API StackOverflow.";
    private const string ERROR_JSON_MESSAGE = "Błąd podczas deserializacji danych.";
    private const string ERROR_CUSTOM_MESSAGE = "Wystąpił nieoczekiwany błąd.";
    private const string WARNING_EMPTY_DATA_MESSAGE = "Pobrane dane są puste lub nieprawidłowe.";

    #endregion
    #region Private class

    private class StackOverflowResponse
    {
        [JsonPropertyName("items")]
        public List<StackOverflowTag>? Items { get; set; }
    }

    private class StackOverflowTag
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }
    }

    #endregion
    #region Private fields

    private readonly HttpClient _httpClient;
    private readonly ILogger<StackOverflowTagService> _logger;

    #endregion
    #region Constructors

    public StackOverflowTagService(HttpClient httpClient, ILogger<StackOverflowTagService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    #endregion
    #region Public methods

    public async Task<List<TagModel>> GetTagsAsyncTask()
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get,
                "https://api.stackexchange.com/tags?order=desc&sort=popular&site=stackoverflow");

            request.Headers.UserAgent.ParseAdd("GanobisApp/1.0");

            var res = await _httpClient.SendAsync(request);
            res.EnsureSuccessStatusCode();

            var content = await res.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<StackOverflowResponse>(content);

            if (result == null || result.Items == null || !result.Items.Any())
            {
                _logger.LogWarning(WARNING_EMPTY_DATA_MESSAGE);
                return new List<TagModel>();
            }

            return countTagModels(result);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, ERROR_HTTP_REQUEST_MESSAGE);
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, ERROR_JSON_MESSAGE);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ERROR_CUSTOM_MESSAGE);
            throw;
        }
    }

    #endregion
    #region Private methods

    private static List<TagModel> countTagModels(StackOverflowResponse result)
    {
        if (result.Items != null)
        {
            var totalCount = result.Items.Sum(t => t.Count);
            var tags = result.Items.Select(t => new TagModel
            {
                Name = t.Name,
                Count = t.Count,
                Percentage = (double)t.Count / totalCount * 100
            }).ToList();

            return tags;
        }
        return new List<TagModel>();
    }
    #endregion
}