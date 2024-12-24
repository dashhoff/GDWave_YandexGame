using UnityEngine;

public class Pause : MonoBehaviour
{

    public void PauseOn()
    {
        Time.timeScale = 0.0f;
    }

    public void PauseOff()
    {
        Time.timeScale = 1.0f;
    }
}
