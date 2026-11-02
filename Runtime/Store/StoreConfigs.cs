using System.Collections.Generic;
using UnityEngine;

public class StoreConfigs : ScriptableObject
{
    public string projectName;
    public string privacyURL = "";
    public string supportURL = "";
    public string supportMail = "";
    public string defaultLanguageCode = "fa-IR";
    public List<string> supportLanguages = new List<string>();
    public bool isAds = false;
    public int showForceAdsUnlockLevel = 15;
    public int showAdsLimitPerDay = -1;
}