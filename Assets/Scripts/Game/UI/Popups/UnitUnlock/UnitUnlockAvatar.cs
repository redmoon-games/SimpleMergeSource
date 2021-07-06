using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Popups.UnitUnlock
{
    public class UnitUnlockAvatar : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI nameTextHolder;
        [SerializeField] private Image avatarImageHolder;
    
        public void Init(string unitName, Sprite sprite)
        {
            nameTextHolder.text = unitName;
            avatarImageHolder.sprite = sprite;
        }
    }
}
