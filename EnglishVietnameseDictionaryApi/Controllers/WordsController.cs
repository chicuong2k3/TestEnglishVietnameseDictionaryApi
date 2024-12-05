using EnglishVietnameseDictionaryApi.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EnglishVietnameseDictionaryApi.Controllers;

[ApiController]
public class WordsController : ControllerBase
{
    private readonly AppDbContext dbContext;

    public WordsController(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    [HttpGet("api/words")]
    public async Task<IActionResult> SearchWords([FromQuery] SearchWordsRequest request)
    {
        var query = dbContext.Words.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            query = query.Where(w => w.EnglishText.Contains(request.Query));
        }

        if (!string.IsNullOrWhiteSpace(request.OrderDirection))
        {
            query = request.OrderDirection.ToLower() switch
            {
                "asc" => query.OrderBy(w => w.EnglishText),
                "desc" => query.OrderByDescending(w => w.EnglishText),
                _ => throw new ArgumentException("Invalid order direction.")
            };
        }

        var words = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return Ok(words);
    }

    [HttpGet("api/words/{word}")]
    public async Task<IActionResult> GetWord(string word)
    {
        var wordEntity = await dbContext.Words
            .FirstOrDefaultAsync(w => w.EnglishText == word.ToLower());

        if (wordEntity == null)
        {
            return NotFound();
        }

        var result = await dbContext.Words
            .Include(w => w.Meanings)
            .ThenInclude(m => m.Examples)
            .Where(w => w.EnglishText == word.ToLower())
            .Select(w => new WordDto(
                w.EnglishText,
                w.Phonetic,
                w.Audio,
                w.Meanings.Select(m => new MeaningDto(
                    m.VietnameseText,
                    m.PartOfSpeech.ToString(),
                    m.Examples.Select(e => new ExampleDto(
                        e.Text,
                        e.TranslatedText
                    ))
                ))
            )).FirstOrDefaultAsync();


        return Ok(result);
    }

}

public record SearchWordsRequest(
    int PageNumber = 1,
    int PageSize = 10,
    string? Query = null,
    string? OrderDirection = "asc"
);

public record WordDto(
    string EnglishText,
    string? Phonetic,
    string? Audio,
    IEnumerable<MeaningDto> Meanings
);

public record MeaningDto(
    string Meaning,
    string PartOfSpeech,
    IEnumerable<ExampleDto> Examples
);

public record ExampleDto(
    string Example,
    string? Translation
);