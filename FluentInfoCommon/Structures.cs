using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentInfoCommon
{
    public enum SectionType
    {
        GENERAL,
        VIDEO,
        AUDIO,
        TEXT,
        MENU,
        IMAGE,
        OTHER
    }

    public record Section(
        SectionType Type,
        string Title,
        string SubTitle,
        List<string> Chips,
        OrderedProperties Properties
    );
}
