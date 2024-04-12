using MediaInfoLib;
using FluentInfoCommon;

namespace FluentInfoTest
{
    [TestClass]
    public class SectionParserTest
    {
        private readonly MediaInfo mediaInfo = new();

        private string ReadMediaInfoForFile(string filePath)
        {
            mediaInfo.Open(filePath);
            return mediaInfo.Inform();
        }

        private static void CheckSection(Section section, SectionType type, string subtitle, List<string> chips)
        {
            Assert.AreEqual(type, section.Type);
            Assert.AreEqual(subtitle, section.SubTitle);
            CollectionAssert.AreEqual(chips, section.Chips, "Expected chips: [" + string.Join(",", chips) + "], got chips: [" + string.Join(",", section.Chips) + "]");
        }

        [TestMethod]
        [DeploymentItem(@"Assets\test.mkv")]
        public void TestFile()
        {
            string filePath = "test.mkv";
            Assert.IsTrue(File.Exists(filePath));

            var info = ReadMediaInfoForFile(filePath);
            var sections = MediaInfoTextParser.Parse(info);

            var generalSection = sections[0];
            var videoSection = sections[1];
            var audioSection = sections[2];
            var textSection = sections[3];
            var menuSection = sections[4];

            CheckSection(generalSection, SectionType.GENERAL, "File title", ["Matroska", "35.2 KiB", "30 s 23 ms"]);
            CheckSection(videoSection, SectionType.VIDEO, "Video Title", ["AVC", "320x240", "25.000 FPS", "4 108 b/s"]);
            CheckSection(audioSection, SectionType.AUDIO, "Audio Title", ["AAC LC", "English", "2 channels", "2 091 b/s"]);
            CheckSection(textSection, SectionType.TEXT, "Sub Title", ["UTF-8", "English"]);
            CheckSection(menuSection, SectionType.MENU, null, ["2 entries"]);
        }
    }
}