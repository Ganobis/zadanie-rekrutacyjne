using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using ZadanieRekrutacyjne_WojciechGanobis.Services;

namespace ServicesTests;

public class StackOverflowTagServiceTests
{
    #region Private class

    private class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly string _response;

        public MockHttpMessageHandler(string response)
        {
            _response = response;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(_response, Encoding.UTF8, "application/json")
            });
        }
    }

    #endregion
    #region Private fields

    private readonly Mock<ILogger<StackOverflowTagService>> _loggerMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly StackOverflowTagService _service;

    #endregion
    #region Contructors

    public StackOverflowTagServiceTests()
    {
        _loggerMock = new Mock<ILogger<StackOverflowTagService>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _service = new StackOverflowTagService(_httpClient, _loggerMock.Object);
    }

    #endregion
    #region Test methods

    [Fact]
    public async Task GetTagsAsyncTask_ReturnsValidTagList()
    {
        var jsonResponse = "{\"items\": [{\"name\": \"c#\", \"count\": 500}, {\"name\": \"asp.net\", \"count\": 300}]}";
        SetupHttpResponse(jsonResponse, HttpStatusCode.OK);

        var result = await _service.GetTagsAsyncTask();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, tag => tag.Name == "c#" && tag.Count == 500);
        Assert.Contains(result, tag => tag.Name == "asp.net" && tag.Count == 300);
    }

    [Fact]
    public async Task GetTagsAsyncTask_ShouldReturnTags()
    {
        // Arrange
        var httpClient = new HttpClient(new MockHttpMessageHandler(
            "{\"items\":[{\"name\":\"c#\",\"count\":100},{\"name\":\"javascript\",\"count\":200}]}"));
        var logger = new Mock<ILogger<StackOverflowTagService>>();
        var service = new StackOverflowTagService(httpClient, logger.Object);

        // Act
        var tags = await service.GetTagsAsyncTask();

        // Assert
        Assert.NotNull(tags);
        Assert.Equal(2, tags.Count);
        Assert.Equal("c#", tags[0].Name);
        Assert.Equal(100, tags[0].Count);
        Assert.Equal(33.33, tags[0].Percentage, 2);
        Assert.Equal("javascript", tags[1].Name);
        Assert.Equal(200, tags[1].Count);
        Assert.Equal(66.67, tags[1].Percentage, 2);
    }

    [Fact]
    public async Task GetTagsAsyncTask_HandlesEmptyResponse()
    {
        SetupHttpResponse("{\"items\": []}", HttpStatusCode.OK);

        var result = await _service.GetTagsAsyncTask();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetTagsAsyncTask_HandlesInvalidJson()
    {
        SetupHttpResponse("invalid json", HttpStatusCode.OK);

        await Assert.ThrowsAsync<JsonException>(() => _service.GetTagsAsyncTask());
    }

    [Fact]
    public async Task GetTagsAsyncTask_HandlesHttpRequestException()
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        await Assert.ThrowsAsync<HttpRequestException>(() => _service.GetTagsAsyncTask());
    }

    [Fact]
    public async Task GetTagsAsyncTask_HandlesNonSuccessStatusCode()
    {
        SetupHttpResponse("", HttpStatusCode.BadRequest);

        await Assert.ThrowsAsync<HttpRequestException>(() => _service.GetTagsAsyncTask());
    }

    #endregion
    #region Private methods

    private void SetupHttpResponse(string content, HttpStatusCode statusCode)
    {
        var response = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(content)
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);
    }
    #endregion
}