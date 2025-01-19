using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YG;

public class CustomPurchase : MonoBehaviour
{
    [SerializeField] private int _id;

    [SerializeField] private bool consumed;

    [SerializeField] private TMP_Text _priceText;

    [SerializeField] private TMP_Text _titleText;
    [SerializeField] private string _ruTitle;
    [SerializeField] private string _enTitle;

    [SerializeField] private TMP_Text _descriptionText;
    [SerializeField] private string _ruDescription;
    [SerializeField] private string _enDescription;

    [SerializeField] private Button _buyButton;

    private void FixedUpdate()
    {
        UpdateText();
    }

    public void UpdateText()
    {
        if (YandexGame.savesData.language == "ru")
        {
            _priceText.text = "жемю: " + YandexGame.PurchaseByID(_id.ToString()).priceValue + YandexGame.purchases[0].priceCurrencyCode;

            _titleText.text = _ruTitle;

            _descriptionText.text = _ruDescription;
        }
        else
        {
            _priceText.text = "PRICE: " + YandexGame.PurchaseByID(_id.ToString()).priceValue + YandexGame.purchases[0].priceCurrencyCode;

            _titleText.text = _enTitle;

            _descriptionText.text = _enDescription;
        }

        if (consumed && GameSettings.Instance.SkipAd)
        {
            if (YandexGame.savesData.language == "ru")
                _priceText.text = "йсокемн";
            else
                _priceText.text = "PURCHASED";

            _buyButton.enabled = false;
        }
    }
}
