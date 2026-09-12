using UnityEngine;

namespace CapstoneDesign.Runtime
{
    /// <summary>
    /// Reads the optional Android Intent extra used by hardware CI. The first
    /// mockup has one scene, so the mode is logged and exposed for later
    /// deterministic scene entry points without adding native Java code.
    /// </summary>
    public sealed class AndroidTestMode : MonoBehaviour
    {
        public const string DefaultMode = "GardenPreview";

        public string ActiveMode { get; private set; } = DefaultMode;

        private void Awake()
        {
            ActiveMode = ReadRequestedMode();
            Debug.Log("Mockup test mode: " + ActiveMode);
        }

        private static string ReadRequestedMode()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                using (AndroidJavaObject intent = activity.Call<AndroidJavaObject>("getIntent"))
                {
                    string requested = intent.Call<string>("getStringExtra", "testScene");
                    if (!string.IsNullOrEmpty(requested))
                    {
                        return requested;
                    }
                }
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning("Test mode Intent read failed: " + exception.Message);
            }
#endif
            return DefaultMode;
        }
    }
}
