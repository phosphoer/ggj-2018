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

  private Vector3 _lastMoveDirection;

  private void Update()
  {
    Vector3 moveVector = MoveDirection * _moveSpeed * Time.deltaTime;
    moveVector.y = 0;

    if (moveVector.sqrMagnitude > 0)
    {
      _lastMoveDirection = moveVector;
    }

    _rigidBody.MovePosition(_rigidBody.position + moveVector);

    if (_lastMoveDirection.sqrMagnitude > 0)
    {
      Quaternion facingRotation = Quaternion.LookRotation(_lastMoveDirection);
      _rigidBody.rotation = Mathfx.Damp(transform.rotation, facingRotation, 0.5f, Time.deltaTime * 5.0f);
    }
  }
}