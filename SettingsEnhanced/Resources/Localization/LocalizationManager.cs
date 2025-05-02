using System;
using System.Globalization;

namespace SettingsEnhanced.Resources.Localization
{
    internal sealed class LocalizationManager : IDisposable
    {
        private bool disposedValue;

        /// <summary>
        ///     Creates a new resource manager and sets up resources.
        /// </summary>
        public LocalizationManager()
        {
            SetupLocalization(Plugin.PluginInterface.UiLanguage);
            Plugin.PluginInterface.LanguageChanged += SetupLocalization;
        }

        /// <summary>
        ///     Disposes of the <see cref="LocalizationManager" />
        /// </summary>
        public void Dispose()
        {
            if (this.disposedValue)
            {
                return;
            }

            Plugin.PluginInterface.LanguageChanged -= SetupLocalization;
            this.disposedValue = true;
        }

        /// <summary>
        ///     Sets up localization for the given language, or uses fallbacks if not found.
        /// </summary>
        /// <param name="language">The language to use.</param>
        private static void SetupLocalization(string language)
        {
            try
            {
                Plugin.Log.Debug($"Setting up localization for {language}");
                Strings.Culture = new CultureInfo(language);
            }
            catch
            {
                // ignored
            }
        }
    }
}
