using UnityEngine;

public class CollisionSound : MonoBehaviour
{
  [SerializeField]
  private SoundBank _collisionSound = null;

  private void OnCollisionEnter()
  {
    AudioManager.Instance.PlaySound(_collisionSound);
  }
}