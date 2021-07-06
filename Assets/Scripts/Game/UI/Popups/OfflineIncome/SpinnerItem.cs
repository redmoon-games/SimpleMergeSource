using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

[Serializable]
public class SpinnerItemData
{
    public float chance;
    public float multiplier;
}

public class SpinnerItem: MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _multiplierText;
    [SerializeField] private RectTransform _rectTransform;

    private float _chance;
    private float _multiplier;

    public float Chance => _chance;
    public float Multiplier => _multiplier;
    public float Width => _rectTransform.rect.width;

    public void Init(SpinnerItemData p)
    {

        _chance = p.chance;
        _multiplier = p.multiplier;

        SetMultiplierText(_multiplier);
    }

    public void SetFillAmount(float fillAmount)
    {
        fillAmount = Mathf.Clamp(fillAmount, 0, 1);

        transform.localScale = new Vector3(fillAmount, fillAmount, fillAmount);
        _multiplierText.color = new Color(_multiplierText.color.r, _multiplierText.color.g, _multiplierText.color.b, fillAmount);
    }

    private void SetMultiplierText(float multiplier)
    {
        _multiplierText.text = $"{multiplier.ToString("G")}x";
    }
}
