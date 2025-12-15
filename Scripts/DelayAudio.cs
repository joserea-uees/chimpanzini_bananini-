using UnityEngine;

public class DelayAudio : MonoBehaviour
{
    AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.PlayDelayed(2f);
    }
}
