using System;

namespace QoLWitchNobeta.Config;

[AttributeUsage(AttributeTargets.Class)]
public class SectionAttribute : Attribute
{
    public string SectionName { get; }

    public SectionAttribute(string sectionName)
    {
        SectionName = sectionName;
    }
}