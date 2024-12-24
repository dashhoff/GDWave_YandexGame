using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [Space(20f)]
    [SerializeField] private AudioSource _correctMergeSound;
    [SerializeField] private AudioSource _failMergeSound;

    [Space(20f)]
    [SerializeField] private AudioSource _confettiSound;

    [Space(20f)]
    [SerializeField] private AudioSource _victorySound;
    [SerializeField] private AudioSource _defeatSound;

    private void OnEnable()
    {
        EventManager.CorrectedMerge += CorrectSoundPlay;
        EventManager.FailedMerge += FailSoundPlay;

        EventManager.Victoried += VictorySoundPlay;
        EventManager.Defeated += DefeatSoundPlay;
    }

    private void OnDisable()
    {
        EventManager.CorrectedMerge -= CorrectSoundPlay;
        EventManager.FailedMerge -= FailSoundPlay;

        EventManager.Victoried -= VictorySoundPlay;
        EventManager.Defeated -= DefeatSoundPlay;
    }

    private void CorrectSoundPlay() => _correctMergeSound.Play();

    private void FailSoundPlay() => _failMergeSound.Play();

    private void VictorySoundPlay()
    {
        _victorySound.Play();

        _confettiSound.Play();
    }

    private void DefeatSoundPlay() => _defeatSound.Play();
}
