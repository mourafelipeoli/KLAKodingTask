namespace KodingTask.Conversion;

internal static class NumberGroups
{
    public static (int Millions, int Thousands, int Units) Split(int number) =>
        (number / 1_000_000, number / 1_000 % 1_000, number % 1_000);

    public static (int HundredMillions, int TenThousands, int Units) SplitMyriads(int number) =>
        (number / 100_000_000, number / 10_000 % 10_000, number % 10_000);

    public static (int Thousands, int Hundreds, int Tens, int Ones) Digits(int belowTenThousand) =>
        (belowTenThousand / 1_000, belowTenThousand / 100 % 10, belowTenThousand / 10 % 10, belowTenThousand % 10);
}
