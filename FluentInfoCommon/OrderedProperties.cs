namespace FluentInfoCommon;

public class OrderedProperties
{
    private readonly Dictionary<string, string> _dictionary = [];
    private readonly List<(string, string)> _storage = [];

    public int Count => _storage.Count;

    public void Add(string key, string value)
    {
        _storage.Add((key, value));
        _dictionary[key] = value;
    }

    public string? Get(string key)
    {
        return _dictionary.GetValueOrDefault(key);
    }

    public List<(string, string)> GetPairs()
    {
        return _storage;
    }
}