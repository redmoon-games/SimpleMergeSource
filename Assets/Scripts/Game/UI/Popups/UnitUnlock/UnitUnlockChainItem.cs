using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Popups.UnitUnlock
{
    public class UnitUnlockChainItem : MonoBehaviour
    {
        public enum EUnitUnlockChainItemStatus
        {
            Active,
            Unlocked,
            Locked
        }

        [SerializeField] private Image statusImage;
        [SerializeField] private Image characterImage;
    
        [SerializeField] private Sprite unlockedSprite;
        [SerializeField] private Sprite lockedSprite;
        [SerializeField] private Sprite activeSprite;

        public UnitUnlockChainItem Init(Sprite avatarSprite)
        {
            characterImage.sprite = avatarSprite;
            return this;
        }
    
        [Button]
        public UnitUnlockChainItem SetStatus(EUnitUnlockChainItemStatus eUnitUnlockChainItemStatus)
        {
            statusImage.sprite = eUnitUnlockChainItemStatus switch
            {
                EUnitUnlockChainItemStatus.Active => activeSprite,
                EUnitUnlockChainItemStatus.Unlocked => unlockedSprite,
                EUnitUnlockChainItemStatus.Locked => lockedSprite
            };
            characterImage.color = eUnitUnlockChainItemStatus == EUnitUnlockChainItemStatus.Locked ? Color.black : Color.white;

            return this;
        }
    }
}
