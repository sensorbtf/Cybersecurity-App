using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class CAPTCHAManager : MonoBehaviour
{
    public LoginManager _loginManager;
    [SerializeField] public CaptchaSet[] _captchaSets;

    private int _selected = -1;

    public Image Img1;
    public Image Img2;
    public Image Img3;

    public TextMeshProUGUI _captchaState;

    // Start is called before the first frame update
    void Start()
    {
        _loginManager.OnBackToLoadingPanel += ReloadSprites;
    }

    private void ReloadSprites()
    {
        _loginManager.HandleCaptcha(false);
        _captchaState.text = "";

        var select = Random.Range(0, _captchaSets.Length);
        if (select == _selected)
        {
            for (int i = 0; i < 50; i++)
            {
                select = Random.Range(0, _captchaSets.Length);
                if (select != _selected)
                {
                    _selected = select;
                    break;
                }
            }
        }

        var setOfSprites = _captchaSets[select];

        Img1.sprite = setOfSprites.ThreeSprites[0].Sprite;
        Img2.sprite = setOfSprites.ThreeSprites[1].Sprite;
        Img3.sprite = setOfSprites.ThreeSprites[2].Sprite;

        setOfSprites.ThreeSprites[0].SpriteButton.onClick.RemoveAllListeners();
        setOfSprites.ThreeSprites[1].SpriteButton.onClick.RemoveAllListeners();
        setOfSprites.ThreeSprites[2].SpriteButton.onClick.RemoveAllListeners();

        setOfSprites.ThreeSprites[0].SpriteButton.onClick.AddListener(delegate
        {
            CheckChoice(setOfSprites.ThreeSprites[0].IsRight);
        });

        setOfSprites.ThreeSprites[1].SpriteButton.onClick.AddListener(delegate
        {
            CheckChoice(setOfSprites.ThreeSprites[1].IsRight);
        });

        setOfSprites.ThreeSprites[2].SpriteButton.onClick.AddListener(delegate
        {
            CheckChoice(setOfSprites.ThreeSprites[2].IsRight);
        });
    }

    private void CheckChoice(bool p_isRight)
    {
        _loginManager.HandleCaptcha(p_isRight);

        if (!p_isRight)
        {
            ReloadSprites();
        }

        _captchaState.text = p_isRight ? "Captcha Correct" : "Wrong Captcha. Try Again";
    }

    [Serializable]
    public struct CaptchaSet
    {
        public SpriteBool[] ThreeSprites;
    }

    [Serializable]
    public struct SpriteBool
    {
        public Sprite Sprite;
        public Button SpriteButton;
        public bool IsRight;
    }
}