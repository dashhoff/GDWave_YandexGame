
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private bool _isMoving = false;

    [SerializeField] private float _speed;

    private Vector2 direction;

    [SerializeField] private Rigidbody2D _rb2D;

    [SerializeField] private SpriteRenderer _playerSpriteRenderer;

    private void Start()
    {
        direction = new Vector2(1, -1).normalized;
    }

    private void FixedUpdate()
    {

    }

    private void Update()
    {
        Move();
    }

    private void OnEnable()
    {
        EventManager.GameStarted += StartMovement;

        EventManager.GameStoped += StopMovement;
    }

    private void OnDisable()
    {
        EventManager.GameStarted -= StartMovement;

        EventManager.GameStoped -= StopMovement;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Collision!");

        if (collision.gameObject.CompareTag("Trap"))
        {
            EventManager.OnDefeated();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Trigger!");

        if (collision.gameObject.CompareTag("LevelLoader"))
        {
            Debug.Log("LevelLoad");

            EventManager.OnCompleteLevel();
        }

        if (collision.gameObject.CompareTag("Coin"))
        {
            Debug.Log("CoinCollected");

            EventManager.OnCollectCoin();
        }
    }

    private void StartMovement()
    {
        _isMoving = true;
    }

    private void StopMovement()
    {
        _isMoving = false;
    }

    private void Move()
    {
        if (!_isMoving) return;

        transform.Translate(direction * _speed * Time.deltaTime);

        if (Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space))
        {
            direction = new Vector2(1, 1).normalized;

            _playerSpriteRenderer.transform.rotation = Quaternion.Euler(0, 0, -45);
        }
        else
        {
            direction = new Vector2(1, -1).normalized;

            _playerSpriteRenderer.transform.rotation = Quaternion.Euler(0, 0, -135);
        }
    }

    public void SetSpeed(float newSpeed)
    {
        _speed = newSpeed;
    }
}
