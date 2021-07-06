using Core.BigNumberAsset;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class IncomeView : MonoBehaviour
    {

        [SerializeField] private TextMeshProUGUI money;
        [SerializeField] private Image icon;
        [Space]
        [SerializeField] private Sprite goldIcon;
        [SerializeField] private Sprite crystalIcon;
        [Space]
        [SerializeField] [Range(0.5f, 5.0f)] private float lifeTime = 2f;
        [SerializeField] [Range(0f, 1000f)] private float distance = 300f;
        [SerializeField] [Range(0f, 1000f)] private float range = 80f;


        void Awake()
        {
            money.text = BigValue.Zero.ToColorString();
        }

        public void Init(BigValue gold, Vector2 position = default)
        {
            if (position != default) transform.position = position;

            transform.localPosition = new Vector2(UnityEngine.Random.Range(transform.localPosition.x - range, transform.localPosition.x + range), transform.localPosition.y);

            icon.sprite = goldIcon;
            money.text = gold.ToColorString();

            PlayAnimation();
        }

        public void Init(int crystal, Vector2 position = default)
        {
            if (position != default) transform.position = position;

            transform.localPosition = new Vector2(UnityEngine.Random.Range(transform.localPosition.x - range, transform.position.x + range), transform.localPosition.y);

            icon.sprite = crystalIcon;
            money.text = crystal.ToString();

            PlayAnimation();
        }

        private void PlayAnimation()
        {
            var sequence = DOTween.Sequence();
            var transparentSequence = DOTween.Sequence();

            Vector3 targetPos = TargetWorldPoint();
            float transparency = 1.0f;

            transparentSequence.AppendInterval(lifeTime / 3);

            sequence.Append(transform.DOLocalMove(targetPos, lifeTime));
            transparentSequence.Append(DOTween.To(() => transparency, x => transparency = x, 0.0f, lifeTime).OnUpdate(() =>
            {
                SetTransparency(transparency);
            }).OnComplete(() =>
            {
                sequence.Kill();
                Destroy(gameObject);
            }));
        }

        private void SetTransparency(float transparency)
        {
            money.color = new Color(money.color.r, money.color.g, money.color.b, transparency);
            icon.color = new Color(icon.color.r, icon.color.g, icon.color.b, transparency);
        }

        private Vector3 TargetWorldPoint()
        {
            return transform.localPosition + Vector3.up * distance;
        }

    }
}
