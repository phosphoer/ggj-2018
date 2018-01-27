using UnityEngine;

public class Character : MonoBehaviour
{
  public Vector3 MoveDirection
  {
    get { return _moveDirection; }
    set { _moveDirection = value.normalized; }
  }

  [SerializeField]
  private float _moveSpeed = 1.0f;

  [SerializeField]
  private Rigidbody _rigidBody = null;

  [SerializeField]
  private Vector3 _moveDirection;

  private void Update()
  {
    _rigidBody.MovePosition(_rigidBody.position + MoveDirection * _moveSpeed * Time.deltaTime);
  }
}