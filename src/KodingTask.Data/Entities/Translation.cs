namespace KodingTask.Data.Entities;

public class Translation
{
    public int Id { get; set; }

    public int TermId { get; set; }

    public Term? Term { get; set; }

    public required string Locale { get; set; }

    public required string Text { get; set; }

    public TranslationSource Source { get; set; }

    public bool Reviewed { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}

public enum TranslationSource
{
    Baked,
    Machine,
    Manual,
}
