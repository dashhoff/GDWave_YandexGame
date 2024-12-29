using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;
using YG;

public class UIController : MonoBehaviour
{
    public static UIController Instance;

    [Space(10f)]
    [SerializeField] private GameObject _mainPanel;

    [Space(20f)]
    [Header("UI Победы")]
    [SerializeField] private GameObject _victoryPanel;
    [SerializeField] private GameObject _victoryBackground;
    [SerializeField] private GameObject _victoryPopup;

    [Space(20f)]
    [Header("UI Поражения")]
    [SerializeField] private GameObject _defeatPanel;
    [SerializeField] private GameObject DefeatBackground;
    [SerializeField] private GameObject DefeatPopup;

    [Space(20f)]
    [Header("UI Паузы")]
    [SerializeField] private GameObject _pausePanel;
    [SerializeField] private GameObject _pauseBackground;
    [SerializeField] private GameObject _pausePopup;

    [Space(20f)]
    [Header("UI Магазина")]
    [SerializeField] private GameObject _shopPanel;
    [SerializeField] private GameObject _shopBackground;
    [SerializeField] private GameObject _shopPopup;

    [Space(20f)]
    [Header("UI Настроек")]
    [SerializeField] private GameObject _settingsPanel;
    [SerializeField] private GameObject _settingsBackground;
    [SerializeField] private GameObject _settingsPopup;

    [Space(20f)]
    [Header("Настройка времени")]
    [SerializeField] private float _openPopupDuration = 0.5f;
    [SerializeField] private float _closePopupDuration = 0.5f;
    [Space(5f)]
    [SerializeField] private float _fadeInDuration = 0.5f;
    [SerializeField] private float _fadeOutDuration = 0.5f;

    [Space(20f)]
    [Header("Текста")]
    [SerializeField] private TMP_Text _scoreText;
    [SerializeField] private TMP_Text _bestScoreText;
    [SerializeField] private TMP_Text _moneyText;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    private void Start()
    {
        UpdateMoneyText();
    }

    private void Update()
    {

    }

    private void OnEnable()
    {
        EventManager.Victoried += Victory;
        EventManager.Defeated += Defeat;
    }

    private void OnDisable()
    {
        EventManager.Victoried -= Victory;
        EventManager.Defeated -= Defeat;
    }

    private void Victory()
    {
        _victoryPanel.SetActive(true);

        FadeInPanel(_victoryBackground.GetComponent<Image>(), 1);
        OpenPopup(_victoryPopup, 1);
    }

    private void Defeat()
    {
        _defeatPanel.SetActive(true);

        FadeInPanel(DefeatBackground.GetComponent<Image>(), 0.5f);
        OpenPopup(DefeatPopup, 0.5f);
    }

    private void CorrectAnimation()
    {
        UpdateMoneyText();
    }

    public void UpdateMoneyText()
    {
        if (_moneyText == null) return;

        _moneyText.text = "" + GameSettings.Money;
    }

    public void OpenPanel(GameObject panel)
    {
        panel.SetActive(true);
    }

    public void ClosePanel(GameObject panel)
    {
        panel.SetActive(false);
    }

    public void OpenPopup(GameObject popup, float startDelay)
    {
        DOTween.Sequence()
            .SetUpdate(true)
            .AppendInterval(startDelay)
            .Append(popup.transform.DOScale(new Vector3(1.1f, 1.1f, 1.1f), _openPopupDuration))
            .Append(popup.transform.DOScale(new Vector3(1, 1, 1), 0.1f));
    }

    public void OpenPopup(GameObject popup)
    {
        DOTween.Sequence()
            .SetUpdate(true)
            .Append(popup.transform.DOScale(new Vector3(1.1f, 1.1f, 1.1f), _openPopupDuration))
            .Append(popup.transform.DOScale(new Vector3(1, 1, 1), 0.1f));
    }

    public void ClosePopup(GameObject popup, float startDelay)
    {
        DOTween.Sequence()
            .SetUpdate(true)
            .AppendInterval(startDelay)
            .Append(popup.transform.DOScale(new Vector3(1.1f, 1.1f, 1.1f), 0.1f))
            .Append(popup.transform.DOScale(new Vector3(0, 0, 0), _closePopupDuration));
    }

    public void ClosePopup(GameObject popup)
    {
        DOTween.Sequence()
            .SetUpdate(true)
            .Append(popup.transform.DOScale(new Vector3(1.1f, 1.1f, 1.1f), 0.1f))
            .Append(popup.transform.DOScale(new Vector3(0, 0, 0), _closePopupDuration));
    }

    public void FadeInPanel(Image panel, float startDelay)
    {
        panel.gameObject.SetActive(true);

        DOTween.Sequence()
            .SetUpdate(true)
            .AppendInterval(startDelay)
            .Append(panel.DOFade(0.45f, _fadeInDuration));
    }

    public void FadeInPanel(Image panel)
    {
        panel.gameObject.SetActive(true);

        DOTween.Sequence()
            .SetUpdate(true)
            .Append(panel.DOFade(0.45f, _fadeInDuration));
    }

    public void FageOutPanel(Image panel, float startDelay)
    {
        DOTween.Sequence()
            .SetUpdate(true)
            .AppendInterval(startDelay)
            .Append(panel.DOFade(0f, _fadeOutDuration))
            .OnComplete(() => ClosePanel(panel.gameObject));
    }

    public void FageOutPanel(Image panel)
    {
        DOTween.Sequence()
            .SetUpdate(true)
            .Append(panel.DOFade(0f, _fadeOutDuration))
            .OnComplete(() => ClosePanel(panel.gameObject));
    }
}
