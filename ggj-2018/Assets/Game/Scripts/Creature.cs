using UnityEngine;

public class Creature : MonoBehaviour
{
  [SerializeField]
  private Character _character = null;

  [SerializeField]
  private Transform _animationAnchor = null;

  [SerializeField]
  private float _animationSpeed = 1.0f;

  [SerializeField]
  private float _animationHeight = 1.0f;

  [SerializeField]
  private AnimationCurve _animationHeightCurve;

  [SerializeField]
  private float _changeDirectionTimeMin = 3.0f;

  [SerializeField]
  private float _changeDirectionTimeMax = 6.0f;

  private float _changeDirectionTimer;
  private float _animationTimer;

  private void Start()
  {
    _animationTimer = Random.Range(0.0f, 10.0f);
  }

  private void Update()
  {
    _changeDirectionTimer -= Time.deltaTime;
    _animationTimer += Time.deltaTime * _animationSpeed;

    float animationT = _animationTimer;
    float height = _animationHeightCurve.Evaluate(animationT) * _animationHeight;
    _animationAnchor.transform.localPosition = Vector3.zero + Vector3.up * height;

    if (_changeDirectionTimer < 0)
    {
      _changeDirectionTimer = Random.Range(_changeDirectionTimeMin, _changeDirectionTimeMax);

      Vector2 direction = Random.insideUnitCircle;
      _character.MoveDirection = new Vector3(direction.x, 0, direction.y);
    }
  }
}