using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Map;

public class GPSettingKeyLoadFromExternalFile : MonoBehaviour
{
    public string filePath;
    // Start is called before the first frame update
    void Start()
    {
        if (!string.IsNullOrEmpty(filePath))
        {
            TextAsset textAsset = Resources.Load(filePath) as TextAsset;

            if (textAsset != null)
            {
                GPAPIKey gpKey = JsonUtility.FromJson<GPAPIKey>(textAsset.text);
                MapController.GoogleApiKey = gpKey.applicationKey;
                DrawTokyo.GoogleApiKey = gpKey.applicationKey;
            }
        }   
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

[Serializable]
internal class GPAPIKey
{
    public string applicationKey;
}