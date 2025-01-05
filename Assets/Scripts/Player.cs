using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance;

    [SerializeField] private PlayerMovement _playerMovement;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void OnEnable()
    {
        EventManager.Defeated += Death;
    }

    private void OnDisable()
    {
        EventManager.Defeated -= Death;
    }

    private void Death()
    {
        gameObject.SetActive(false);
    }
}
