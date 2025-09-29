using System;

public class AllowedKeyFilter
{
    private const int DEFAULT_HEX_MINIMUM_LENGTH = 32;
    private const StringComparison PREFIX_COMPARISON = StringComparison.OrdinalIgnoreCase;
    
    private readonly string[] allowedKeyPrefixes;

    public AllowedKeyFilter(string[] allowedKeyPrefixes)
    {
        this.allowedKeyPrefixes = allowedKeyPrefixes ?? Array.Empty<string>();
    }

    public bool IsAllowed(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            return false;

        string lowerCaseKey = key.ToLowerInvariant();

        for (int prefixIndex = 0; prefixIndex < allowedKeyPrefixes.Length; prefixIndex++)
        {
            string allowedPrefix = allowedKeyPrefixes[prefixIndex];

            if (!string.IsNullOrEmpty(allowedPrefix) && lowerCaseKey.StartsWith(allowedPrefix, PREFIX_COMPARISON))
                return true;
        }

        return false;
    }
    
    public bool IsHexLike(string input, int minimumLength = DEFAULT_HEX_MINIMUM_LENGTH)
    {
        if (string.IsNullOrEmpty(input) || input.Length < minimumLength)
            return false;

        for (int charIndex = 0; charIndex < input.Length; charIndex++)
        {
            char character = input[charIndex];

            bool isHexDigit = (character >= '0' && character <= '9') || (character >= 'a' && character <= 'f') ||
                              (character >= 'A' && character <= 'F');

            if (!isHexDigit)
                return false;
        }

        return true;
    }
}