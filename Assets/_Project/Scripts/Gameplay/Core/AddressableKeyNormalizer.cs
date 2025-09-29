public class AddressableKeyNormalizer
{
    public object Normalize(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return string.Empty;

        raw = raw.Trim();

        if (int.TryParse(raw, out var i))
            return i;

        if (long.TryParse(raw, out var l))
            return l;

        return raw;
    }
}