using Core.BigNumberAsset;
using Game.Services;
using TMPro;
using UnityEngine;

namespace Game.UI
{
    public class GameUI : MonoBehaviour
    {

        [SerializeField] private TextMeshProUGUI crystal;
        [SerializeField] private TextMeshProUGUI money;
        [SerializeField] private TextMeshProUGUI income;

        void Start()
        {
            crystal.text = "0";
            money.text = BigValue.Zero.ToColorString();
            income.text = $"{BigValue.Zero.ToColorString()}/Sec";
        }

        public void UpdateMoney(WalletData walletData)
        {
            money.text = walletData.gold.ToColorString();
            crystal.text = walletData.crystal.ToString();
        }

        public void UpdateIncome(BigValue moneyIncome)
        {
            income.text = $"{moneyIncome.ToColorString()}/Sec";
        }
    }
}
