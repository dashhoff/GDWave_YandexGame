
using UnityEngine;


public class Player : MonoBehaviour
{
    [SerializeField] private PlayerMovement _playerMovement;

    private void Start()
    {
        
    }

    private void FixedUpdate()
    {
        
    }


    private void Update()
    {
        
    }

    private void OnEnable()
    {
        EventManager.Defeated += Death;
    }

    private void OnDisable()
    {
        EventManager.Defeated -= Death;
    }

    private void Victory()
    {

    }

    private void Death()
    {
        gameObject.SetActive(false);
    }
}
