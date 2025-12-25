namespace FluentInfoCommon;

public static class MediaInfoTextParser
{
    private const string PropertySeparator = " : ";

    private static string? GetSubtitle(OrderedProperties properties)
    {
        var performer = properties.Get("Performer") ?? properties.Get("ARTIST");
        var title = properties.Get("Title");

        if (performer != null && title != null) return performer + " - " + title;

        return title;
    }

    private static string? GetResolution(OrderedProperties properties, string widthKey = "Width",
        string heightKey = "Height")
    {
        var width = properties.Get(widthKey);
        var height = properties.Get(heightKey);

        if (width == null || height == null) return null;

        width = width.Replace("pixels", "").Replace(" ", "");
        height = height.Replace("pixels", "").Replace(" ", "");
        return width + "x" + height;
    }

    private static List<string?> GetChipsForGeneral(OrderedProperties properties)
    {
        return [properties.Get("Format"), properties.Get("File size"), properties.Get("Duration")];
    }

    private static List<string?> GetChipsForVideo(OrderedProperties properties)
    {
        var framerate = properties.Get("Frame rate");
        if (framerate != null)
        {
            var open = framerate.IndexOf('(');
            var close = framerate.IndexOf(')');

            if (open != -1 && close != -1) framerate = framerate.Remove(open, close - open + 2);
        }

        return [properties.Get("Format"), GetResolution(properties), framerate, properties.Get("Bit rate")];
    }

    private static List<string?> GetChipsForAudio(OrderedProperties properties)
    {
        return
        [
            properties.Get("Format"), properties.Get("Language"),
            properties.Get("Channel(s)"), properties.Get("Bit rate")
        ];
    }

    private static List<string?> GetChipsForText(OrderedProperties properties)
    {
        return [properties.Get("Format"), properties.Get("Language")];
    }

    private static List<string?> GetChipsForMenu(OrderedProperties properties)
    {
        return [properties.Count + " entries"];
    }

    private static List<string?> GetChipsForImage(OrderedProperties properties)
    {
        return [properties.Get("Format"), GetResolution(properties)];
    }

    private static List<string> GetChips(SectionType type, OrderedProperties properties)
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

    private static Section CreateSection(string? title, OrderedProperties properties)
    {
        var type = SectionType.Other;

        if (title != null)
        {
            if (title.StartsWith("General")) type = SectionType.General;
            else if (title.StartsWith("Video")) type = SectionType.Video;
            else if (title.StartsWith("Audio")) type = SectionType.Audio;
            else if (title.StartsWith("Text")) type = SectionType.Text;
            else if (title.StartsWith("Menu")) type = SectionType.Menu;
            else if (title.StartsWith("Image")) type = SectionType.Image;
        }

        var subtitle = GetSubtitle(properties);
        var chips = GetChips(type, properties);

        return new Section(type, title, subtitle, chips, properties);
    }

    public static List<Section> Parse(string text)
    {
        var sections = new List<Section>();
        var lines = Utils.SplitToLines(text, StringSplitOptions.RemoveEmptyEntries);

        string? title = null;
        OrderedProperties properties = new();

        foreach (var line in lines)
        {
            var index = line.IndexOf(PropertySeparator, StringComparison.Ordinal);

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
                var value = line[(index + PropertySeparator.Length)..].Trim();

                properties.Add(name, value);
            }
        }

        if (properties.Count > 0) sections.Add(CreateSection(title, properties));

        return sections;
    }
}