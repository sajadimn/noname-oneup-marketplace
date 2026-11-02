using UnityEngine;

public static class AndroidVersionCode
{
    private const int LongVersionCodeSdk = 28;

    public static int Get()
    {
        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
        using (AndroidJavaObject packageManager = activity.Call<AndroidJavaObject>("getPackageManager"))
        using (AndroidJavaObject packageInfo = packageManager.Call<AndroidJavaObject>("getPackageInfo",
                   activity.Call<string>("getPackageName"), 0))
        using (AndroidJavaClass buildVersion = new AndroidJavaClass("android.os.Build$VERSION"))
        {
            return buildVersion.GetStatic<int>("SDK_INT") >= LongVersionCodeSdk
                ? (int)packageInfo.Call<long>("getLongVersionCode")
                : packageInfo.Get<int>("versionCode");
        }
    }
}
