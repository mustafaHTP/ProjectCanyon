using TMPro;
using UnityEngine;

public class DriftScore : MonoBehaviour
{
    [SerializeField] private GameObject car;
    [SerializeField] private int driftScoreFactor;

    private int _driftScore;
    TextMeshProUGUI _driftScoreText;
    private Rigidbody _carRigidBody;
    private CarController _carController;

    private void Awake()
    {
        _carRigidBody = car.GetComponent<Rigidbody>();
        _carController = car.GetComponent<CarController>();
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
        _driftScore += (int)(driftScoreFactor * Mathf.Abs(driftDirection) * Time.fixedDeltaTime);
        _driftScoreText.text = _driftScore.ToString();
    }

}
