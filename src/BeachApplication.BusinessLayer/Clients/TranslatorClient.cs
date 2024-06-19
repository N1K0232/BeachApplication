﻿using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using BeachApplication.BusinessLayer.Clients.Interfaces;
using BeachApplication.BusinessLayer.Exceptions;
using BeachApplication.BusinessLayer.Providers.Interfaces;
using BeachApplication.BusinessLayer.Resources;
using BeachApplication.BusinessLayer.Settings;
using BeachApplication.Shared.Models.Responses;
using BeachApplication.Shared.Models.TranslationClient;
using Microsoft.Extensions.Options;
using TinyHelpers.Extensions;

namespace BeachApplication.BusinessLayer.Clients;

public class TranslatorClient : ITranslatorClient
{
    private const int MaxArrayLengthForTranslation = 25;
    private const int MaxTextLengthForTranslation = 5000;
    private const int MaxArrayLengthForDetection = 100;
    private const int MaxTextLengthForDetection = 10000;

    private readonly HttpClient httpClient;
    private readonly IAzureTokenProvider azureTokenProvider;

    private CancellationTokenSource cancellationTokenSource;
    private bool disposed = false;
    private string authorizationHeaderValue;

    public TranslatorClient(HttpClient httpClient, IAzureTokenProvider azureTokenProvider, IOptions<TranslatorSettings> translatorSettingsOptions)
    {
        this.httpClient = httpClient;
        this.azureTokenProvider = azureTokenProvider;

        Initialize(translatorSettingsOptions.Value);
    }

    public string SubscriptionKey
    {
        get => azureTokenProvider.SubscriptionKey;
        set => azureTokenProvider.SubscriptionKey = value;
    }

    public string Region
    {
        get => azureTokenProvider.Region;
        set => azureTokenProvider.Region = value;
    }

    public string Language { get; set; }

    public async Task<DetectedLanguageResponse> DetectLanguageAsync(string input)
    {
        ThrowIfDisposed();

        cancellationTokenSource = new CancellationTokenSource();
        var languages = await DetectLanguagesCoreAsync([input], cancellationTokenSource.Token);
        return languages.FirstOrDefault();
    }

    public async Task<IEnumerable<DetectedLanguageResponse>> DetectLanguagesAsync(IEnumerable<string> input)
    {
        ThrowIfDisposed();

        cancellationTokenSource = new CancellationTokenSource();
        return await DetectLanguagesCoreAsync(input, cancellationTokenSource.Token);
    }

    private async Task<IEnumerable<DetectedLanguageResponse>> DetectLanguagesCoreAsync(IEnumerable<string> input, CancellationToken cancellationToken)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(input, nameof(input));

        if (!input.Any())
        {
            throw new ArgumentException($"{nameof(input)} array must contain at least 1 element");
        }

        if (input.Count() > MaxArrayLengthForDetection)
        {
            throw new ArgumentException($"{nameof(input)} array can have at most {MaxArrayLengthForDetection} elements");
        }

        await CheckUpdateTokenAsync();
        var uriString = $"{ClientValues.BaseUrl}detect?{ClientValues.ApiVersion}";

        using var request = CreateHttpRequest(uriString, HttpMethod.Post, input.Select(t => new
        {
            Text = t.Substring(0, Math.Min(t.Length, MaxTextLengthForDetection))
        }));

        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<IEnumerable<DetectedLanguageResponse>>(cancellationToken);
        }

        throw await HttpClientException.ReadFromResponseAsync(response);
    }

    public async Task<IEnumerable<ServiceLanguage>> GetLanguagesAsync(string language = null)
    {
        ThrowIfDisposed();

        cancellationTokenSource = new CancellationTokenSource();
        await CheckUpdateTokenAsync();

        var uriString = $"{ClientValues.BaseUrl}languages?scope=translation&{ClientValues.ApiVersion}";
        using var request = CreateHttpRequest(uriString);

        var requestLanguage = language.GetValueOrDefault(Language);
        if (requestLanguage.HasValue())
        {
            // If necessary, adds the Accept-Language header in order to get localized language names.
            request.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue(language));
        }

        using var response = await httpClient.SendAsync(request, cancellationTokenSource.Token);
        if (response.IsSuccessStatusCode)
        {
            using var contentStream = await response.Content.ReadAsStreamAsync(cancellationTokenSource.Token);
            using var jsonDocument = await JsonDocument.ParseAsync(contentStream, cancellationToken: cancellationTokenSource.Token);

            var jsonContent = jsonDocument.RootElement.GetProperty("translation");
            var responseContent = JsonSerializer.Deserialize<Dictionary<string, ServiceLanguage>>(jsonContent.ToString()).ToList();

            responseContent.ForEach(r => r.Value.Code = r.Key);
            return responseContent.Select(r => r.Value).OrderBy(r => r.Name).ToList();
        }

        throw await HttpClientException.ReadFromResponseAsync(response);
    }

    public async Task<TranslationResponse> TranslateAsync(string input, IEnumerable<string> to)
    {
        ThrowIfDisposed();

        cancellationTokenSource = new CancellationTokenSource();
        return (await TranslateCoreAsync([input], null, to, cancellationTokenSource.Token)).FirstOrDefault();
    }

    public async Task<TranslationResponse> TranslateAsync(string input, string from, IEnumerable<string> to = null)
    {
        ThrowIfDisposed();

        cancellationTokenSource = new CancellationTokenSource();
        return (await TranslateCoreAsync([input], from, to, cancellationTokenSource.Token)).FirstOrDefault();
    }

    public async Task<IEnumerable<TranslationResponse>> TranslateAsync(IEnumerable<string> input, string from, IEnumerable<string> to)
    {
        ThrowIfDisposed();

        cancellationTokenSource = new CancellationTokenSource();
        return await TranslateCoreAsync(input, from, to, cancellationTokenSource.Token);
    }

    private async Task<IEnumerable<TranslationResponse>> TranslateCoreAsync(IEnumerable<string> input, string from, IEnumerable<string> to, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input, nameof(input));

        if (input.Count() > MaxArrayLengthForTranslation)
        {
            throw new ArgumentException($"{nameof(input)} array can have at most {MaxArrayLengthForTranslation} elements");
        }

        if (input.Any(str => string.IsNullOrWhiteSpace(str) || str.Length > MaxTextLengthForTranslation))
        {
            throw new ArgumentException($"Each sentence cannot be null or longer than {MaxTextLengthForTranslation} characters");
        }

        if (to == null || !to.Any())
        {
            to = [Language];
        }

        await CheckUpdateTokenAsync();

        var toQueryString = string.Join("&", to.Select(t => $"to={t}"));
        var uriString = (string.IsNullOrWhiteSpace(from) ? $"{ClientValues.BaseUrl}translate?{toQueryString}" : $"{ClientValues.BaseUrl}translate?from={from}&{toQueryString}") + $"&{ClientValues.ApiVersion}";

        using var request = CreateHttpRequest(uriString, HttpMethod.Post, input.Select(t => new { Text = t }));
        using var response = await httpClient.SendAsync(request, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<IEnumerable<TranslationResponse>>(cancellationToken: cancellationToken);
        }

        throw await HttpClientException.ReadFromResponseAsync(response);
    }

    private async Task CheckUpdateTokenAsync()
    {
        authorizationHeaderValue = await azureTokenProvider.GetAccessTokenAsync();
    }

    private void Initialize(TranslatorSettings translatorSettings)
    {
        SubscriptionKey = translatorSettings.SubscriptionKey;
        Region = translatorSettings.Region;
        Language = translatorSettings.Language ?? Thread.CurrentThread.CurrentUICulture.Name.ToLowerInvariant();
    }

    private HttpRequestMessage CreateHttpRequest(string uriString) => CreateHttpRequest(uriString, HttpMethod.Get);

    private HttpRequestMessage CreateHttpRequest(string uriString, HttpMethod method, object content = null)
    {
        var request = new HttpRequestMessage(method, new Uri(uriString))
        {
            Content = content != null ? JsonContent.Create(content, content.GetType()) : null
        };

        request.Headers.Add(ClientValues.AuthorizationHeader, authorizationHeaderValue);
        return request;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!disposed && disposing)
        {
            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;

            disposed = true;
        }
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(disposed, GetType().FullName);
    }
}