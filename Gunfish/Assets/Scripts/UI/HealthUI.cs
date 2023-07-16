using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{

    [SerializeField]
    private Canvas _canvas;
    [SerializeField]
    private RawImage _redBar;
    [SerializeField]
    private RawImage _orangeBar;
    [SerializeField]
    private RawImage _greenBar;

    Gunfish _gunfish;

    public void Start()
    {
        _redBar = transform.FindDeepChild("Red").GetComponent<RawImage>();
        _orangeBar = transform.FindDeepChild("Orange").GetComponent<RawImage>();
        _greenBar = transform.FindDeepChild("Green").GetComponent<RawImage>();
    }

    public void Init(Gunfish gunfish)
    {
        _gunfish = gunfish;

        _gunfish.gameObject.GetComponent<Gunfish>().OnHealthUpdated += UpdateHealth;
        SetHealth(_gunfish.data.maxHealth);
    }

    public void SetHealth(float health)
    {
        _greenBar.rectTransform.localScale = new Vector3(health / _gunfish.data.maxHealth, 1f, 1f);
        _orangeBar.rectTransform.localScale = new Vector3(health / _gunfish.data.maxHealth, 1f, 1f);
        _canvas.enabled = false;
    }

    private bool _hitInProgress = false;
    private float _targetPercentage = 0f;
    private const float _hitDuration = 2f;
    private float _timeSpentWaiting = 0f;

    public void UpdateHealth(float health)
    {
        _canvas.enabled = true;
        _timeSpentWaiting = 0f;
        _targetPercentage = health / _gunfish.data.maxHealth;
        _greenBar.rectTransform.localScale = new Vector3(_targetPercentage, 1f, 1f);

        if (!_hitInProgress)
        {
            _hitInProgress = true;
            StartCoroutine(UpdateHealthCR());
        }
    }

    private IEnumerator UpdateHealthCR()
    {
        yield return new WaitForSeconds(0.2f);
        float currentPercentage = _orangeBar.rectTransform.localScale.x;
        while (currentPercentage > _targetPercentage)
        {
            _orangeBar.rectTransform.localScale = new Vector3(currentPercentage, 1f, 1f);
            currentPercentage -= Time.deltaTime / _hitDuration;
            yield return new WaitForEndOfFrame();
        }
        _orangeBar.rectTransform.localScale = new Vector3(_targetPercentage, 1f, 1f);
        _hitInProgress = false;

        while (_timeSpentWaiting < 2f)
        {
            if (_hitInProgress) break;
            _timeSpentWaiting += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }


        if (!_hitInProgress)
        {
            _canvas.enabled = false;
        }
    }
}