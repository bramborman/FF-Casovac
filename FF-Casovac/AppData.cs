﻿using Newtonsoft.Json;
using NotifyPropertyChangedBase;
using System;
using System.Threading.Tasks;
using UWPHelper.Utilities;
using Windows.Storage;

namespace FF_Casovac
{
    public sealed class AppData : NotifyPropertyChanged
    {
        private const string FILE_NAME = "AppData.json";

        public static AppData Current { get; private set; }

        [JsonIgnore]
        public bool ShowLoadingError { get; set; }
        public TimeSpan ActualTimeSpan
        {
            get { return (TimeSpan)GetValue(); }
            set { SetValue(value); }
        }
        public bool? IsAutomaticFullScreenModeEnabled
        {
            get { return (bool?)GetValue(); }
            set { SetValue(value); }
        }
        public bool? IsTimerStoppingEnabled
        {
            get { return (bool?)GetValue(); }
            set { SetValue(value); }
        }
        public bool? IsSoundEnabled
        {
            get { return (bool?)GetValue(); }
            set { SetValue(value); }
        }
        
        public AppData()
        {
            RegisterProperty(nameof(ActualTimeSpan), typeof(TimeSpan), TimeSpan.Zero);
            RegisterProperty(nameof(IsAutomaticFullScreenModeEnabled), typeof(bool?), true);
            RegisterProperty(nameof(IsTimerStoppingEnabled), typeof(bool?), true);
            RegisterProperty(nameof(IsSoundEnabled), typeof(bool?), true);
        }

        public Task SaveAsync()
        {
            return StorageHelper.SaveObjectAsync(this, FILE_NAME, ApplicationData.Current.LocalFolder);
        }

        public static async Task LoadAsync()
        {
#if DEBUG
            if (Current != null)
            {
                throw new Exception("You're not doing it right ;)");
            }
#endif

            var loadResult              = await StorageHelper.LoadObjectAsync<AppData>(FILE_NAME, ApplicationData.Current.LocalFolder);
            Current                     = loadResult.LoadedObject;
            Current.ShowLoadingError    = loadResult.Status == StorageFileHelperStatus.Failure;

            Current.PropertyChanged += async (sender, e) =>
            {
                if (e.PropertyName != nameof(ActualTimeSpan))
                {
                    await Current.SaveAsync();
                }
            };
        }
    }
}
