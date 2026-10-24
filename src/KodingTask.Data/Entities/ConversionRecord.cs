namespace KodingTask.Data.Entities;

public class ConversionRecord
{
    public long Id { get; set; }

    public required string UserId { get; set; }

    public required string Amount { get; set; }

    public int Dollars { get; set; }

    public int Cents { get; set; }

    public required string LanguageCode { get; set; }

    public required string Words { get; set; }

    public DateTime ConvertedAtUtc { get; set; }
}
