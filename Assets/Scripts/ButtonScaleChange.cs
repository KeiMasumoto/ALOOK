using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonScaleChange : MonoBehaviour
{

    IEnumerator ScaleUp()
    {
        GameObject obj = GameObject.Find("UnMask");

        float t;


        for (float i = 1.0f; i < 10; i += t)
        {
            obj.transform.localScale = new Vector3(i, i, i);
            yield return new WaitForSeconds(0.1f);
            if (i < 1.5f)
            {
                t = 1.25f;
            }
            else if (i > 9.0f)
            {
                t = 1.5f;
            }
            else
            {
                t = 1.5f;
            }

        }
    }

    public void OnClick()
    {
        StartCoroutine("ScaleUp");

    }

}
