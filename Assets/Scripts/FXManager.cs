using UnityEngine;

public class FXManager : MonoBehaviour
{
    [Space(20f)]
    [SerializeField] private GameObject _levelCompletedParticle;
    [SerializeField] private GameObject _moneyParticle;
    [SerializeField] private GameObject _defeatParticle;


    private void OnEnable()
    {
        EventManager.Defeated += DefeatParticlePlay;

        EventManager.LevelCompleted += LevelCompletedParticle;

    }

    private void OnDisable()
    {
        EventManager.Defeated -= DefeatParticlePlay;

        EventManager.LevelCompleted -= LevelCompletedParticle;

    }

    private void LevelCompletedParticle()
    {
        Instantiate(_levelCompletedParticle, PlayerMovement.Instance.transform.position, Quaternion.identity);
    }

    private void DefeatParticlePlay()
    {
        Instantiate(_defeatParticle, PlayerMovement.Instance.transform.position, Quaternion.identity);
    }
}
