using System;
using Core.BigNumberAsset;

namespace Game.Models
{
    [Serializable]
    public class LevelVo
    {
        public int level;
        public BigValue income;
        public BigValue startPrice;
        public float priceFactor;
        public int priceStep;
        public int crystalPrice;

        public BigValue GetGoldPrice(int step)
        {
            var factor = (float)step / priceStep * priceFactor;

            BigValue additionalGoldPrice = new BigValue(startPrice);
            additionalGoldPrice = (BigValue)additionalGoldPrice.Multiply((decimal)factor);

            BigValue goldPrice = new BigValue(startPrice);
            goldPrice = (BigValue)goldPrice.Add(additionalGoldPrice);

            return goldPrice;
        }

        public int GetCrystalPrice => crystalPrice;
    }
}
