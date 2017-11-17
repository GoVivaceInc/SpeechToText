// Copyright 2017 All Rights Reserved in GoVivace.Inc
using Android.App;
using Android.Widget;
using Android.OS;
using Android.Speech;
using Android.Content;
using Android.Content.Res;
using static Android.Resource;
using Android.Preferences;
using System;
using Android.Support.V4.App;
using Android;
using Java.Util;
using Android.Views;
using Java.Lang;
using static SpeechToText.Resource;

namespace SpeechToText
{
    [Activity(Label = "SpeechToText", MainLauncher = true, Icon = "@drawable/ic_launcher")]
    public class MainActivity : Activity, IRecognitionListener
    {
        private TextView textBox;
        private Button recButton;
        private ISharedPreferences mPrefs;
        private SpeechRecognizer mSr;
        bool islistening;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);
            mPrefs = PreferenceManager.GetDefaultSharedPreferences(this);
            recButton = FindViewById<Button>(Resource.Id.btnRecord);
            textBox = FindViewById<TextView>(Resource.Id.textYourText);     
        }
        public void OnBeginningOfSpeech(){}

        public void OnBufferReceived(byte[] buffer) { }

        public void OnEndOfSpeech()
        {
            islistening = false;
            Toast.MakeText(this,"Stop Recording", ToastLength.Long).Show();
            recButton.Text = "Start Recording";
        }

        public void OnError(SpeechRecognizerError error)
        {  
            switch (error)
            {
                case SpeechRecognizerError.Audio:
                    showError(Resource.String.errorResultAudioError);
                    break;
                case SpeechRecognizerError.Client:
                    showError(Resource.String.errorResultClientError);
                    break;
                case SpeechRecognizerError.Network:
                    showError(Resource.String.errorResultNetworkError);
                    break;
                case SpeechRecognizerError.NetworkTimeout:
                    showError(Resource.String.errorResultNetworkError);
                    break;
                case SpeechRecognizerError.Server:
                    showError(Resource.String.errorResultServerError);
                    break;
                case SpeechRecognizerError.RecognizerBusy:
                    showError(Resource.String.errorResultServerError);
                    break;
               
                case SpeechRecognizerError.SpeechTimeout:
                    showError(Resource.String.errorResultNoMatch);
                    break;
                case SpeechRecognizerError.InsufficientPermissions:
                    ActivityCompat.RequestPermissions(this,
                            new string[] { Manifest.Permission.RecordAudio },
                            0);
                    showError(Resource.String.errorResultInsufficientPermissionsError);
                    break;
                default:
                    break;
            }
        }

        public void OnEvent(int eventType, Bundle Params) { }

        public void OnPartialResults(Bundle partialResults)
        {
            var data = partialResults.GetStringArrayList(
                SpeechRecognizer.ResultsRecognition);
            System.Collections.Generic.IList<string> unstableData = partialResults.GetStringArrayList("android.speech.extra.UNSTABLE_TEXT");
            string sTextFromSpeech;
            if (data != null)
            {
                sTextFromSpeech = data[0];
                textBox.Text = sTextFromSpeech;
            }
            else
            {
                sTextFromSpeech = "";
            }

        }
        public void OnReadyForSpeech(Bundle Params) { }

        public void OnResults(Bundle results)
        {
            var matches = results.GetStringArrayList(SpeechRecognizer.ResultsRecognition);
            string sTextFromSpeech;
            if (matches != null)
            {
                sTextFromSpeech = matches[0];
                textBox.Text = sTextFromSpeech;
            }
            else
            {
                sTextFromSpeech = "";
            }     
        }

        public void OnRmsChanged(float rmsdB) { }

        private void showError(int errorResultNetworkError)
        {
            Toast.MakeText(this, GetString(errorResultNetworkError), ToastLength.Long).Show();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu, menu);
            return base.OnCreateOptionsMenu(menu);
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            var id = item.ItemId;
            if (id == Resource.Id.action_menu)
            {
                StartActivity(new Intent(this, typeof(SettingActivity)));
            }
            return true;
        }

        protected override void OnStart()
        {
            base.OnStart();
            ComponentName serviceComponent = ComponentName.UnflattenFromString(
                    mPrefs.GetString(GetString(Resource.String.keyService), GetString(Resource.String.nameK6neleService)));
            if (mPrefs.GetBoolean(GetString(Resource.String.prefFirstTime), true))
            {
                if (RecognitionServiceManager.isRecognitionServiceInstalled(PackageManager, serviceComponent))
                {
                    ISharedPreferencesEditor editor = mPrefs.Edit();
                    editor.PutString(GetString(Resource.String.keyService), GetString(Resource.String.nameK6neleService));
                    editor.PutBoolean(GetString(Resource.String.prefFirstTime), false);
                    editor.Apply();
                }
            }
            if (serviceComponent == null)
            { 
                Toast.MakeText(this,GetString(Resource.String.errorNoDefaultRecognizer), ToastLength.Long).Show(); 
            }
            else
            {
                mSr = SpeechRecognizer.CreateSpeechRecognizer(this, serviceComponent);
                if (mSr == null)
                {
                    Toast.MakeText(this, GetString(Resource.String.errorNoDefaultRecognizer), ToastLength.Long).Show();
                }
                else
                {
                    Intent intentRecognizer = createRecognizerIntent("en-US");
                    setUpRecognizerGui(mSr, intentRecognizer);
                }
            }
        }

        private Intent createRecognizerIntent(string langSource)
        {
            Locale locale = new Locale(langSource);
            Intent intent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
            intent.PutExtra(RecognizerIntent.ExtraCallingPackage, true);
            intent.PutExtra(RecognizerIntent.ExtraPartialResults, true);
            intent.PutExtra(RecognizerIntent.ExtraLanguageModel, true);
            intent.PutExtra(RecognizerIntent.ExtraLanguage, langSource);
            intent.PutExtra(RecognizerIntent.ExtraPrompt, "Hello, How can I help you?");
            return intent;
        }

        private void setUpRecognizerGui(SpeechRecognizer sr, Intent intentRecognizer)
        {
            sr.SetRecognitionListener(this);
            string rec = Android.Content.PM.PackageManager.FeatureMicrophone;
            if (rec != "android.hardware.microphone")
            {
                // no microphone, no recording. Disable the button and output an alert
                var alert = new AlertDialog.Builder(recButton.Context);
                alert.SetTitle("You don't seem to have a microphone to record with");
                alert.SetPositiveButton("OK", (sender, e) =>
                {
                    textBox.Text = "No microphone present";
                    recButton.Enabled = false;
                    return;
                });

                alert.Show();
            }
            else
            {
                recButton.Click += (sender, e) =>
                {
                    // Perform action on click
                    if (!islistening)
                    {
                        sr.StartListening(intentRecognizer);
                        islistening = true;
                        Toast.MakeText(this, "Start Recording", ToastLength.Long).Show();
                        recButton.Text = "Stop Recording";
                    }
                    else
                    {
                        sr.StopListening();
                    }

                };
            }
           
        }
        
    }
}
// Copyright 2017 All Rights Reserved in GoVivace.Inc


