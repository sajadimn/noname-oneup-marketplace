using UnityEditor;

[CustomEditor(typeof(StoreConfigs))]
public class StoreConfigsEditor : Editor
{
    private const string ScriptField = "m_Script";
    private static readonly string[] AdsFields = { "showForceAdsUnlockLevel", "showAdsLimitPerDay" };

    public static string[] GetHiddenFields(bool isAds)
    {
        return isAds ? new[] { ScriptField } : new[] { ScriptField, AdsFields[0], AdsFields[1] };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawPropertiesExcluding(serializedObject, GetHiddenFields(serializedObject.FindProperty("isAds").boolValue));
        serializedObject.ApplyModifiedProperties();
    }
}
