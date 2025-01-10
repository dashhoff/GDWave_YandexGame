using UnityEngine;

public class PlayerSkin : MonoBehaviour
{
    [SerializeField] private Sprite[] _skins;

    [SerializeField] private SpriteRenderer _playerSpriteRenderer;

    private void Start()
    {
        Init();
    }

    public void Init()
    {
        SetSkin();
    }

    public void SetSkin()
    {
        _playerSpriteRenderer.sprite = _skins[GameSettings.Instance.PlayerSkinId];
    }
}
