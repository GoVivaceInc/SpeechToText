// Copyright 2017 All Rights Reserved in GoVivace.Inc
using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4;
using Android.Support.V7.App;
using Android.Views;
using Android.Preferences;
using Java.Lang;


namespace SpeechToText
{
    [Activity(Label = "SettingActivity", Theme = "@style/AppTheme", ParentActivity = typeof(MainActivity))]
    public class SettingActivity :AppCompatActivity, ISharedPreferencesOnSharedPreferenceChangeListener
    {
        private string mKeyService;
        private PrefsFragment mSettingsFragment;
        private ISharedPreferences mPrefs;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            mSettingsFragment = new PrefsFragment();
            mPrefs = PreferenceManager.GetDefaultSharedPreferences(this);
            mKeyService = GetString(Resource.String.keyService);
            SetContentView(Resource.Layout.activity_main);
            FragmentManager

                .BeginTransaction()

                .Replace(Resource.Id.content, mSettingsFragment)

                .Commit();
            if (SupportActionBar != null)
            {
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            }
        }

        public class PrefsFragment : PreferenceFragment
        {
            public override void OnCreate(Bundle savedInstanceState)
            {
                base.OnCreate(savedInstanceState);
                AddPreferencesFromResource(Resource.Xml.Setting);
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Android.Resource.Id.Home)
            {
                Android.Support.V4.App.NavUtils.NavigateUpFromSameTask(this);
            }
            return base.OnOptionsItemSelected(item);
        }

        protected override void OnResume()
        {
            base.OnResume();
            RecognitionServiceManager mngr = new RecognitionServiceManager();
            ListPreference prefServices = (ListPreference)mSettingsFragment.FindPreference(mKeyService);
            populateServices(prefServices,mngr.getServices(PackageManager));
            populateLangs();
            mPrefs.RegisterOnSharedPreferenceChangeListener(this);
        }

        
        private void populateServices(ListPreference prefServices, IList<string> services)
        {
            ICharSequence[] entryValues = new ICharSequence[services.Count];
            for (int i = 0; i < services.Count; i++)
            {
                ICharSequence servicevalue = new Java.Lang.String(services[i]);
                entryValues[i] = servicevalue;
            }
            System.String[] entries = new System.String[services.Count];
            string selectedService = mPrefs.GetString(mKeyService, null);
            int index = 0;
            int selectedIndex = 0;
            foreach (ICharSequence service in entryValues)
            {
                ICharSequence temp = new Java.Lang.String(RecognitionServiceManager.getServiceLabel(this, service.ToString()));
                entries[index] = RecognitionServiceManager.getServiceLabel(this, service.ToString());
                if (Equals(service.ToString(),selectedService))
                {
                    selectedIndex = index;
                } 
                index++;
            }
            prefServices.SetEntries(entries);
            prefServices.SetEntryValues(entryValues);
            prefServices.SetValueIndex(selectedIndex);
            prefServices.Summary=prefServices.Entry;
           
        }

        private void populateLangs()
        {
            ListPreference prefServices = (ListPreference)mSettingsFragment.FindPreference(mKeyService);
            RecognitionServiceManager mngr = new RecognitionServiceManager();
        }

        public void OnComplete(List<string> combos, ISet<string> selectedCombos)
        {
            ISet<string> languages = new HashSet<string>();
            foreach (string combo in combos)
            {
                string[] serviceAndLang = RecognitionServiceManager.getServiceAndLang(combo);
                languages.Add(serviceAndLang[1]);
            }
            
        }


        public void OnSharedPreferenceChanged(ISharedPreferences sharedPreferences, string key)
        {
            if (key.Equals(mKeyService))
            {
                ListPreference pref = (ListPreference)mSettingsFragment.FindPreference(key);
                pref.Summary = pref.Entry;
                ISharedPreferencesEditor editor = mPrefs.Edit();
                editor.PutString(GetString(Resource.String.keyService), pref.Value);
                editor.Apply();
                populateLangs();
            }
        }

        protected override void OnPause()
        {
            base.OnPause();
            mPrefs.
                UnregisterOnSharedPreferenceChangeListener(this);
        }
    }
}
// Copyright 2017 All Rights Reserved in GoVivace.Inc