using System.Collections.Generic;
using Game.Databases;
using Game.Models;
using UnityEngine;
using Zenject;

namespace Game.UI.Popups.UnitUnlock
{
    public class UnitUnlockChain : MonoBehaviour
    {
        [SerializeField] private UnitUnlockChainItem chainItemPrefab;

        private List<UnitUnlockChainItem> _items = new List<UnitUnlockChainItem>();
        
        private ResourceDatabase _resourceDatabase;

        [Inject]
        public void Construct(ResourceDatabase resourceDatabase)
        {
            _resourceDatabase = resourceDatabase;
        }

        public void Init(UnitVo unitToUnlock, List<UnitVo> units)
        {
            while (_items.Count > 0)
            {
                Destroy(_items[0].gameObject);
                _items.Remove(_items[0]);
            }

            var activeIndex = 0;
            for (var i = 0; i < units.Count; i++)
            {
                var unit = units[i];
                var sprite = _resourceDatabase.GetUnitImageById(unit.id);
                var item = Instantiate(chainItemPrefab, transform).Init(sprite);
                _items.Add(item);
                if (unit == unitToUnlock)
                {
                    activeIndex = i;
                }
            }

            SelectStatusItem(activeIndex);
        }

        public void SelectStatusItem(int index)
        {
            for (var i = 0; i < _items.Count; i++)
            {
                if (i < index) _items[i].SetStatus(UnitUnlockChainItem.EUnitUnlockChainItemStatus.Unlocked);
                else if (i == index) _items[i].SetStatus(UnitUnlockChainItem.EUnitUnlockChainItemStatus.Active);
                else if (i > index) _items[i].SetStatus(UnitUnlockChainItem.EUnitUnlockChainItemStatus.Locked);
            }
        }
    }
}