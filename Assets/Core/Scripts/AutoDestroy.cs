using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    [SerializeField] private float _delay;

    private void Start()
    {
        Invoke("Destroy", _delay);
    }

    private void Destroy()
    {
        Destroy(gameObject);
    }
}
