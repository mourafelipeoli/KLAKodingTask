namespace KodingTask.Data.Entities;

public class Term
{
    public int Id { get; set; }

    public required string Text { get; set; }

    public required string TextHash { get; set; }

    public TermOrigin Origin { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public List<Translation> Translations { get; set; } = [];
}

public enum TermOrigin
{
    Code,
    Visitor,
    Server,
}
