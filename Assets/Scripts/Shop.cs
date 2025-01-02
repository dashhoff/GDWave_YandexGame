using UnityEngine;
using UnityEngine.UI;
using YG;

public class Shop : MonoBehaviour
{
    [SerializeField] private GameObject[] _skinsUI;
    [SerializeField] private GameObject[] _priceUI;
    [SerializeField] private int[] _price;
        
    [SerializeField] private AudioSource _errorBuySound;
    [SerializeField] private AudioSource _buySound;
    [SerializeField] private AudioSource _selectSound;

    private void Start()
    {
        for (int i = 0; i < _priceUI.Length; i++)
        {
            if (!YandexGame.savesData.OpenSkins[i])
            {
                _priceUI[i].SetActive(true);
            }
        }
            
        for (int i = 0; i < _skinsUI.Length; i++)
        {
            if (YandexGame.savesData.OpenSkins[i])
            {
                Image image = _skinsUI[i].GetComponent<Image>();
                image.color = Color.white;
            }
        }
    }

    public void SelectOrBuyObject(int id)
    {
        if (YandexGame.savesData.OpenSkins[id])
        {
            _selectSound.Play();
                
            YandexGame.savesData.PlayerSkinId = id;
            YandexGame.SaveProgress();
        }
        else if (YandexGame.savesData.Money > _price[id])
        {
            _buySound.Play();
                
            Image image = _skinsUI[id].GetComponent<Image>();
            image.color = Color.white;
                
            _priceUI[id].SetActive(false);
                
            YandexGame.savesData.Money -= _price[id];
            YandexGame.savesData.OpenSkins[id] = true;
            YandexGame.savesData.PlayerSkinId = id;
            YandexGame.SaveProgress();
        }
        else
        {
            _errorBuySound.Play();
        }
    }
}
