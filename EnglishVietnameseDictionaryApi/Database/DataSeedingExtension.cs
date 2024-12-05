using EnglishVietnameseDictionaryApi.Models;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;

namespace EnglishVietnameseDictionaryApi.Database;

public static class DataSeedingExtension
{
    public static async Task SeedDatabaseAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        context.Database.EnsureCreated();

        string filePath = @"words_alpha.txt";

        if (!context.Words.Any())
        {
            using (var reader = new StreamReader(filePath))
            {
                while (!reader.EndOfStream)
                {
                    var word = await reader.ReadLineAsync();

                    if (string.IsNullOrEmpty(word))
                    {
                        continue;
                    }

                    context.Words.Add(new Models.Word()
                    {
                        EnglishText = word
                    });
                }
            }

            context.SaveChanges();
        }

        if (!context.Meanings.Any())
        {
            var i = 0;

            foreach (var word in context.Words)
            {
                if (!context.Meanings.Where(x => x.WordId == word.Id).Any())
                {
                    Console.WriteLine($"Adding {word.EnglishText}...");
                    await CrawlTraTuAsync(word, context);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"{word.EnglishText} added");
                    Console.ForegroundColor = ConsoleColor.White;

                    if (i > 0 && i % 100 == 0)
                    {
                        context.SaveChanges();
                    }

                    i++;
                }
            }

        }

        var removeWords = context.Words.Include(x => x.Meanings)
            .Where(x => !x.Meanings.Any()).ToList();

        context.Words.RemoveRange(removeWords);
        context.SaveChanges();

        foreach (var w in context.Words)
        {
            if (string.IsNullOrEmpty(w.Audio) && string.IsNullOrEmpty(w.Phonetic))
            {
                Console.WriteLine($"Adding audio for {w.EnglishText}...");

                var (audio, phonetic) = await GetAudioUrlAndPhoneticAsync(w.EnglishText);

                if (string.IsNullOrEmpty(audio))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Audio was not found for {w.EnglishText}");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    w.Audio = audio;

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Audio was added for {w.EnglishText}");
                    Console.ForegroundColor = ConsoleColor.White;
                }

                if (string.IsNullOrEmpty(phonetic))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Phonetic was not found for {w.EnglishText}");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else
                {
                    w.Phonetic = phonetic;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Phonetic was added for {w.EnglishText}");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }

        }
        context.SaveChanges();
    }
    private static string TransformTitleCase(string text)
    {
        return text.Substring(0, 1).ToUpper() + text.Substring(1).ToLower();
    }

    private static async Task<string> GetHtmlAsync(string url)
    {
        try
        {
            var httpClient = new HttpClient();
            return await httpClient.GetStringAsync(url);
        }
        catch (Exception)
        {
            return string.Empty;
        }

    }

    private static async Task<(string, string)> GetAudioUrlAndPhoneticAsync(string word)
    {
        var url = $"https://dictionary.cambridge.org/pronunciation/english/{word}";
        var html = await GetHtmlAsync(url);
        if (string.IsNullOrEmpty(html))
        {
            return (string.Empty, string.Empty);
        }

        var document = new HtmlDocument();
        document.LoadHtml(html);

        var audioPhoneticElements = document.DocumentNode
                          .Descendants()
                          .Where(n => n.GetAttributeValue("class", "") == "pron-info")
                          .ToList();

        if (!audioPhoneticElements.Any())
        {
            return (string.Empty, string.Empty);
        }

        var audioElements = audioPhoneticElements[0]
                          .Descendants()
                          .Where(n => n.GetAttributeValue("class", "") == "soundfile")
                          .SelectMany(x => x.Descendants("source"))
                          .ToList();

        var audioUrl = string.Empty;
        if (audioElements.Count > 0)
        {
            audioUrl = "https://dictionary.cambridge.org" + audioElements[0].GetAttributeValue("src", "");
        }


        var phoneticElements = audioPhoneticElements[0]
                          .Descendants("span")
                          .Where(n => n.GetAttributeValue("class", "") == "ipa")
                          .ToList();

        var phonetic = string.Empty;
        if (phoneticElements.Count > 0)
        {
            phonetic = $"/{phoneticElements[0].InnerText}/";
        }

        return (audioUrl, phonetic);
    }

    private static async Task CrawlTraTuAsync(Word word, AppDbContext context)
    {
        var text = TransformTitleCase(word.EnglishText);

        var url = $"http://tratu.soha.vn/dict/en_vn/{text}";
        var html = await GetHtmlAsync(url);
        if (string.IsNullOrEmpty(html))
        {
            return;
        }

        var document = new HtmlDocument();
        document.LoadHtml(html);

        var elements = document.DocumentNode
                          .Descendants()
                          .Where(n => n.GetAttributeValue("id", "") == "content-3")
                          .ToList();

        if (elements.Count == 0)
        {

            context.Words.Remove(word);
            context.SaveChanges();

            return;
        }

        foreach (var element in elements)
        {
            var partOfSpeechTexts = element.Descendants("h3")
                .SelectMany(x => x.Descendants("span"))
                .Select(x => x.InnerHtml).ToList();

            if (!partOfSpeechTexts.Any())
            {
                continue;
            }

            var partOfSpeech = GetPartOfSpeech(partOfSpeechTexts[0]);

            if (partOfSpeech == null)
            {
                continue;
            }

            var meaningElements = element.Descendants("div")
                .Where(x => x.GetAttributeValue("id", "") == "content-5")
                .ToList();

            if (!meaningElements.Any())
            {
                continue;
            }

            foreach (var meaningElement in meaningElements)
            {
                var spanElements = meaningElement.Descendants("span").ToList();

                if (!spanElements.Any())
                {
                    continue;
                }

                var translatedDefinition = spanElements[0].InnerHtml;

                var meaning = new Models.Meaning()
                {
                    VietnameseText = translatedDefinition,
                    PartOfSpeech = partOfSpeech.Value,
                    WordId = word.Id
                };

                context.Meanings.Add(meaning);

                var exampleElements = meaningElement.Descendants("dl")
                    .ToList();

                if (!exampleElements.Any())
                {
                    continue;
                }

                exampleElements = exampleElements[0].Descendants("dl").SelectMany(x => x.Descendants("dd"))
                    .ToList();

                var exampleOrigin = string.Empty;
                var exampleTranslated = string.Empty;

                for (var i = 0; i < exampleElements.Count; i++)
                {
                    if (i % 2 == 0)
                    {
                        var exampleDoc = new HtmlDocument();
                        exampleDoc.LoadHtml(exampleElements[i].InnerHtml);
                        exampleOrigin = exampleDoc.DocumentNode.InnerText.Trim();
                    }
                    else
                    {
                        exampleTranslated = exampleElements[i].InnerHtml.Trim();
                        context.Examples.Add(new Models.Example()
                        {
                            Text = exampleOrigin,
                            TranslatedText = exampleTranslated,
                            Meaning = meaning
                        });
                    }
                }
            }
        }



    }

    private static PartOfSpeech? GetPartOfSpeech(string partOfSpeechText)
    {
        PartOfSpeech? partOfSpeech = null;

        if (partOfSpeechText.ToLower().Contains("danh từ"))
        {
            partOfSpeech = PartOfSpeech.Noun;
        }
        else if (partOfSpeechText.ToLower().Contains("động từ"))
        {
            partOfSpeech = PartOfSpeech.Verb;
        }
        else if (partOfSpeechText.ToLower().Contains("tính từ"))
        {
            partOfSpeech = PartOfSpeech.Adjective;
        }
        else if (partOfSpeechText.ToLower().Contains("phó từ"))
        {
            partOfSpeech = PartOfSpeech.Adverb;
        }
        else if (partOfSpeechText.ToLower().Contains("đại từ"))
        {
            partOfSpeech = PartOfSpeech.Pronoun;
        }
        else if (partOfSpeechText.ToLower().Contains("giới từ"))
        {
            partOfSpeech = PartOfSpeech.Preposition;
        }
        else if (partOfSpeechText.ToLower().Contains("liên từ"))
        {
            partOfSpeech = PartOfSpeech.Conjunction;
        }
        else if (partOfSpeechText.ToLower().Contains("thán từ"))
        {
            partOfSpeech = PartOfSpeech.Interjection;
        }

        return partOfSpeech;
    }
}

