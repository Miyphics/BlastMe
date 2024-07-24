using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

public class LocalizationManager : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern string GetLang();

    private string currentLanguage = "en_US";
    public string CurrentLanguage
    {
        get { return currentLanguage; }
        set { currentLanguage = GetLangFromText(value); }
    }
    private Dictionary<string, string> localizedText;

    private bool isReady;
    public delegate void ChangeLangText();
    public event ChangeLangText OnLanguageChanged;

    public static LocalizationManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
            return;
        }

        Debug.Log("Loading language...");

        string lang;

        if (Application.platform == RuntimePlatform.WebGLPlayer)
            lang = GetLang();
        else
        {
            lang = Application.systemLanguage switch
            {
                SystemLanguage.Russian => "ru_RU",
                SystemLanguage.Turkish => "tr_TR",
                _ => "en_US",
            };
        }

        Debug.Log("Current lang: " + lang);

        StartCoroutine(LoadLocalizedTextIE(GetLangFromText(lang)));
    }

    private string GetLangFromText(string text)
    {
        var split = text.Split('_');
        if (split.Length == 2)
        {
            return split[0].ToLower() + "_" + split[1].ToUpper();
        }
        else
        {
            return text switch
            {
                string s when s.StartsWith("ru") => "ru_RU",
                string s when s.StartsWith("tr") => "tr_TR",
                _ => "en_US",
            };
        }
    }

    public IEnumerator LoadLocalizedTextIE(string langName)
    {
        if (currentLanguage == langName)
            yield break;

        isReady = false;
        string path = Path.Combine(Application.streamingAssetsPath, "Languages");
        path = Path.Combine(path, $"{langName}.json");

        //Debug.Log("FinalLangName: " + langName + "\nPath: " + path);

        if (File.Exists(path) || Application.platform == RuntimePlatform.WebGLPlayer || Application.platform == RuntimePlatform.Android)
        {
            string data;

            if ((Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.WebGLPlayer) && !Application.isEditor)
            {
                Debug.Log("Using mobile or webgl...");
                using UnityWebRequest unityWebRequest = UnityWebRequest.Get(path);
                yield return unityWebRequest.SendWebRequest();
                data = System.Text.Encoding.UTF8.GetString(unityWebRequest.downloadHandler.data, 3, unityWebRequest.downloadHandler.data.Length - 3);
            }
            else
            {
                data = File.ReadAllText(path);
            }

            if (data == null || data == "")
            {
                Debug.LogError("The language file is empty");
                yield break;
            }

            LocalizationData loadedData;

            try
            {
                loadedData = JsonUtility.FromJson<LocalizationData>(data);
            }
            catch
            {
                Debug.LogError("The language file cannot be downloaded");
                yield break;
            }
            

            localizedText = new();
            for (int i = 0; i < loadedData.items.Length; i++)
            {
                localizedText.Add(loadedData.items[i].key, loadedData.items[i].value);
            }

            currentLanguage = langName;
            isReady = true;

            OnLanguageChanged?.Invoke();
        }
        else
        {
            throw new Exception("Language file not found!");
        }
    }

    public string GetLocalizedText(string key)
    {
        if (currentLanguage == "en_US")
            return key;

        if (isReady)
        {
            if (localizedText.ContainsKey(key))
            {
                return localizedText[key];
            }
            else
            {
                throw new Exception($"Key \"{key}\" is not found in the languages file");
            }
        }
        else
        {
            return key;
        }
    }
}
