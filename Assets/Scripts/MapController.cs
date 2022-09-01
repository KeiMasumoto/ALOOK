using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Map
{
    public class MapController : MonoBehaviour
    {
        /// <summary>
        /// Google API Key & Base URL
        /// </summary>
        public static string GoogleApiKey;  // 設定してください
        private string BaseUrl = @"https://maps.googleapis.com/maps/api/staticmap?";

        /// <summary>
        /// 緯度（経度）１度の距離（m）
        /// </summary>
        private const float Lat2Meter = 111319.491f;

        /// <summary>
        /// マップ更新の閾値
        /// </summary>
        private const float ThresholdDistance = 10f;

        /// <summary>
        /// マップ更新時間
        /// </summary>
        private const float UpdateMapTime = 5f;

        /// <summary>
        /// ダウンロードするマップイメージのサイズ
        /// </summary>
        private const int MapImageSize = 640;

        /// <summary>
        /// 画面に表示するマップスプライトのサイズ
        /// </summary>
        private const int MapSpriteSize = 1280;

        /// <summary>
        /// ズームのサイズ
        /// </summary>
        public int Zoom = 17;

        // 現在地を描画するモードとタッチで画面を動かすモードのフラグ falseで現在地を描画
        private bool modeChangeFlag = false;

        //　描画する地図の中央の座標
        private float centerPosition;
        private UnityEngine.Vector2 prevTouchPosition;
        private UnityEngine.Vector2 touchedMoveDistance;
        private float distanceX = 0;
        private float distanceY = 0;


        [SerializeField] GameObject loading;        // ダウンロード確認用オブジェクト
        [SerializeField] Text txtLocation;    // 座標
        [SerializeField] Text txtDistance;    // 距離
        [SerializeField] Image mapImage;       // マップ Image
        [SerializeField] Cursor cursor;         // カーソル

        /// <summary>
        /// 起動時処理
        /// </summary>
        void Start()
        {
            // ローディング表示を非表示にしておく
            loading.SetActive(false);
            updateDistance(0f);

            // GPS 初期化
            Input.location.Start();
            Input.compass.enabled = true;

            // マップ取得
            StartCoroutine(updateMap());
        }

        private float timeleft;

        void Update()
        {

            OnTouchOperation();

            //秒ごとに実行
            timeleft -= Time.deltaTime;
            if (timeleft <= 0.0)
            {
                timeleft = 0.25f;

                //Zoomの値が変更されたらマップを更新
                int ZoomPast = 17;

                if (Zoom != ZoomPast)
                {
                    curr = Input.location.lastData;
                    StartCoroutine(downloading(curr));
                    ZoomPast = Zoom;

                }
            }

        }

        // private void getTouchDistance()
        // {
        //     if (Input.touchCount > 0)
        //     {
        //         Touch touch = Input.GetTouch(0);
        //         if (touch.phase == TouchPhase.Began)
        //         {
        //             // 現在タッチしているところにレイを作成します
        //             prevTouchPosition = touch.position;
        //         }
        //         else if (touch.phase == TouchPhase.Ended)
        //         {
        //             touchedPosition = touch.position;
        //         }
        //     }
        // }

        // タッチ移動量に応じてオブジェクトを移動させる処理
        private void OnTouchOperation()
        {
            // 移動用タッチの情報取得(とりあえずシングルタッチのみ対応)
            if (0 < Input.touchCount)
            {
                modeChangeFlag = true;
                Touch touchPos = Input.GetTouch(0);

                // 移動していたら移動量を取得してオブジェクトを移動
                if (TouchPhase.Moved == touchPos.phase)
                {
                    touchedMoveDistance = (touchPos.deltaPosition / touchPos.deltaTime) * Time.deltaTime;
                }
            }

            return;
        }



        /// <summary>
        /// マップ更新
        /// </summary>
        /// <returns></returns>
        private LocationInfo curr;
        private IEnumerator updateMap()
        {
            // GPS が許可されていない
            if (!Input.location.isEnabledByUser) yield break;

            // サービスの状態が起動中になるまで待機
            while (Input.location.status != LocationServiceStatus.Running)
            {
                yield return new WaitForSeconds(2f);
            }

            // カーソルをアクティブに
            cursor.IsEnabled = true;

            //LocationInfo curr;
            LocationInfo prev = new LocationInfo();


            while (true)
            {
                // 現在位置
                curr = Input.location.lastData;

                txtLocation.text = string.Format("緯度：{0:0.000000}, 経度：{1:0.000000}", curr.latitude, curr.longitude);

                // 一定以上移動している
                if (getDistanceFromLocation(curr, prev) >= ThresholdDistance)
                {
                    // マップ見込み
                    yield return StartCoroutine(downloading(curr));
                    prev = curr;
                }
                else if (0 < Input.touchCount)
                {
                    // マップ見込み
                    yield return StartCoroutine(downloading(curr));
                    prev = curr;
                }
                // // 一定以上移動している
                // if (getDistanceFromLocation(curr, prev) >= ThresholdDistance)
                // {
                //     // マップ見込み
                //     yield return StartCoroutine(downloading(curr));
                //     prev = curr;
                // }

                // 待機
                // yield return new WaitForSeconds(UpdateMapTime);
                yield return new WaitForSeconds((float)1/60);
            }


        }

        /// <summary>
        /// マップ画像ダウンロード
        /// </summary>
        /// <param name="curr">現在の座標</param>
        /// <returns>コルーチン</returns>
        private IEnumerator downloading(LocationInfo curr)
        {
            loading.SetActive(true);

            // ベース URL
            string url = BaseUrl;
            // 中心座標
            if (modeChangeFlag)
            {
                distanceX -= (touchedMoveDistance.x / 10000);
                distanceY -= (touchedMoveDistance.y / 10000);
            }

            float latitude =  curr.latitude + distanceY;
            float longitude =  curr.longitude + distanceX;
            url += "center=" + latitude + "," + longitude;
            // ズーム
            url += "&zoom=" + Zoom;   // デフォルト 0 なので、適当なサイズに設定
            // 画像サイズ（640x640が上限）
            url += "&size=" + MapImageSize + "x" + MapImageSize;

            // マーカーを指定する　松本
            url += "&markers=" + "icon:http://epogames.html.xdomain.jp/marker1x.png" + "|35.676400780203686,139.7621372994596" + "|35.681676207463006,139.7671952215971" + "|35.6857758776159,139.7528958394739";


            // API Key
            url += "&key=" + GoogleApiKey;

            // 地図画像をダウンロード
            url = UnityWebRequest.UnEscapeURL(url);
            UnityWebRequest req = UnityWebRequestTexture.GetTexture(url);
            yield return req.SendWebRequest();

            // テクスチャ生成
            if (req.error == null) yield return StartCoroutine(updateSprite(req.downloadHandler.data));

            updateDistance(0f);
            loading.SetActive(false);
        }

        /// <summary>
        /// スプライトの更新
        /// </summary>
        /// <param name="data">マップ画像データ</param>
        /// <returns>コルーチン</returns>
        private IEnumerator updateSprite(byte[] data)
        {
            // テクスチャ生成
            Texture2D tex = new Texture2D(MapSpriteSize, MapSpriteSize);
            tex.LoadImage(data);
            if (tex == null) yield break;
            // スプライト（インスタンス）を明示的に開放
            if (mapImage.sprite != null)
            {
                Destroy(mapImage.sprite);
                yield return null;
                mapImage.sprite = null;
                yield return null;
            }
            // スプライト（インスタンス）を動的に生成
            mapImage.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
        }

        /// <summary>
        /// 2点間の距離を取得する
        /// </summary>
        /// <param name="curr">現在の座標</param>
        /// <param name="prev">直前の座標</param>
        /// <returns>距離（メートル）</returns>
        private float getDistanceFromLocation(LocationInfo curr, LocationInfo prev)
        {
            float dist = 0;
            if (modeChangeFlag)
            {
                Vector3 cv = new Vector3((float)curr.longitude - ( touchedMoveDistance.y / 10000 ), 0, (float)curr.latitude - ( touchedMoveDistance.x / 10000 ));
                Vector3 pv = new Vector3((float)prev.longitude, 0, (float)prev.latitude);
                dist = Vector3.Distance(cv, pv) * Lat2Meter;
                updateDistance(dist);
            }
            else
            {
                Vector3 cv = new Vector3((float)curr.longitude, 0, (float)curr.latitude);
                Vector3 pv = new Vector3((float)prev.longitude, 0, (float)prev.latitude);
                dist = Vector3.Distance(cv, pv) * Lat2Meter;
                updateDistance(dist);
            }
            return dist;
        }

        /// <summary>
        /// 距離表示
        /// </summary>
        /// <param name="dist">距離</param>
        private void updateDistance(float dist)
        {
            txtDistance.text = string.Format("距離：{0:0.0000} m", dist);
        }


    }

}
