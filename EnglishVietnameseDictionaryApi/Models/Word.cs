namespace EnglishVietnameseDictionaryApi.Models
{
    public class Word
    {
        public int Id { get; set; }
        public string EnglishText { get; set; } = string.Empty;
        public string? Phonetic { get; set; }
        public string? Audio { get; set; }
        public ICollection<Meaning> Meanings { get; set; } = [];

    }
}
