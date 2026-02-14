using System.Globalization;
using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;

namespace FluentInfoCommon;

public partial class MediaInfoTextParser
{
    private Dictionary<string, string> _strings = new();

    [GeneratedRegex(@" *\(.+\) *")]
    private static partial Regex ParenthesesWithSpacesRegex();

    public void UpdateLanguage(string languageText)
    {
        _strings = ParseLanguageText(languageText);
    }

    private string? GetSubtitle(OrderedProperties properties)
    {
        var performer = properties.Get(_strings["Performer"]) ?? properties.Get("ARTIST");
        var title = properties.Get(_strings["Title"]);

        if (performer != null && title != null) return performer + " - " + title;

        return title;
    }

    private string? GetResolution(OrderedProperties properties)
    {
        var width = properties.Get(_strings["Width"]);
        var height = properties.Get(_strings["Height"]);

        if (width == null || height == null) return null;

        width = string.Concat(width.Where(char.IsAsciiDigit));
        height = string.Concat(height.Where(char.IsAsciiDigit));
        return width + "x" + height;
    }

    private List<string?> GetChipsForGeneral(OrderedProperties properties)
    {
        return
        [
            properties.Get(_strings["Format"]), properties.Get(_strings["FileSize"]),
            properties.Get(_strings["Duration"])
        ];
    }

    private List<string?> GetChipsForVideo(OrderedProperties properties)
    {
        var framerate = properties.Get(_strings["FrameRate"]);
        if (framerate != null) framerate = ParenthesesWithSpacesRegex().Replace(framerate, " ");

        return
        [
            properties.Get(_strings["Format"]), GetResolution(properties), framerate,
            properties.Get(_strings["BitRate"])
        ];
    }

    private List<string?> GetChipsForAudio(OrderedProperties properties)
    {
        return
        [
            properties.Get(_strings["Format"]), properties.Get(_strings["Language"]),
            properties.Get(_strings["Channel(s)"]), properties.Get(_strings["BitRate"])
        ];
    }

    private List<string?> GetChipsForText(OrderedProperties properties)
    {
        return [properties.Get(_strings["Format"]), properties.Get(_strings["Language"])];
    }

    private List<string?> GetChipsForMenu(OrderedProperties properties)
    {
        return [_strings["Total"] + ": " + properties.Count];
    }

    private List<string?> GetChipsForImage(OrderedProperties properties)
    {
        return [properties.Get(_strings["Format"]), GetResolution(properties)];
    }

    private List<string> GetChips(SectionType type, OrderedProperties properties)
    {
        var chips = type switch
        {
            SectionType.General => GetChipsForGeneral(properties),
            SectionType.Video => GetChipsForVideo(properties),
            SectionType.Audio => GetChipsForAudio(properties),
            SectionType.Text => GetChipsForText(properties),
            SectionType.Menu => GetChipsForMenu(properties),
            SectionType.Image => GetChipsForImage(properties),
            _ => []
        };

        chips.RemoveAll(x => x == null);

        return chips as List<string>;
    }

    private Section CreateSection(string? title, OrderedProperties properties)
    {
        var type = SectionType.Other;

        var generalName = _strings["General"];
        var videoName = _strings["Video"];
        var audioName = _strings["Audio"];
        var textName = _strings["Text"];
        var menuName = _strings["Menu"];
        var imageName = _strings["Image"];

        if (title != null)
        {
            if (title.StartsWith(generalName)) type = SectionType.General;
            else if (title.StartsWith(videoName)) type = SectionType.Video;
            else if (title.StartsWith(audioName)) type = SectionType.Audio;
            else if (title.StartsWith(textName)) type = SectionType.Text;
            else if (title.StartsWith(menuName)) type = SectionType.Menu;
            else if (title.StartsWith(imageName)) type = SectionType.Image;
        }

        var subtitle = GetSubtitle(properties);
        var chips = GetChips(type, properties);

        return new Section(type, title, subtitle, chips, properties);
    }

    public List<Section> Parse(string text)
    {
        var sections = new List<Section>();
        var lines = Utils.SplitToLines(text, StringSplitOptions.RemoveEmptyEntries);

        var propertySeparator = _strings["  Config_Text_Separator"];

        string? title = null;
        OrderedProperties properties = new();

        foreach (var line in lines)
        {
            var index = line.IndexOf(propertySeparator, StringComparison.Ordinal);

            if (index == -1)
            {
                // current line is a title
                if (properties.Count > 0)
                {
                    sections.Add(CreateSection(title, properties));
                    properties = new OrderedProperties();
                }

                title = line.Trim();
            }
            else
            {
                // current line is a field
                var name = line[..index].Trim();
                var value = line[(index + propertySeparator.Length)..].Trim();

                properties.Add(name, value);
            }
        }

        if (properties.Count > 0) sections.Add(CreateSection(title, properties));

        return sections;
    }

    private static Dictionary<string, string> ParseLanguageText(string languageText)
    {
        var result = new Dictionary<string, string>();

        var csvOptions = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";",
            HasHeaderRecord = false
        };

        using var stringReader = new StringReader(languageText);
        using var csv = new CsvReader(stringReader, csvOptions);

        while (csv.Read())
        {
            var key = csv.GetField<string>(0)!;
            var value = csv.GetField<string>(1)!;
            result.Add(key, value);
        }

        return result;
    }
}