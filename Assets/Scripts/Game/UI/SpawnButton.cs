using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpawnButton : MonoBehaviour
{

    [SerializeField] private GameObject Counter;
    [SerializeField] private TextMeshProUGUI CounterText;
    [SerializeField] private TextMeshProUGUI RoomFullText;
    [SerializeField] private Image ButtonImage;
    [SerializeField] private Animator ButtonAnimator;

    private float _maxTime = 2f;
    private bool _isRoomFull;

    public Action<float> ButtonTaped;


    public void Tap()
    {
        if (!_isRoomFull)
        {
            ButtonAnimator.SetTrigger("OnTapped");
            ButtonTaped?.Invoke(_maxTime);
        }
    }

    public void SetIsRoomFull(bool isRoomFull)
    {
        _isRoomFull = isRoomFull;

        Counter.gameObject.SetActive(!isRoomFull);
        ButtonImage.gameObject.SetActive(!isRoomFull);
        RoomFullText.gameObject.SetActive(isRoomFull);
    }

    public void SetFillAmount(float fillAmount)
    {
        float counterSeconds = Mathf.Ceil((1 - fillAmount) * _maxTime);

        if (counterSeconds < 1)
        {
            return;
        }

        ButtonImage.fillAmount = fillAmount;
        CounterText.text = counterSeconds.ToString("0");
    }

    public void SetMaxTime(float maxTime)
    {
        _maxTime = maxTime;
    }
}
