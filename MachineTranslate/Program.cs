using OpenAI.Chat;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Xml;

List<string>? langs = null;
List<string>? includeKeys = null;
List<string> excludeKeys = ["MediaInfoLanguage"];

var sourceLang = "en";
var context = """
You are a software localization expert. Read the description of the software below:

FluentInfo is an app for Windows that displays detailed information about various media files using the MediaInfo library. It has multiple settings to configure how this information is displayed.

Provide a translation for each of the following strings into the language specified by the ISO639 code **{0}**. Use common Windows OS terminology. Try to keep strings the same width as in English. Be consistent when introducing new terminology.
""";

langs ??= getLangs();
var sourceStrings = getStrings(sourceLang);
var jsonOptions = new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, WriteIndented = true };
var client = new ChatClient(model: "gpt-5.2", apiKey: Environment.GetEnvironmentVariable("OPENAI_API_KEY"));

foreach (var lang in langs)
{
    var prompt = string.Format(context, lang);

    prompt += "\n\nStrings:\n----\n\n```json\n";
    prompt += JsonSerializer.Serialize(sourceStrings, jsonOptions);
    prompt += "\n```\n";

    Console.WriteLine(prompt);

    List<ChatMessage> messages = [new UserChatMessage(prompt)];
    var options = new ChatCompletionOptions()
    {
        ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
            jsonSchemaFormatName: "localization_strings",
            jsonSchema: BinaryData.FromBytes("""
            {
                "type": "object",
                "properties": {
                    "strings": {
                        "type": "array",
                        "items": {
                            "type": "object",
                            "properties": {
                                "key": { "type": "string" },
                                "value": { "type": "string" }
                            },
                            "required": ["key", "value"],
                            "additionalProperties": false
                        }
                    }
                },
                "required": ["strings"],
                "additionalProperties": false
            }
            """u8.ToArray()),
            jsonSchemaIsStrict: true
        )
    };
    ChatCompletion completion = client.CompleteChat(messages, options);

    Console.WriteLine(completion.Content[0].Text);
    var results = JsonSerializer.Deserialize<TranslatedStrings>(completion.Content[0].Text, JsonSerializerOptions.Web)!.Strings;

    if (results.Count != sourceStrings.Count)
    {
        Console.WriteLine($"WARN: Wrong number of strings for lang {lang}! Expected: {sourceStrings.Count}, Got: {results.Count}");
    }

    writeStrings(lang, results);
}

List<ResourceString> getStrings(string lang)
{
    var doc = new XmlDocument();
    doc.Load(@$"FluentInfo\Strings\{lang}\Resources.resw");

    var nodes = doc.SelectNodes("root/data")!;
    var result = new List<ResourceString>();

    foreach (XmlNode item in nodes)
    {
        var key = item.Attributes!["name"]!.InnerText;
        if (includeKeys != null && !includeKeys.Contains(key)) continue;
        if (excludeKeys.Contains(key)) continue;
        var value = item["value"]!.InnerText;
        var comment = item["comment"]!.InnerText;
        result.Add(new ResourceString(key, value, comment));
    }

    return result;
}

void writeStrings(string lang, List<TranslatedString> strings)
{
    var doc = new XmlDocument();
    doc.Load(@$"FluentInfo\Strings\{lang}\Resources.resw");

    foreach (var resource in strings)
    {
        var node = doc.SelectSingleNode($"root/data[@name='{resource.Key}']")!;
        node["value"]!.InnerText = resource.Value;
    }

    doc.Save(@$"FluentInfo\Strings\{lang}\Resources.resw");
}

List<string> getLangs()
{
    return [.. Directory.GetDirectories(@"FluentInfo\Strings").Select(path => Path.GetFileName(path)).Where(name => name != sourceLang)];
}

record ResourceString(string Key, string EnglishValue, string Comment);
record TranslatedString(string Key, string Value);
record TranslatedStrings(List<TranslatedString> Strings);
