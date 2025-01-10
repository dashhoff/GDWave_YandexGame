using UnityEngine;

public class Shop : MonoBehaviour
{
    [SerializeField] private ShopProduct[] _products;

    [SerializeField] private Sprite[] _skinsImages;

    private void Start()
    {
        for (int i = 0; i < _products.Length; i++)
        {
            _products[i].Id = i;
            _products[i].SkinImage.sprite = _skinsImages[i];
        }

        _products[GameSettings.Instance.PlayerSkinId].SetPurchased(true);
        _products[GameSettings.Instance.PlayerSkinId].SetEquip(true);

        UpdateProsduct();
    }

    public void Init()
    {
        for (int i = 0; i < _products.Length; i++)
        {
            _products[i].Id = i;
            _products[i].SkinImage.sprite = _skinsImages[i];
        }

        _products[GameSettings.Instance.PlayerSkinId].SetPurchased(true);
        _products[GameSettings.Instance.PlayerSkinId].SetEquip(true);

        UpdateProsduct();
    }

    public void BuySkin(int id)
    {
        if (GameSettings.Instance.Money < _products[id].Price)
        {
            AudioController.Instance.PlayErrorSound();
            return;
        }

        GameSettings.Instance.Money -= _products[id].Price;
        GameSettings.Instance.PlayerSkinId = id;
        GameSettings.Instance.IntOpenSkins[id] = 1;
        GameSettings.Instance.Save();

        UpdateProsduct();

        EquipSkin(id);

        AudioController.Instance.PlayBuySound();

        UIController.Instance.UpdateMoneyText();
    }

    public void EquipSkin(int id)
    {
        GameSettings.Instance.PlayerSkinId = id;
        GameSettings.Instance.Save();

        UpdateProsduct();

        AudioController.Instance.PlayEquipSound();
    }

    public void UpdateProsduct()
    {
        for (int i = 0; i < _products.Length; i++)
        {
            _products[i].Init();

            _products[i].UpdateButtons();
        }
    }
}
