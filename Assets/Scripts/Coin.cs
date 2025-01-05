using UnityEngine;
using DG.Tweening;

public class Coin : MonoBehaviour
{
    [SerializeField] private Vector3 _endTrasform = new Vector3(1.1f, 1.1f, 1.1f);
    [SerializeField] private float _timeToEnd = 0.2f;

    [SerializeField] private float _timeToZero = 0.2f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
            DestroyAnim();
    }

    private void DestroyAnim()
    {
        DOTween.Sequence()
            .Append(transform.DOScale(_endTrasform, _timeToEnd))
            .Append(transform.DOScale(0, _timeToZero))
            .OnComplete(()=>
            {
                Destroy(gameObject);
            });
    }
}
