namespace KodingTask.Conversion;

public enum ConversionError
{
    Empty,
    TooLong,
    InvalidCharacter,
    NegativeAmount,
    DecimalPointNotAllowed,
    MultipleCommas,
    MissingDollars,
    InvalidCents,
    TooManyCentDigits,
    InvalidGrouping,
    DollarsOutOfRange,
    UnsupportedLanguage,
}
