using ZXing;
using ZXing.QrCode;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UI;

public class UploadPhoto : MonoBehaviour
{
    public static UploadPhoto instance;

    Texture2D tex;
    public RawImage QRImg;
    ServiceReference1.Service1 client = new ServiceReference1.Service1();

    public static bool isSave;
    bool isDone;
    string fname;
    bool offline;

    void Start()
    {
        instance = this;
        tex = new Texture2D(256, 256);

        Manager.instance._CamHide += (bool b) => { QRImg.gameObject.SetActive(b); };
        Manager.instance._CamHide += DeletePhoto;
    }

    void Update()
    {
        if (isSave)
        {
            LoadPhoto(CamManager.fileName);
            isSave = false;
        }
        if (isDone)
        {
            //SlidePhoto.instance.UpdataPhoto();

            Manager.instance.timer = Manager.instance.setTimer;

            QRImg.gameObject.SetActive(true);
            var color32 = Encode("http://maia.yoia.cn/images/" + fname, tex.width, tex.height);
            tex.SetPixels32(color32);
            tex.Apply();
            //生成的二维码图片附给RawImage  
            QRImg.texture = tex;
            isDone = false;
        }
        print(offline);
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            offline = true;
        }
        else
        {
            offline = false;
        }
    }


    public void LoadPhoto(string filename)
    {
        var th = new System.Threading.Thread(() =>
        {
            //要上传的文件  
            FileInfo finfo;
            if (filename != null) finfo = new FileInfo(filename); else { print("name is null"); return; }
            if (finfo.Exists && !offline)
            {
                //创建文件
                bool isCreate = client.CreateFile(finfo.Name);
                if (isCreate)
                {
                    #region 读取文件并分块上传
                    FileStream fs = finfo.Open(FileMode.Open, FileAccess.Read, FileShare.None);
                    int size = (int)fs.Length;
                    int bufferSize = 1024 * 512;
                    int count = (int)Math.Ceiling((double)size / (double)bufferSize);
                    for (int i = 0; i < count; i++)
                    {
                        int readSize = bufferSize;
                        if (i == count - 1)
                            readSize = size - bufferSize * i;
                        byte[] buffer = new byte[readSize];
                        fs.Read(buffer, 0, readSize);
                        client.Append(finfo.Name, buffer);
                        //PBar1.Dispatcher.BeginInvoke((Action)delegate { PBar1.Value = i * 100 / count; });
                    }
                    fs.Close();
                    #endregion

                    //上传完成后效验
                    string md5 = GetMD5(finfo.FullName);
                    bool isVerify = client.Verify(finfo.Name, md5);

                    if (isVerify)
                    {
                        fname = finfo.Name;
                        print("prepare save" + CamManager.fileName);
                        isDone = true;
                    }
                    else
                        print("MD5码效验失败！");
                }
                else
                    print("创建失败");
            }
            else
                print("图片上传失败");
        });
        th.Start();
    }

    public void DeletePhoto(bool b)
    {
        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            client.DeleteFile(CamManager.fileName);
        }
    }

    /// <summary>
    /// 生成二维码
    /// </summary>
    /// <param name="textForEncoding">二维码链接</param>
    /// <param name="width">宽</param>
    /// <param name="height">高</param>
    /// <returns></returns>
    private static Color32[] Encode(string textForEncoding, int width, int height)
    {
        var writer = new BarcodeWriter
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new QrCodeEncodingOptions
            {
                Height = height,
                Width = width
            }
        };
        return writer.Write(textForEncoding);
    }

    private string GetMD5(string fileName)
    {
        FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.None);
        MD5CryptoServiceProvider p = new MD5CryptoServiceProvider();
        byte[] md5buffer = p.ComputeHash(fs);
        fs.Flush();
        fs.Close();
        string md5Str = "";
        foreach (var b in md5buffer)
            md5Str += b.ToString("x2");
        return md5Str;
    }

    private void OnApplicationQuit()
    {
        DeletePhoto(true);
    }
}
