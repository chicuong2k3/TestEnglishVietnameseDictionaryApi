
namespace EnglishVietnameseDictionaryApi.Services;

public class Translator : ITranslator
{
    private readonly IConfiguration configuration;

    public Translator(IConfiguration configuration)
    {
        this.configuration = configuration;
    }
    public async Task<string> TranslateAsync(string originalText, Language from, Language to)
    {
        var key = configuration["MyMemory:ApiKey"] ?? throw new ArgumentNullException("MyMemory:ApiKey");

        var languageFrom = from switch
        {
            Language.English => "en",
            Language.Vietnamese => "vi",
            _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
        };

        var languageTo = to switch
        {
            Language.English => "en",
            Language.Vietnamese => "vi",
            _ => throw new ArgumentOutOfRangeException(nameof(to), to, null)
        };

        try
        {
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync($"https://api.mymemory.translated.net/get?q={originalText}&langpair={languageFrom}|{languageTo}&key={key}");

            if (!response.IsSuccessStatusCode)
            {
                return string.Empty;

            }

            var content = await response.Content.ReadFromJsonAsync<TranslateResponse>();
            return content?.ResponseData?.TranslatedText ?? string.Empty;
        }
        catch (Exception)
        {
            return string.Empty;
        }

    }
}

public class TranslateResponse
{
    public ResponseData ResponseData { get; set; } = default!;
}

public class ResponseData
{
    public string TranslatedText { get; set; } = string.Empty;
}
