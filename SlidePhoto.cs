using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SlidePhoto : MonoBehaviour
{

    public GameObject photoPrefab;
    public Image bg;

    LoadMsg load;
    /// <summary>
    /// 照片Sprite
    /// </summary>
    List<Sprite> photoSpr;
    /// <summary>
    /// 图片组件
    /// </summary>
    List<Image> photos;

    int nowLastIndex = 0;
    int nextSpriteIndex = 0;


    void Start()
    {
        load = new LoadMsg();

        photoSpr = new List<Sprite>();
        photos = new List<Image>();


        InitialSlide();
    }

    void Update()
    {

    }

    /// <summary>
    /// 初始化
    /// </summary>
    void InitialSlide()
    {
        bg.sprite = load.GetSprite(Application.dataPath + @"\menu.jpg", bg);
        foreach (string s in load.GetJPGPath(@"\Photos"))
        {
            photoSpr.Add(load.GetSprite(s, photoPrefab.GetComponent<Image>()));
        }

        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            GameObject father = gameObject.transform.GetChild(i).gameObject;
            photos.Add(father.transform.GetChild(0).GetComponent<Image>());
        }

        nextSpriteIndex = gameObject.transform.childCount;
        nowLastIndex = gameObject.transform.childCount - 1;

        if (photoSpr.Count < photos.Count)
        {
            foreach (Image img in photos)
            {
                int ix = Random.Range(0, photoSpr.Count);
                img.sprite = photoSpr[ix];
            }
            return;
        }
        for (int i = 0; i < photos.Count; i++)
        {
            photos[i].sprite = photoSpr[i];
        }
    }

    /// <summary>
    /// 移动循环图片位置
    /// </summary>
    /// <param name="i">被移动的图片序号</param>
    public void MovePhoto(int i)
    {

        photos[i].transform.localPosition = new Vector2(0, photos[nowLastIndex].transform.localPosition.y - 120);
        if (photoSpr.Count <= photos.Count)
        {
            photos[i].sprite = photoSpr[Random.Range(0, photoSpr.Count)];
        }
        else
        {
            photos[i].sprite = photoSpr[nextSpriteIndex];
        }
        if (nowLastIndex < photos.Count - 1) nowLastIndex ++; else nowLastIndex = 0;
        if (nextSpriteIndex < photoSpr.Count - 1) nextSpriteIndex++; else nextSpriteIndex = 0;
    }

    /// <summary>
    /// 更新照片
    /// </summary>
    public void UpdataPhoto()
    {
        photoSpr.Add(load.GetSprite(CamManager.fileName, photoPrefab.GetComponent<Image>()));
    }
}
