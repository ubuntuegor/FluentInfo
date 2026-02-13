using System.Diagnostics.CodeAnalysis;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Xml;
using OpenAI.Chat;

List<string>? langs = null;
List<string>? includeKeys = null;
List<string> excludeKeys = ["MediaInfoLanguage"];

const string sourceLang = "en";
const string context =
    """
    You are a software localization expert. Read the description of the software below:

    FluentInfo is an app for Windows that displays detailed information about various media files using the MediaInfo library. It has multiple settings to configure how this information is displayed.

    Provide a translation for each of the following strings into the language specified by the ISO639 code **{0}**. Use common Windows OS terminology. Try to keep strings the same width as in English. Be consistent when introducing new terminology.
    """;

var jsonOptions =
    new JsonSerializerOptions { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, WriteIndented = true };

langs ??= GetLangs();
var sourceStrings = GetStrings(sourceLang);

foreach (var lang in langs)
{
    var prompt = FormatPrompt(lang, sourceStrings);
    Console.WriteLine(prompt);

    var translatedStrings = TranslateStringsWithGpt(prompt);
    Console.WriteLine(translatedStrings);

    if (translatedStrings.Count != sourceStrings.Count)
        Console.WriteLine(
            $"WARN: Wrong number of strings for lang {lang}! Expected: {sourceStrings.Count}, Got: {translatedStrings.Count}");

    WriteStrings(lang, translatedStrings);
}

return;

List<ResourceString> GetStrings(string lang)
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

void WriteStrings(string lang, List<TranslatedString> strings)
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

List<string> GetLangs()
{
    return [.. Directory.GetDirectories(@"FluentInfo\Strings").Select(Path.GetFileName).Except([sourceLang])!];
}

List<TranslatedString> TranslateStringsWithGpt(string s)
{
    var chatClient = new ChatClient("gpt-5.2", Environment.GetEnvironmentVariable("OPENAI_API_KEY"));
    List<ChatMessage> messages = [new UserChatMessage(s)];
    var options = new ChatCompletionOptions
    {
        ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
            "localization_strings",
            BinaryData.FromBytes("""
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
    ChatCompletion chatCompletion = chatClient.CompleteChat(messages, options);
    return JsonSerializer.Deserialize<TranslatedStrings>(chatCompletion.Content[0].Text, JsonSerializerOptions.Web)!
        .Strings;
}

string FormatPrompt(string lang, List<ResourceString> resourceStrings)
{
    var prompt = string.Format(context, lang);

    prompt += "\n\nStrings:\n----\n\n```json\n";
    prompt += JsonSerializer.Serialize(resourceStrings, jsonOptions);
    prompt += "\n```\n";
    return prompt;
}

[SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Global")]
internal record ResourceString(string Key, string EnglishValue, string Comment);

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal record TranslatedString(string Key, string Value);

internal record TranslatedStrings(List<TranslatedString> Strings);