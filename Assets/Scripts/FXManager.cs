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
        EventManager.CorrectedMerge += CorrectParticlePlay;
        EventManager.FailedMerge += FailParticlePlay;

        EventManager.Victoried += VictorySoundPlay;
    }

    private void OnDisable()
    {
        EventManager.CorrectedMerge -= CorrectParticlePlay;
        EventManager.FailedMerge -= FailParticlePlay;

        EventManager.Victoried -= VictorySoundPlay;
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

    private void VictorySoundPlay()
    {
        for (int i = 0; i < _confettiParticles.Length; i++)
        {
            _confettiParticles[i].Play();
        }
    }
}
