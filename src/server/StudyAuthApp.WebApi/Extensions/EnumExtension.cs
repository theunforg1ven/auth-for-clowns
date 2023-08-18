namespace StudyAuthApp.WebApi.Extensions;

public static class EnumExtension
{
    public static TEnum ParseEnum<TEnum>(this string value, bool ignoreCase = true) where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(value));

        try
        {
            return (TEnum)Enum.Parse(typeof(TEnum), value, ignoreCase);
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Error parsing '{value}' to enum {typeof(TEnum).Name}.", ex);
        }
    }
}
