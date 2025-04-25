namespace SettingsEnhanced.Game.Settings.Interfaces
{
    public interface IUiDisplay
    {
        /// <summary>
        ///     User-interface friendly name of this property.
        /// </summary>
        public string InterfaceName { get; }

        /// <summary>
        ///     User-interface header this property belongs to.
        /// </summary>
        public string InterfaceHeaderName { get; }

        /// <summary>
        ///     The interface group this property belongs to.
        /// </summary>
        public string InterfaceGroup { get; }

        /// <summary>
        ///     Whether this property is nested under the previous one.
        /// </summary>
        public bool Nested { get; }
    }
}
