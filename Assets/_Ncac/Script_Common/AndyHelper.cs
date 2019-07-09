using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleARCore;

namespace NCToolset
{
    //This class includes helper functions specific to Android device
    public static class AndyHelper
    {
        /// <summary>
        /// Show an Android toast message.
        /// </summary>
        /// <param name="message">Message string to show in the toast.</param>
        static public void ShowToastMsg(string msg)
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            if (unityActivity != null)
            {
                AndroidJavaObject toastClass = new AndroidJavaClass("android.widget.Toast");
                unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity, msg, 0);
                    toastObject.Call("show");
                }));

            }
        }

        public static IEnumerator ToastAndQuit(string msg, float timeout)
        {
            ShowToastMsg(msg);
            yield return new WaitForSecondsRealtime(timeout);
            Application.Quit();
        }
    }
}
