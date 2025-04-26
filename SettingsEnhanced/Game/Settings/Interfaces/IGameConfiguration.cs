using System.Reflection;

namespace SettingsEnhanced.Game.Settings.Interfaces
{
    public interface IGameConfiguration<TInner>
    {
        /// <summary>
        ///     Persist all values so they are retained during serialisation and deserialisation.
        /// </summary>
        public TInner PersistAllValues();

        /// <summary>
        ///     "De-persist" all values so they will not be retained during serialisation and deserialisation.
        /// </summary>
        public TInner DepersistAllValues();

        /// <summary>
        ///     Whether this configuration has any persisted values.
        /// </summary>
        /// <returns></returns>
        public bool HasPersistedValues();

        /// <summary>
        ///     Whether a property is marked as persisted.
        /// </summary>
        public bool IsPropertyPersistent(PropertyInfo prop);

        /// <summary>
        ///     Set a property to the given value and persist it.
        /// </summary>
        public void SetPropertyPersistent<T>(PropertyInfo prop, T value);

        /// <summary>
        ///     Set a property to the given value and remove any persistence it may have.
        /// </summary>
        public void SetProperty<T>(PropertyInfo prop, T value);

    }
}
