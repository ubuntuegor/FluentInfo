using FluentInfoCommon;
using MediaInfoLib;

namespace FluentInfoTest;

[TestClass]
public class SectionParserTest
{
    private readonly MediaInfo _mediaInfo = new();

    private string? ReadMediaInfoForFile(string filePath)
    {
        return _mediaInfo.Open(filePath) ? _mediaInfo.Inform() : null;
    }

    private static void CheckSection(Section section, SectionType type, string? subtitle, List<string> chips)
    {
        Assert.AreEqual(type, section.Type);
        Assert.AreEqual(subtitle, section.SubTitle);
        CollectionAssert.AreEqual(chips, section.Chips,
            "Expected chips: [" + string.Join(",", chips) + "], got chips: [" + string.Join(",", section.Chips) +
            "]");
    }

    [TestMethod]
    [DeploymentItem(@"Assets\test.mkv")]
    public void TestMkvFile()
    {
        const string filePath = "test.mkv";
        Assert.IsTrue(File.Exists(filePath));

        var info = ReadMediaInfoForFile(filePath);
        Assert.IsNotNull(info);
        var sections = MediaInfoTextParser.Parse(info);

        var generalSection = sections[0];
        var videoSection = sections[1];
        var audioSection = sections[2];
        var textSection = sections[3];
        var menuSection = sections[4];

        CheckSection(generalSection, SectionType.General, "File title", ["Matroska", "35.2 KiB", "30 s 23 ms"]);
        CheckSection(videoSection, SectionType.Video, "Video Title", ["AVC", "320x240", "25.000 FPS", "4 108 b/s"]);
        CheckSection(audioSection, SectionType.Audio, "Audio Title",
            ["AAC LC", "English", "2 channels", "2 091 b/s"]);
        CheckSection(textSection, SectionType.Text, "Sub Title", ["UTF-8", "English"]);
        CheckSection(menuSection, SectionType.Menu, null, ["2 entries"]);
    }

    [TestMethod]
    [DeploymentItem(@"Assets\test.mp3")]
    public void TestMp3File()
    {
        const string filePath = "test.mp3";
        Assert.IsTrue(File.Exists(filePath));

        var info = ReadMediaInfoForFile(filePath);
        Assert.IsNotNull(info);
        var sections = MediaInfoTextParser.Parse(info);

        var generalSection = sections[0];

        CheckSection(generalSection, SectionType.General, "Rick Astley - Never Gonna Give You Up",
            ["MPEG Audio", "41.8 KiB", "5 s 41 ms"]);
    }

    [TestMethod]
    [DeploymentItem(@"Assets\test.webm")]
    public void TestWebmAudioFile()
    {
        const string filePath = "test.webm";
        Assert.IsTrue(File.Exists(filePath));

        var info = ReadMediaInfoForFile(filePath);
        Assert.IsNotNull(info);
        var sections = MediaInfoTextParser.Parse(info);

        var generalSection = sections[0];

        CheckSection(generalSection, SectionType.General, "Infekt - Projectile", ["WebM", "65.6 KiB", "5 s 8 ms"]);
    }
}