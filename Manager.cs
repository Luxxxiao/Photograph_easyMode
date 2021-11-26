using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using System;
using UnityEngine.Events;

public class Manager : MonoBehaviour
{
    public static Manager instance;

    public delegate void Cam(bool b);
    public event Cam _CamShow;
    public event Cam _CamHide;

    int coverIndex = 0;
    public Image bg;
    /// <summary>
    /// 中间图片
    /// </summary>
    public Image coverImg;
    /// <summary>
    /// 左边图片（上一张）
    /// </summary>
    public Image last_coverImg;
    /// <summary>
    /// 右边图片（下一张）
    /// </summary>
    [SerializeField]private Image coverImg_next;
    public GameObject coverParent;
    //public GameObject photoParent;
    //public Animator camAnim;
    public Transform btnParent;


    //public Button closeShowImgBtn;
    public GameObject showImg;
    //public GameObject front;

    /// <summary>
    /// 前景图
    /// </summary>
    [HideInInspector]public List<Sprite> coverSpr;
    List<string> imgPath;
    LoadMsg load;

    /// <summary>
    /// 返回首页倒计时
    /// </summary>
    public float timer = 60, setTimer = 60;


    void Start()
    {
        instance = this;
        _CamShow += SetCamShow;
        _CamHide += SetCamHide;

        load = new LoadMsg();
        imgPath = load.GetPNGPath(@"\Image");

        coverSpr = new List<Sprite>();
        foreach(string s in imgPath)
        {
            coverSpr.Add(load.GetSprite(s, coverImg));
        }
        
        coverImg.sprite = coverSpr[0];
        last_coverImg.sprite = coverSpr[coverSpr.Count-1];
        coverImg_next.sprite = coverSpr[1];

        //closeShowImgBtn.onClick.AddListener(new UnityAction(() => { _CamHide(false); }));
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0) BackBtn();
    }

    public void SetCamShow(bool b)
    {
        btnParent.SetAsFirstSibling();
        //camAnim.SetBool("isTake", b);
        //coverImg.GetComponent<Animator>().SetBool("isTake", b);

    }

    public void SetCamHide(bool b)
    {
        CamManager.instance.showImage.SetActive(b);
        //CamManager.instance.showImage.GetComponent<Animator>().SetBool("isShow", b);
        //closeShowImgBtn.gameObject.SetActive(false);
    }

    #region Buttons
    public void StartBtn()
    {
        //front.SetActive(false);
    }

    //上一张
    public void LastBtn()
    {
        //PhotosOut();
        timer = setTimer;
        coverParent.transform.DOMoveX(17.8f, 0.5f);
        Invoke("LastBtnAnim", 0.6f);
    }
    void LastBtnAnim()
    {
        coverImg_next.sprite = coverSpr[coverIndex];
        if (coverIndex > 0) coverIndex--; else coverIndex = coverSpr.Count - 1;
        coverImg.sprite = coverSpr[coverIndex];
        if (coverIndex > 0) last_coverImg.sprite = coverSpr[coverIndex - 1]; else last_coverImg.sprite = coverSpr[coverSpr.Count - 1];
        coverParent.transform.position = new Vector3(0, 0, 90);
    }
    //下一张
    public void NextBtn()
    {
        //PhotosOut();
        timer = setTimer;
        if (showImg.activeSelf) _CamHide(false);
        coverParent.transform.DOMoveX(-17.8f, 0.5f);
        Invoke("NextBtnAnim", 0.6f);
    }
    void NextBtnAnim()
    {
        last_coverImg.sprite = coverSpr[coverIndex];
        if (coverIndex < coverSpr.Count - 1) coverIndex++; else coverIndex = 0;
        coverImg.sprite = coverSpr[coverIndex];
        if (coverIndex < coverSpr.Count - 1) coverImg_next.sprite = coverSpr[coverIndex + 1]; else coverImg_next.sprite = coverSpr[0];
        coverParent.transform.position = new Vector3(0, 0, 90);
    }
    //拍照
    public void TakePhoto()
    {
        //PhotosIn();
        timer = setTimer;
        if (showImg.activeSelf) _CamHide(false);
        _CamShow(true);
    }
    //上传二维码
    public void QRcode()
    {
        UploadPhoto.isSave = true;
    }
    //返回首页
    void BackBtn()
    {
        SceneManager.LoadScene(0);
        CamManager.instance.WebCamTex.Stop();
        //front.SetActive(true);
    }
    #endregion

    //void PhotosOut()
    //{
    //    photoParent.transform.SetParent(GameObject.Find("Canvas").transform);
    //    photoParent.transform.SetAsLastSibling();
    //}
    //void PhotosIn()
    //{
    //    photoParent.transform.SetParent(coverParent.transform);
    //    photoParent.transform.SetSiblingIndex(2);
    //}
}

/// <summary>
/// 获取外部文件
/// </summary>
public class LoadMsg
{
    public Sprite GetSprite(string path,Image img)
    {
        FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
        byte[] b = new byte[fs.Length];
        fs.Read(b, 0, (int)fs.Length);
        fs.Close();fs.Dispose();

        Texture2D tex = new Texture2D((int)img.preferredWidth, (int)img.preferredHeight);
        tex.LoadImage(b);

        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        return sprite;
    }

    public List<string> GetPNGPath(string path)
    {
        List<string> paths = new List<string>();
        path = Application.dataPath + path;
        var files = Directory.GetFiles(path,"*.png");
        foreach(var file in files)
        {
            paths.Add(file);
        }

        return paths;
    }

    public List<string> GetJPGPath(string path)
    {
        List<string> paths = new List<string>();
        path = Application.dataPath + path;
        var files = Directory.GetFiles(path, "*.jpg");
        foreach (var file in files)
        {
            paths.Add(file);
        }

        return paths;
    }

}
