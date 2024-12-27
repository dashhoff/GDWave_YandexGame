using UnityEngine;

public class FXManager : MonoBehaviour
{
    [Space(20f)]
    [SerializeField] private ParticleSystem[] _correctMergeParticle;
    [SerializeField] private ParticleSystem[] _failMergeParticle;

    [Space(20f)]
    [SerializeField] private ParticleSystem[] _confettiParticles;

    private void OnEnable()
    {
        EventManager.Victoried += VictoryParticlePlay;
    }

    private void OnDisable()
    {
        EventManager.Victoried -= VictoryParticlePlay;
    }

    private void CorrectParticlePlay()
    {
        for (int i = 0; i < _correctMergeParticle.Length; i++)
        {
            _correctMergeParticle[i].Play();
        }
    }

    private void FailParticlePlay()
    {
        for (int i = 0; i < _failMergeParticle.Length; i++)
        {
            _failMergeParticle[i].Play();
        }
    }

    private void VictoryParticlePlay()
    {
        for (int i = 0; i < _confettiParticles.Length; i++)
        {
            _confettiParticles[i].Play();
        }
    }
}
