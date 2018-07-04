using System.Collections.Generic;
using System.Configuration;

namespace FileLister.Properties
{
    // This class allows you to handle specific events on the settings class:
    //  The SettingChanging event is raised before a setting's value is changed.
    //  The PropertyChanged event is raised after a setting's value is changed.
    //  The SettingsLoaded event is raised after the setting values are loaded.
    //  The SettingsSaving event is raised before the setting values are saved.
    internal sealed partial class Settings
    {
        public Settings()
        {
            SettingChanging += SettingChangingEventHandler;
        }

        protected override void OnSettingsLoaded(object sender, SettingsLoadedEventArgs e)
        {
            base.OnSettingsLoaded(sender, e);

            if (IgnoredExtensions == null)
            {
                var defaultIgnoredExtensions = new List<string> {".jpg"};

                IgnoredExtensions = new List<string>(defaultIgnoredExtensions);
                Save();
            }
        }

        private void SettingChangingEventHandler(object sender, SettingChangingEventArgs e)
        {
        }
    }
}