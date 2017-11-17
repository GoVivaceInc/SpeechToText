// Copyright 2017 All Rights Reserved in GoVivace.Inc
using System;
using System.Collections.Generic;
using Android.Content;
using Android.Speech;
using Android.Text;
using Android.Graphics.Drawables;
using Android.Content.PM;

namespace SpeechToText
{
    public class RecognitionServiceManager
    {
        private static String SEPARATOR = ";";
        private static ISet<String> mInitiallySelectedCombos = new HashSet<String>();
        private static ISet<String> mCombosExcluded = new HashSet<String>();
        public interface Listener
        {
            void OnComplete(List<string> combos, ISet<string> selectedCombos);
        }

        /**
         * @return true iff a RecognitionService with the given component name is installed
         */
        public static Boolean isRecognitionServiceInstalled(PackageManager pm, ComponentName componentName)
        {
            IList<ResolveInfo> services = pm.QueryIntentServices(
                    new Intent(RecognitionService.ServiceInterface), 0);
            foreach (ResolveInfo ri in services)
            {
                ServiceInfo si = ri.ServiceInfo;
                if (si == null)
                {
                    continue;
                }
                if (componentName.Equals(new ComponentName(si.PackageName, si.Name)))
                {
                    return true;
                }
            }
            return false;
        }

        public static String[] getServiceAndLang(String str)
        {
            return TextUtils.Split(str, SEPARATOR);
        }

        public static String getServiceLabel(Context context, String service)
        {
            ComponentName recognizerComponentName = ComponentName.UnflattenFromString(service);
            return getServiceLabel(context, recognizerComponentName);
        }

        public static String getServiceLabel(Context context, ComponentName recognizerComponentName)
        {

            try
            {
                PackageManager pm = context.PackageManager;
                ServiceInfo si = pm.GetServiceInfo(recognizerComponentName, 0);
                return si.LoadLabel(pm).ToString();
            }
            catch (PackageManager.NameNotFoundException e)
            {

                Console.WriteLine("call getservicelabel" + e.ToString());
            }
            return "[?]";
        }

        /**
         * @return list of currently installed RecognitionService component names flattened to short strings
         */
        public IList<String> getServices(PackageManager pm)
        {
            IList<String> services = new List<String>();
            PackageInfoFlags flags = 0;

            
            IList<ResolveInfo> infos = pm.QueryIntentServices(
                    new Intent(RecognitionService.ServiceInterface), flags);

            foreach (ResolveInfo ri in infos)
            {
                ServiceInfo si = ri.ServiceInfo;
                if (si == null)
                {
                    
                    continue;
                }
                string pkg = si.PackageName;
                string cls = si.Name;
                // TODO: process si.metaData
                string component = (new ComponentName(pkg, cls)).FlattenToShortString();
                if (!mCombosExcluded.Contains(component))
                {
                    services.Add(component);
                }
            }
            return services;
        }
    }
}
// Copyright 2017 All Rights Reserved in GoVivace.Inc
