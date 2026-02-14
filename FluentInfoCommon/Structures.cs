namespace FluentInfoCommon;

public enum SectionType
{
    General = 1,
    Video,
    Audio,
    Text,
    Menu,
    Image,
    Other
}

public record Section(
    SectionType Type,
    string? Title,
    string? Subtitle,
    List<string> Chips,
    OrderedProperties Properties
);