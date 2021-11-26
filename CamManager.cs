using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class CamManager : MonoBehaviour
{
    public static CamManager instance;

    [HideInInspector] public WebCamTexture WebCamTex;
    public static string fileName;   //拍摄图片存储路径
    string filePath;                //拍摄图片文件夹存储路径
    public RawImage rawImage;
    public GameObject showImage;
    public Text warning;
    public GameObject countDownBack;
    Text countDown;
    //public GameObject shine;

    float countDownTime = 5;

    void Start()
    {
        instance = this;
        countDown = countDownBack.GetComponentInChildren<Text>();
        Manager.instance._CamShow += TakePhoto;
        showImage.SetActive(false);
        warning.enabled = false;
        countDownBack.SetActive(false);
        //shine.SetActive(false);
        filePath = Application.dataPath + "/Photos";
        print(filePath);
        StartCoroutine(OpenCamera());
    }

    void Update()
    {
       
    }

    #region 拍照倒计时
    IEnumerator PhotoTime()
    {
        countDownBack.SetActive(true);
        //shine.SetActive(true);
        Debug.Log(countDown.text);

        if (countDownTime <= 0)
        {
            yield break;
        }
        
        while (countDownTime > 0)
        {
            if (!countDownBack.activeSelf)
            {
                yield break;
            }
            else
            {
                countDown.text = countDownTime.ToString();
                yield return new WaitForSeconds(1);

                countDownTime--;
            }
        }
        countDownBack.SetActive(false);

        //shine.SetActive(false);

        showImage.GetComponent<RawImage>().texture = CaptureCamera(Camera.main, new Rect(0,0,1920,1080));     //截图;
        showImage.SetActive(true);
        yield return new WaitForSeconds(1);
        Manager.instance.SetCamShow(false);
        showImage.GetComponent<Animator>().SetBool("isShow", true);
        AudioManager.instance.ShootVoice();

        UploadPhoto.isSave = true;      //上传二维码
        Manager.instance.btnParent.SetAsLastSibling();
    }
    #endregion

    #region 拍照
    void TakePhoto(bool b)
    {
        if (WebCamTex == null)
        {
            warning.enabled = true;
            warning.text = "没有摄像头设备，请检查";
            return;
        }
        WebCamTex.Play();
        AudioManager.instance.CountDown();
        countDownTime = 5;
        StartCoroutine(PhotoTime());
    }
    #endregion


    /// <summary>  
    /// 对相机截图。   
    /// </summary>  
    /// <returns>The screenshot2.</returns>  
    /// <param name="camera">Camera.要被截屏的相机</param>  
    /// <param name="rect">Rect.截屏的区域</param>  
    Texture2D CaptureCamera(Camera camera, Rect rect)
    {
        // 创建一个RenderTexture对象  new RenderTexture(Screen.width, Screen.height, 0);
        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 0);
        // 临时设置相关相机的targetTexture为rt, 并手动渲染相关相机  
        camera.targetTexture = rt;
        camera.Render();
        //ps: --- 如果这样加上第二个相机，可以实现只截图某几个指定的相机一起看到的图像。  
        //ps: camera2.targetTexture = rt;  
        //ps: camera2.Render();  
        //ps: -------------------------------------------------------------------  

        // 激活这个rt, 并从中中读取像素。  
        RenderTexture.active = rt;
        Texture2D screenShot = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.RGB24, false);
        screenShot.ReadPixels(rect, 0, 0);// 注：这个时候，它是从RenderTexture.active中读取像素
        screenShot.Apply();

        // 重置相关参数，以使用camera继续在屏幕上显示  
        camera.targetTexture = null;
        //ps: camera2.targetTexture = null;  
        RenderTexture.active = null; // JC: added to avoid errors  
        Destroy(rt);
        // 最后将这些纹理数据，成一个jpg图片文件
        byte[] bytes = screenShot.EncodeToJPG();
        fileName = filePath + "/" + DateTime.Now.ToString("yyMMdd_HHmmss") + ".jpg";

        File.WriteAllBytes(fileName, bytes);
        Debug.Log(string.Format("截屏了一张照片: {0}", fileName));

        return screenShot;
    }


    public IEnumerator OpenCamera()
    {
        int maxl = Screen.width;
        if (Screen.height > Screen.width)
        {
            maxl = Screen.height;
        }

        // 申请摄像头权限
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (Application.HasUserAuthorization(UserAuthorization.WebCam))
        {
            if (WebCamTex != null)
            {
                WebCamTex.Stop();
                print("stop");
            }

            //打开渲染图
            if (rawImage != null)
            {
                rawImage.enabled = true;
            }

            // 监控第一次授权，是否获得到设备（因为很可能第一次授权了，但是获得不到设备，这里这样避免）
            // 多次 都没有获得设备，可能就是真没有摄像头，结束获取 camera
            int i = 0;
            while (WebCamTexture.devices.Length <= 0 && i < 50)
            {
                yield return new WaitForEndOfFrame();
                i++;
            }
            WebCamDevice[] devices = WebCamTexture.devices;//获取可用设备
            if (WebCamTexture.devices.Length <= 0)
            {
                warning.enabled = true;
                warning.text = "没有摄像头设备，请检查";
            }
            else
            {
                string deviceName = devices[0].name;
                WebCamTex = new WebCamTexture(deviceName, maxl, maxl == Screen.height ? Screen.width : Screen.height, 60)
                {
                    wrapMode = TextureWrapMode.Repeat
                };
                // 渲染到 UI 或者 游戏物体上
                if (rawImage != null)
                {
                    rawImage.texture = WebCamTex;
                }


                WebCamTex.Play();
                print("play");
            }

        }
        else
        {
            Debug.LogError("未获得读取摄像头权限");
        }
    }
}
