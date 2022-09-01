using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.IO;

namespace Map
{

    public class DrawTokyo : MonoBehaviour
    {
        /// <summary>
        /// Google API Key & Base URL
        /// </summary>
        public static string GoogleApiKey;  // 設定してください
        private string BaseUrl      = @"https://maps.googleapis.com/maps/api/staticmap?";

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
        private const int MapImageSize  = 640; 

        /// <summary>
        /// 画面に表示するマップスプライトのサイズ
        /// </summary>
        private const int MapSpriteSize = 1280; 

        [SerializeField] GameObject loading;        // ダウンロード確認用オブジェクト
        // [SerializeField] Text       txtLocation;    // 座標
        // [SerializeField] Text       txtDistance;    // 距離
        [SerializeField] Image      mapImage;       // マップ Image
        [SerializeField] Cursor     cursor;         // カーソル

        LocationInfo curr;
        LocationInfo prev;

        /// <summary>
        /// 起動時処理
        /// </summary>
        void Start()
        {
            // ローディング表示を非表示にしておく
            loading.SetActive(false);
            // updateDistance(0f);

            // GPS 初期化
            Input.location.Start();
            Input.compass.enabled = true;

            // マップ取得
            StartCoroutine(updateMap());

            // GPLログ
            StartCoroutine(UpdateLocation());
        }

        private IEnumerator UpdateLocation()
        {
            while (true) {
                // 毎フレームループします
                yield return new WaitForSeconds(UpdateMapTime);
                saveLocation();
            }
        }

        private void saveLocation()
        {
            string dir = makeCsv();
            UnityEngine.Debug.Log("test");
            if(curr.latitude != null)
            {
                List<string> line = getLocationList();
                File.AppendAllLines(dir, line, System.Text.Encoding.UTF8);
            }
        }

        private List<string> getLocationList()
        {
            // #CSVデータをリストで用意
            List<string> location = new List<string>();
            location.Add("'" + curr.latitude.ToString() + "'" + "," + "'" + curr.longitude.ToString() + "'"); // 緯度 経度
            // #リストの内容をファイル（CSV)の末尾に追加する

            return location;
        }

        private string makeCsv()
        {
            string dir = Application.streamingAssetsPath + "/location_log.csv";
            Debug.Log(dir);
            if(!File.Exists(dir))
            {      
                StreamWriter sw = new StreamWriter(dir);
                sw.Close();
            }

            return dir;
        }

        /// <summary>
        /// マップ更新
        /// </summary>
        /// <returns></returns>
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
                
            // LocationInfo curr;
            prev = new LocationInfo();
            while(true)
            {
                // 現在位置
                curr = Input.location.lastData;

                // 一定以上移動している
                // if(getDistanceFromLocation(curr, prev) >= ThresholdDistance)
                // {
                //     // マップ見込み
                //     yield return StartCoroutine(downloading(curr));
                //     prev = curr;
                // }

                StartCoroutine(downloading(curr));
                prev = curr;

                // 待機
                yield return new WaitForSeconds(UpdateMapTime);
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
            // 中心座標 日本橋の座標35.6825376 139.7737089
            // url += "center=" + curr.latitude + "," + curr.longitude;
            url += "center=" + 35.71543112622532	 + "," + 139.402592038906;
            // ズーム
            url += "&zoom=" + 8;   // デフォルト 0 なので、適当なサイズに設定
            // 画像サイズ（640x640が上限）
            url += "&size=" + MapImageSize + "x" + MapImageSize;

            url += "&style=" + "saturation:-100";

            // マーカーを指定する　松本
            // url += "&markers=" + "icon:http://epogames.html.xdomain.jp/marker1x.png" + "|35.676400780203686,139.7621372994596" + "|35.681676207463006,139.7671952215971" + "|35.6857758776159,139.7528958394739";
            
            url += "&path=" + "weight:2" + "|color:red" + "|35.6663,139.3158|35.7363832,139.651215|35.6313051,139.7777839" + "&path=" + "color:red" + jointLocation();
            url += "&path=" + "weight:2" + "|color:blue" + "|35.730070123572936, 139.5440804112299|35.7003702149922, 139.5762527090414|35.75135708812812, 139.80448107069628|35.636551868115355, 139.49943742927454";

            // API Key
            url += "&key=" + GoogleApiKey;

            // 地図画像をダウンロード
            url = UnityWebRequest.UnEscapeURL(url);
            UnityWebRequest req = UnityWebRequestTexture.GetTexture(url);
            yield return req.SendWebRequest();

            // テクスチャ生成
            if(req.error == null) yield return StartCoroutine(updateSprite(req.downloadHandler.data));

            // updateDistance(0f);
            loading.SetActive(false);
        }

        private string jointLocation()
        {
            //ファイル名
            var fileName = Application.streamingAssetsPath + "/location_log.csv";

            string googleMapPath = null;

            try
            {
                //ファイルをオープンする
                using (StreamReader sr = new StreamReader(fileName))
                {
                    while (0 <= sr.Peek())
                    {
                        //カンマ区切りで分割して配列で格納する
                        var line = sr.ReadLine()?.Split(',');
                        if (line is null) continue;
                        Debug.Log(line[0]);
                        Debug.Log(line[1]);

                        googleMapPath += "|" + line[0] + "," + line[1];
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }

            return googleMapPath;

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
            Vector3 cv = new Vector3((float)curr.longitude, 0, (float)curr.latitude);
            Vector3 pv = new Vector3((float)prev.longitude, 0, (float)prev.latitude);
            float dist = Vector3.Distance(cv, pv) * Lat2Meter;
            // updateDistance(dist);        
            return dist;        
        }

        // /// <summary>
        // /// 距離表示
        // /// </summary>
        // /// <param name="dist">距離</param>
        // private void updateDistance(float dist)
        // {
        //     txtDistance.text = string.Format("距離：{0:0.0000} m", dist);
        // }

    }
}