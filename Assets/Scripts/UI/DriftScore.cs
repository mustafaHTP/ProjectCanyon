using TMPro;
using UnityEngine;

public class DriftScore : MonoBehaviour
{
    [SerializeField] private int _driftScoreFactor;

    private Transform _playerCar;
    private int _driftScore;
    private TextMeshProUGUI _driftScoreText;
    private Rigidbody _carRigidBody;
    private CarController _carController;

    private void Awake()
    {
        FindPlayerCar();

        _carRigidBody = _playerCar.GetComponent<Rigidbody>();
        _carController = _playerCar.GetComponent<CarController>();
        _driftScoreText = GetComponent<TextMeshProUGUI>();
        _driftScore = 0;
    }

    private void FixedUpdate()
    {
        if (_carController.IsDrifting)
        {
            _driftScoreText.enabled = true;
            IncreaseScore();
        }
        else
        {
            _driftScoreText.enabled = false;
        }
    }

    private void IncreaseScore()
    {
        float driftDirection = Vector3.Dot(_carRigidBody.velocity.normalized, _carRigidBody.transform.right.normalized);
        _driftScore += (int)(_driftScoreFactor * Mathf.Abs(driftDirection) * Time.fixedDeltaTime);
        _driftScoreText.text = _driftScore.ToString();
    }

    private void FindPlayerCar()
    {
        _playerCar = FindAnyObjectByType<Player>().transform;
        if (_playerCar == null)
        {
            Debug.LogError("Player car has not been found !");
        }
    }
}
