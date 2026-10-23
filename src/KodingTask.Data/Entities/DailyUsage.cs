namespace KodingTask.Data.Entities;

public class DailyUsage
{
    public DateOnly Day { get; set; }

    public required string Counter { get; set; }

    public int Count { get; set; }
}
