namespace FluentInfoCommon;

public enum SectionType
{
    General,
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
    string? SubTitle,
    List<string> Chips,
    OrderedProperties Properties
);