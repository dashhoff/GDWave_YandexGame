
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

    private void Victory()
    {
        EventManager.OnVictoried();
    }

    private void Death()
    {
        EventManager.OnDefeated();
    }
}
