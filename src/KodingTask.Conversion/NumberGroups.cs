namespace KodingTask.Conversion;

internal static class NumberGroups
{
    public static (int Millions, int Thousands, int Units) Split(int number) =>
        (number / 1_000_000, number / 1_000 % 1_000, number % 1_000);
}
