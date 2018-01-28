using UnityEngine;
using System.Collections;

public class Music : MonoBehaviour
{
  [SerializeField]
  private SoundBank _musicIntro = null;

  [SerializeField]
  private SoundBank _musicLoop = null;

  private IEnumerator Start()
  {
    AudioManager.AudioInstance instance = AudioManager.Instance.PlaySound(gameObject, _musicIntro);

    while (instance.AudioSource.isPlaying)
    {
      yield return null;
    }

    AudioManager.Instance.PlaySound(gameObject, _musicLoop);
  }
}