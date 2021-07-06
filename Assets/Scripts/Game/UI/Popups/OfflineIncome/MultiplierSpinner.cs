using Core.Pool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using DG.Tweening;
using System;
using Game.Settings;
using UnityEngine.UI;

public class MultiplierSpinner : MonoBehaviour
{
    [SerializeField] private SpinnerItem spinerItemPrefab;
    [SerializeField] private Transform spinerItems;
    [SerializeField] private RectTransform rectTransform;

    private Pool<SpinnerItem> _itemPool;

    private float _maxSpinerPosition;
    private float _minSpinerPosition;

    private OfflinePopupSettings _offlinePopupSettings;
    private Sequence _rotatingSequence;

    public event Action<float> Rotated;

    [Inject]
    public void Construct(OfflinePopupSettings offlinePopupSettings)
    {
        _offlinePopupSettings = offlinePopupSettings;

        _itemPool = new Pool<SpinnerItem>(
               () =>
               {
                   var item = Instantiate(spinerItemPrefab, spinerItems);
                   item.gameObject.SetActive(false);
                   return item;
               },
               4
           );
    }

    public void Init(SpinnerItemData[] itemsParams)
    {
        if(_rotatingSequence != null)
        {
            _rotatingSequence.Kill(true);
            _rotatingSequence = null;
        }

        foreach (var spinerItem in _itemPool.GetActive())
        {
            Recycle(spinerItem);
        }

        foreach (SpinnerItemData itemParams in itemsParams)
        {
            Vector2 itemPos = Vector2.zero;
            if (TryGetLastItem(out var lastItem))
            {
                itemPos = GetNextItemPosition(lastItem);
            }

            var item = _itemPool.Take();
            item.Init(itemParams);
            item.transform.localPosition = itemPos;
            item.SetFillAmount(GetFillAmount(itemPos));
            item.gameObject.SetActive(true);
        }

        var items = GetSortedActiveItems();
        if(items.Count > 0)
        {
            var centralItem = items[items.Count / 2];
            MoveSpinerItems(-centralItem.transform.localPosition.x);

            _minSpinerPosition = items[0].transform.localPosition.x;
            _maxSpinerPosition = GetNextItemPosition(items[items.Count - 1]).x;
        }

    }

    private void Recycle(SpinnerItem item)
    {
        item.gameObject.SetActive(false);
        item.transform.localPosition = Vector2.zero;
        _itemPool.Recycle(item);
    }

    public void Rotate()
    {
        var randomItem = GetRandomItem();

        if(randomItem == null)
        {
            return;
        }

        RotateSpinerTo(randomItem, _offlinePopupSettings.spinerRotatingTime, _offlinePopupSettings.spinnerAdditionalCircles);
    }

    private bool TryGetLastItem(out SpinnerItem item)
    {
        if(_itemPool != null && _itemPool.GetActive().Count > 0)
        {
            var items = GetSortedActiveItems();

            item = items[items.Count - 1];
            return true;
        }

        item = null;
        return false;
    }

    private void MoveSpinerItems(float distance)
    {
        foreach (var item in _itemPool.GetActive())
        {
            item.transform.localPosition = new Vector2(item.transform.localPosition.x + distance, item.transform.localPosition.y);
            item.SetFillAmount(GetFillAmount(item.transform.localPosition));
        }
    }

    private List<SpinnerItem> GetSortedActiveItems()
    {
        var items = _itemPool.GetActive();
        items.Sort((i1, i2) =>
        {
            return i1.transform.localPosition.x.CompareTo(i2.transform.localPosition.x);
        });

        return items;
    }

    private Vector2 GetNextItemPosition(SpinnerItem item) => new Vector2(item.transform.localPosition.x + item.Width, item.transform.localPosition.y);

    private float GetSpinerWidth()
    {
        float width = 0;
        foreach (var spinerItem in _itemPool.GetActive())
        {
            width += spinerItem.Width;
        }

        return width;
    }

    private SpinnerItem GetRandomItem()
    {
        var items = _itemPool.GetActive();
        items.Sort((x, y) => -x.Chance.CompareTo(y.Chance));

        float totalChance = 0;

        foreach (SpinnerItem item in items)
        {
            totalChance += item.Chance;
        }

        if(totalChance > 1)
        {
            Debug.LogWarning("The overall probability is greater than 100%");
            return null;
        }

        var randomProbability = UnityEngine.Random.value * totalChance;

        foreach(SpinnerItem item in items)
        {
            var itemChance = item.Chance;

            if (randomProbability <= itemChance)
            {
                return item;
            }

            randomProbability -= itemChance;
        }

        return items[items.Count - 1];
    }

    public float GetFillAmount(Vector2 localPosition)
    {
        float rectBorder = rectTransform.rect.width * transform.localScale.x;

        float fillAmount = 1 - Math.Abs(localPosition.x) / rectBorder;
        return fillAmount;
    }

    private void RotateSpinerTo(SpinnerItem targetItem, float time, int rotateCount)
    {
        if(_rotatingSequence != null)
        {
            return;
        }

        _rotatingSequence = DOTween.Sequence();

        float spinerWidth = GetSpinerWidth();
        float targetSpinerPosition = (spinerWidth - targetItem.transform.localPosition.x) + spinerWidth * rotateCount;

        float spinerPosition = 0;
        float stepSpinerPosition = 0;

        _rotatingSequence.Append(DOTween.To(() => stepSpinerPosition, x => stepSpinerPosition = x, targetSpinerPosition, time).OnUpdate(() =>
        {
            float step = stepSpinerPosition - spinerPosition;

            MoveSpinerItems(step);
            spinerPosition += step;

            foreach (SpinnerItem item in GetSortedActiveItems())
            {
                if (item.transform.localPosition.x >= _maxSpinerPosition)
                {
                    float difference = item.transform.localPosition.x - _maxSpinerPosition;
                    item.transform.localPosition = new Vector2(_minSpinerPosition + difference, item.transform.localPosition.y);
                    item.SetFillAmount(GetFillAmount(item.transform.localPosition));
                }
            }

        }).OnComplete(() => 
        {
            _rotatingSequence.Kill();
            _rotatingSequence = null;

            Rotated?.Invoke(targetItem.Multiplier);
        }
        ).SetEase(Ease.OutQuart));
    }
}
