using System.Reflection;

namespace SettingsEnhanced.Game.Settings.Interfaces
{
    public interface IGameConfiguration<TInner>
    {
        /// <summary>
        ///     Persist all properties so they are kept after serialisation and deserialisation.
        /// </summary>
        public TInner PersistAllProperties();

        /// <summary>
        ///     "De-persist" all properties so they are not kept after serialisation and deserialisation.
        /// </summary>
        public TInner DepersistAllProperties();

        /// <summary>
        ///     Whether this configuration has any persisted properties.
        /// </summary>
        public bool AnyPersistedProperties();

        /// <summary>
        ///     Whether a property is marked as persisted.
        /// </summary>
        public bool IsPropertyPersisted(PropertyInfo prop);

        /// <summary>
        ///     Mark a property as persisted.
        /// </summary>
        public bool PersistProperty(PropertyInfo prop);

        /// <summary>
        ///     Unmark a property as persisted.
        /// </summary>
        public bool DepersistProperty(PropertyInfo prop);

        /// <summary>
        ///     Set a property to the given value.
        /// </summary>
        /// <remarks>
        ///     The property will automatically be marked as modified.
        /// </remarks>
        public void SetPropertyValue<T>(PropertyInfo prop, T value);
    }
}
