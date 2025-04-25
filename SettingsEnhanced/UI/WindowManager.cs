using System;
using System.Linq;
using Dalamud.Interface.Windowing;
using SettingsEnhanced.UI.Windows;

namespace SettingsEnhanced.UI
{
    internal sealed class WindowManager : IDisposable
    {
        private bool disposedValue;

        /// <summary>
        ///     All windows to add to the windowing system, holds all references.
        /// </summary>
        private readonly Window[] windows = [new ConfigurationWindow()];

        /// <summary>
        ///     The windowing system.
        /// </summary>
        private readonly WindowSystem windowingSystem;

        /// <summary>
        ///     Initializes a new instance of the <see cref="WindowManager" /> class.
        /// </summary>
        public WindowManager()
        {
            this.windowingSystem = new WindowSystem(Plugin.PluginInterface.Manifest.InternalName);
            foreach (var window in this.windows)
            {
                this.windowingSystem.AddWindow(window);
            }
            Plugin.PluginInterface.UiBuilder.Draw += this.windowingSystem.Draw;
            Plugin.ClientState.Login += this.OnLogin;
            Plugin.ClientState.Logout += this.OnLogout;
            if (Plugin.ClientState.IsLoggedIn)
            {
                Plugin.PluginInterface.UiBuilder.OpenConfigUi += this.ToggleConfigWindow;
            }
        }

        /// <summary>
        ///     Disposes of the window manager.
        /// </summary>
        public void Dispose()
        {
            if (this.disposedValue)
            {
                ObjectDisposedException.ThrowIf(this.disposedValue, nameof(this.windowingSystem));
                return;
            }
            Plugin.ClientState.Login -= this.OnLogin;
            Plugin.ClientState.Logout -= this.OnLogout;
            Plugin.PluginInterface.UiBuilder.OpenConfigUi -= this.ToggleConfigWindow;
            Plugin.PluginInterface.UiBuilder.Draw -= this.windowingSystem.Draw;
            this.windowingSystem.RemoveAllWindows();
            foreach (var disposable in this.windows.OfType<IDisposable>())
            {
                disposable.Dispose();
            }
            this.disposedValue = true;
        }

        /// <summary>
        ///     Toggles the open state of the configuration window.
        /// </summary>
        private void ToggleConfigWindow()
        {
            ObjectDisposedException.ThrowIf(this.disposedValue, nameof(this.windowingSystem));
            this.windows.FirstOrDefault(window => window is ConfigurationWindow)?.Toggle();
        }

        private void OnLogin() => Plugin.PluginInterface.UiBuilder.OpenConfigUi += this.ToggleConfigWindow;
        private void OnLogout(int type, int code) => Plugin.PluginInterface.UiBuilder.OpenConfigUi -= this.ToggleConfigWindow;
    }
}
