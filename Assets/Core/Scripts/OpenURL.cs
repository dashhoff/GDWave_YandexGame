using UnityEngine;
using YG;

public class OpenURL : MonoBehaviour
{
    [SerializeField] private string _ruLink;
    [SerializeField] private string _enLink;

    public void OpenLink()
    {
        if (YandexGame.EnvironmentData.domain == "ru")
            Application.OpenURL(_ruLink);
        else
            Application.OpenURL(_enLink);
    }
}
