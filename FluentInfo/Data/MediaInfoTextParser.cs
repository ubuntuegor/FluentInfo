using FluentInfo.Controls.PrettyView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentInfo.Data
{
    public class MediaInfoTextParser
    {
        private const string PROPERTY_SEPARATOR = " : ";

        private MediaInfoTextParser() { }

        private static Section CreateSection(string title, OrderedProperties properties)
        {
            SectionType type = SectionType.OTHER;

            if (title != null)
            {
                if (title.StartsWith("General")) type = SectionType.GENERAL;
                else if (title.StartsWith("Video")) type = SectionType.VIDEO;
                else if (title.StartsWith("Audio")) type = SectionType.AUDIO;
                else if (title.StartsWith("Text")) type = SectionType.TEXT;
                else if (title.StartsWith("Menu")) type = SectionType.MENU;
            }

            return new Section(type, title, null, [], properties);
        }

        public static List<Section> Parse(string text)
        {
            var sections = new List<Section>();

            var lines = Utils.SplitToLines(text, StringSplitOptions.RemoveEmptyEntries);

            string title = null;
            OrderedProperties properties = new();

            foreach (string line in lines)
            {
                var index = line.IndexOf(PROPERTY_SEPARATOR);

                if (index == -1)
                {
                    // current line is a title
                    if (properties.Count > 0)
                    {
                        sections.Add(CreateSection(title, properties));
                        properties = new();
                    }

                    title = line.Trim();
                }
                else
                {
                    // current line is a field
                    var name = line[..index].Trim();
                    var value = line[(index + PROPERTY_SEPARATOR.Length)..].Trim();

                    properties.Add(name, value);
                }
            }

            if (properties.Count > 0)
            {
                sections.Add(CreateSection(title, properties));
            }

            return sections;
        }
    }
}
