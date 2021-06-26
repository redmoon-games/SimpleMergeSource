using System;
using Core.BigNumberAsset;
using Game.UI;
using UnityEngine;

namespace Game.Services
{
    [Serializable]
    public class WalletData
    {
        public BigValue gold = BigValue.Zero;
        public int crystal;
    }
    
    public class WalletService : MonoBehaviour
    {
        public event Action<WalletData> Updated;

        [SerializeField] private GameUI gameUI;
        [SerializeField] private Camera camera;
        [SerializeField] private Transform incomeViewsTarget;
        [SerializeField] private GameObject incomeViewPrefab;

        private WalletData _data;

        public void Load(WalletData data)
        {
            if(data == null)
            {
                data = new WalletData();
            }

            Updated += gameUI.UpdateMoney;
            _data = data;
        }

        public bool TryAddGold(BigValue amount, Vector2 position = default)
        {
            if (amount >= BigValue.Zero)
            {
                _data.gold.Add(amount);
                if(position != default) SpawnIncomeView(amount, position);
                OnUpdateWallet();
                return true;
            }

            return false;
        }

        public bool TrySubtractGold(BigValue amount)
        {
            if(amount <= _data.gold)
            {
                _data.gold.Subtract(amount);
                OnUpdateWallet();
                return true;
            }

            return false;
        }

        public bool TryAddCrystal(int amount, Vector2 position = default)
        {
            if (amount >= 0)
            {
                _data.crystal += amount;
                if (position != default) SpawnIncomeView(amount, position);
                OnUpdateWallet();
                return true;
            }

            return false;
        }

        public bool TrySubtractCrystal(int amount)
        {
            if(amount >= 0 && amount <= _data.crystal)
            {
                _data.crystal -= amount;
                OnUpdateWallet();
                return true;
            }

            return false;
        }



        private void OnUpdateWallet()
        {
            Updated?.Invoke(_data);
        }

        private void SpawnIncomeView(BigValue amount, Vector3 position)
        {
            var incomeViewGameObject = Instantiate(incomeViewPrefab, WorldToCanvas(position), Quaternion.identity, incomeViewsTarget);
            var incomeView = incomeViewGameObject.GetComponent<IncomeView>();

            incomeView.Init(amount);
        }

        private void SpawnIncomeView(int amount, Vector3 position)
        {
            var incomeViewGameObject = Instantiate(incomeViewPrefab, WorldToCanvas(position), Quaternion.identity, incomeViewsTarget);
            var incomeView = incomeViewGameObject.GetComponent<IncomeView>();

            incomeView.Init(amount);
        }

        private Vector2 WorldToCanvas(Vector2 vector)
        {
            Vector2 ViewportPosition = camera.WorldToScreenPoint(vector);
            return ViewportPosition;
        }

        public WalletData GetData => _data;
    }
}
