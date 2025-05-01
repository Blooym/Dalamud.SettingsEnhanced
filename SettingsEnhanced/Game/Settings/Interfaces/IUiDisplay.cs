namespace SettingsEnhanced.Game.Settings.Interfaces
{
    public interface IUiDisplay
    {
        /// <summary>
        ///     The name to use when displaying this item.
        /// </summary>
        public string InterfaceName { get; }

        /// <summary>
        ///     The main group this item should be placed under.
        /// </summary>
        public string InterfaceGroup { get; }

        /// <summary>
        ///     The heading this item should be placed under.
        /// </summary>
        public string InterfaceHeaderName { get; }

        /// <summary>
        ///     Whether this item should be indented when displayed.
        /// </summary>
        public bool Indented { get; }
    }
}
