using UnityEngine;

namespace Game.Controllers
{
    public class CellController : MonoBehaviour
    {
        [SerializeField]private SpriteRenderer background;
        [SerializeField] private SpriteRenderer selectEffect;
        [SerializeField] private SpriteRenderer mergeEffect;
        [Space]
        [SerializeField] private Sprite lightCellSprite;
        [SerializeField] private Sprite darkCellSprite;
        [Space] 
        [SerializeField] private Animator animator;
        
        public Vector2Int GridCoord { get; private set; }

        private static readonly int OnCreateTrigger = Animator.StringToHash("OnCreate");

        private void Awake()
        {
            background.sprite = darkCellSprite;
            background.gameObject.SetActive(false);

            SetSelected(false);
            SetMergeable(false);
        }

        public void Init(bool isLightCell, Vector2Int gridCoord)
        {
            GridCoord = gridCoord;
            background.sprite = isLightCell ? lightCellSprite : darkCellSprite;
            background.gameObject.SetActive(true);
        }

        public void SetSelected(bool selected)
        {
            selectEffect.gameObject.SetActive(selected);
        }

        public void SetMergeable(bool mergeable)
        {
            mergeEffect.gameObject.SetActive(mergeable);
        }

        public void SetScale(Vector3 newScale)
        {
            transform.localScale = newScale;
        }

        public void AnimateCreation()
        {
            animator.SetTrigger(OnCreateTrigger);
        }
    }
}