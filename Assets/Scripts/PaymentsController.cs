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

    // ������������� �� ������ ��������/���������� �������
    private void OnEnable()
    {
        YandexGame.PurchaseSuccessEvent += SuccessPurchased;
        YandexGame.PurchaseFailedEvent += FailedPurchased; // �������������
    }

    private void OnDisable()
    {
        YandexGame.PurchaseSuccessEvent -= SuccessPurchased;
        YandexGame.PurchaseFailedEvent -= FailedPurchased; // �������������
    }

    // ������� ������� ���������, ����� �����
    private void SuccessPurchased(string id)
    {
        // ��� ��� ��� ��������� �������. ��������:
        if (id == "1")
            GameManager.Instance.Money += 50;
        else if (id == "2")
            GameManager.Instance.Money += 150;
        else if (id == "3")
            GameManager.Instance.Money += 250;

        GameManager.Instance.Save();

        UIController.Instance.UpdateMoneyText();
    }

    // ������� �� ���� �����������
    void FailedPurchased(string id)
    {
        // ��������, ����� ������� ����������� � ������������ �������.
    }
}