namespace EnglishVietnameseDictionaryApi.Models
{
    public class Example
    {
        public int Id { get; set; }
        public int MeaningId { get; set; }
        public Meaning? Meaning { get; set; }
        public string Text { get; set; } = string.Empty;
        public string? TranslatedText { get; set; }
    }
}
