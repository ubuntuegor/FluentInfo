namespace FluentInfoCommon;

public class OrderedProperties
{
    private readonly List<(string, string)> storage = [];
    private readonly Dictionary<string, string> dictionary = [];

    public int Count => storage.Count;

    public void Add(string key, string value)
    {
        storage.Add((key, value));
        dictionary[key] = value;
    }

    public string? Get(string key)
    {
        return dictionary.GetValueOrDefault(key);
    }

    public List<(string, string)> GetPairs()
    {
        return storage;
    }
}
