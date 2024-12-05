namespace EnglishVietnameseDictionaryApi.Models
{
    public class Meaning
    {
        public int Id { get; set; }
        public string VietnameseText { get; set; } = string.Empty;
        public PartOfSpeech PartOfSpeech { get; set; }
        public ICollection<Example> Examples { get; set; } = [];
        public int WordId { get; set; }
        public Word? Word { get; set; }
    }
}
