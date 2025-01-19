using UnityEngine;
using YG;

public class DonateInfo : ScriptableObject
{
    #region Fields
    [SerializeField] private string _purchaseKey;
    [SerializeField] private string _titleRU;
    [SerializeField] private string _titleEN;
    [SerializeField] private Sprite _icon;
    #endregion

    #region Properties
    public string PurchaseKey => _purchaseKey;
#if !UNITY_EDITOR
    public string Price => YandexGame.PurchaseByID(_purchaseKey).priceValue;
#else
    public string Price = "100";
#endif
    public Sprite Icon => _icon;
    public string TitleRU => _titleRU;
    public string TitleEN => _titleEN;
#endregion
}
