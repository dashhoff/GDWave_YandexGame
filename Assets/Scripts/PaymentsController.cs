using UnityEngine;
using YG;

public class PaymentsController : MonoBehaviour
{
    public static PaymentsController Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // Подписываемся на ивенты успешной/неуспешной покупки
    private void OnEnable()
    {
        YandexGame.PurchaseSuccessEvent += SuccessPurchased;
        YandexGame.PurchaseFailedEvent += FailedPurchased; // Необязательно
    }

    private void OnDisable()
    {
        YandexGame.PurchaseSuccessEvent -= SuccessPurchased;
        YandexGame.PurchaseFailedEvent -= FailedPurchased; // Необязательно
    }

    // Покупка успешно совершена, выдаём товар
    private void SuccessPurchased(string id)
    {
        // Ваш код для обработки покупки. Например:
        if (id == "1")
            GameSettings.Instance.SkipAd = true;
        else if (id == "2")
            GameSettings.Instance.Money += 100;
        else if (id == "3")
            GameSettings.Instance.Money += 5000;

        GameSettings.Instance.Save();

        UIController.Instance.UpdateMoneyText();
    }

    // Покупка не была произведена
    void FailedPurchased(string id)
    {
        // Например, можно открыть уведомление о неуспешности покупки.
        UIController.Instance.UpdateMoneyText();
    }
}
