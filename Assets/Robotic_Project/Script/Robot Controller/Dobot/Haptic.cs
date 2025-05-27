using UnityEngine;

public class HapticFeedback : MonoBehaviour
{
#if UNITY_ANDROID && !UNITY_EDITOR
    private AndroidJavaObject vibrator;

    private void Awake()
    {
        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
        }
    }

    public void Vibrate(long milliseconds)
    {
        if (vibrator != null)
        {
            vibrator.Call("vibrate", milliseconds);
        }
    }
#else
    public void Vibrate(long milliseconds)
    {
        // Optional: simulate vibration in editor
        Debug.Log($"[Haptic] Vibrate for {milliseconds} ms");
    }
#endif
}
