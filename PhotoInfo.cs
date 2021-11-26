using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhotoInfo : MonoBehaviour
{
    public int _index;
    public Sprite _sprite;

    void Start()
    {
    }

    void Update()
    {
        transform.Translate(Vector2.up * Time.deltaTime * 1);

        if (transform.localPosition.y > Screen.height)
        {
            //SlidePhoto.instance.MovePhoto(_index);
        }
    }
}
