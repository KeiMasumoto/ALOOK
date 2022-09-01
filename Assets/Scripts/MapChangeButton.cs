using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapChangeButton : MonoBehaviour
{
    public void OnClick()
    {
        SceneManager.LoadScene("map");

    }
}
