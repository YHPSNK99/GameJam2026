using UnityEngine;

public class PlaySceneMusic : MonoBehaviour
{
    [SerializeField] private AudioClip sceneMusic;

    private void Start()
    {
        if (MusicManager.Instance && sceneMusic)
            MusicManager.Instance.PlayMusic(sceneMusic);
    }
}
