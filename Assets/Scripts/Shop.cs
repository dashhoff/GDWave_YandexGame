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

    public void TrySelectOrBuyObject(int id)
    {
        if (GameSettings.Instance.OpenSkins[id] && GameSettings.Instance.PlayerSkinId != id)
        {
            EquipSkin(id);
        }
        if (!GameSettings.Instance.OpenSkins[id] && GameSettings.Instance.Money >= _products[id].Price)
        {
            BuySkin(id);
        }
        else
        {
            AudioController.Instance.PlayErrorSound();
        }
    }

    public void BuySkin(int id)
    {
        GameSettings.Instance.Money -= _products[id].Price;
        GameSettings.Instance.PlayerSkinId = id;
        GameSettings.Instance.OpenSkins[id] = true;
        GameSettings.Instance.Save();

        UpdateProsduct();

        EquipSkin(id);

        AudioController.Instance.PlayBuySound();
    }

    public void EquipSkin(int id)
    {
        for (int i = 0; i < _products.Length; i++)
        {
            _products[i].SetEquip(false);
        }

        _products[id].SetPurchased(true);

        _products[id].SetEquip(true);

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
