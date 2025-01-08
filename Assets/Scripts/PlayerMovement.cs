using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement Instance;

    [SerializeField] private bool _godMode = false;

    [SerializeField] private bool _isMoving = false;

    [SerializeField] private float _speed = 20f;

    [SerializeField] private float _1Speed = 10f;
    [SerializeField] private float _2Speed = 20f;
    [SerializeField] private float _3Speed = 30f;
    [SerializeField] private float _4Speed = 40f;

    private Vector2 direction;

    [SerializeField] private Rigidbody2D _rb2D;

    [SerializeField] private SpriteRenderer _playerSpriteRenderer;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        direction = new Vector2(1, -1).normalized;
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
            if (_godMode) return;

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
            Coin coin = collision.gameObject.GetComponent<Coin>();
            if (coin.Collected) return;

            coin.Collected = true;

            Debug.Log("CoinCollected");

            EventManager.OnCollectCoin();
        }

        if (collision.gameObject.CompareTag("1Speed"))
        {
            Debug.Log("1 Speed player");
            SetSpeed(_1Speed);
        }
        if (collision.gameObject.CompareTag("2Speed"))
        {
            Debug.Log("2 Speed player");
            SetSpeed(_2Speed);
        }
        if (collision.gameObject.CompareTag("3Speed"))
        {
            Debug.Log("3 Speed player");
            SetSpeed(_3Speed);
        }
        if (collision.gameObject.CompareTag("4Speed"))
        {
            Debug.Log("4 Speed player");
            SetSpeed(_4Speed);
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

    public void StartGodMode()
    {
        StartCoroutine(GodModeCoroutine());
    }

    private IEnumerator GodModeCoroutine()
    {
        _godMode = true;

        yield return new WaitForSeconds(3f);

        _godMode = false;
    }
}
