namespace EnglishVietnameseDictionaryApi.Services;

public interface ITranslator
{
    Task<string> TranslateAsync(string originalText, Language from, Language to);
}

public enum Language
{
    English,
    Vietnamese
}