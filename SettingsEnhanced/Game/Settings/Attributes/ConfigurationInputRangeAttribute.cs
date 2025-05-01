using System;

namespace SettingsEnhanced.Game.Settings.Attributes
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    internal sealed class ConfigurationInputRangeAttribute(int min, int max) : Attribute
    {
        public int Min { get; } = min;
        public int Max { get; } = max;
    }
}
