using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinchInOutMap : MonoBehaviour
{
    Map.MapController MapController;

 
    
    /// RootオブジェクトからMapControllerの呼び出し
       void Start()
    {
        MapController = GameObject.Find("Root").GetComponent<Map.MapController>();
                     
    }

    //変更可能なズーム値
    private float MaxZoom = 22;
    private float MinZoom = 0;

    //直前の2点間の距離.
    private float backDist = 0.0f;

    //初期値
    private float vZoom = 17;


    /// ピンチインアウトでMapControllerのZoom値を変更
    void Update()
    {
        // マルチタッチであることを確認
        if (Input.touchCount >= 2)
        {
            // タッチしている2点を取得
            Touch t1 = Input.GetTouch(0);
            Touch t2 = Input.GetTouch(1);

            //　2点タッチ開始時の距離を取得
            if (t2.phase == TouchPhase.Began)
            {
                backDist = Vector2.Distance(t1.position, t2.position);
            }
            else if (t1.phase == TouchPhase.Moved && t2.phase == TouchPhase.Moved)
            {
                // タッチ位置の移動後、長さを再測し、前回の距離からの相対値取得
                float newDist = Vector2.Distance(t1.position, t2.position);
                vZoom = vZoom + (newDist - backDist) / 1000.0f;

                // 限界値をオーバーした際の処理
                if (vZoom > MaxZoom)
                {
                    vZoom = MaxZoom;
                }
                else if (vZoom < MinZoom)
                {
                    vZoom = MinZoom;
                }

                // floatをintに変換
                MapController.Zoom = (int)vZoom;


            }
        }



    }
}
