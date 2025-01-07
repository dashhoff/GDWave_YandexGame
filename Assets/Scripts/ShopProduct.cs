using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YG;

public class ShopProduct : MonoBehaviour
{
    public int Id;
    public int Price = 10;

    public Image SkinImage;

    [SerializeField] private TMP_Text _mainText;

    [SerializeField] private bool _purchased = false;
    [SerializeField] private bool _equipped = false;

    [SerializeField] private GameObject _buyButton;
    [SerializeField] private GameObject _equipButton;
    [SerializeField] private GameObject _equippedImage;

    public void Init()
    {
        if (GameSettings.Instance.IntOpenSkins[Id] == 1)
            _purchased = true;
        else
            _equipped = false;

        if (GameSettings.Instance.PlayerSkinId == Id)
            _equipped = true;
        else
            _equipped = false;

        UpdateButtons();
    }

    public void UpdateButtons()
    {
        if (_purchased)
        {
            _buyButton.SetActive(false);
            _equipButton.SetActive(true);
            _equippedImage.SetActive(false);
        }

        if (_equipped)
        {
            _buyButton.SetActive(false);
            _equipButton.SetActive(false);
            _equippedImage.SetActive(true);
        }

        UpdateText();
    }

    public void SetPurchased(bool value)
    {
        _purchased = value;

        UpdateText();
    }

    public void SetEquip(bool value)
    {
        _equipped = value;

        UpdateText();
    }

    public void UpdateText()
    {
        if (YandexGame.savesData.language == "ru")
        {
            if (!_purchased)
            {
                _mainText.text = "÷≈Õ¿: " + Price;
                return;
            }
            if (_purchased && !_equipped)
            {
                _mainText.text = "¬€¡–¿“‹";
                return;
            }
            if (_equipped)
            {
                _mainText.text = "¬€¡–¿ÕŒ";
                return;
            }
        }
        else
        {
            if (!_purchased)
            {
                _mainText.text = "PRICE: " + Price;
                return;
            }
            if (_purchased && !_equipped)
            {
                _mainText.text = "EQUIP";
                return;
            }
            if (_equipped)
            {
                _mainText.text = "EQUIPED" + Price;
                return;
            }
        }
    }
}
