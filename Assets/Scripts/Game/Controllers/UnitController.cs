using System;
using Core.Databases.Audio;
using Core.StateMachine;
using Game.AnimationControllers;
using Game.Merge;
using Game.Models;
using Game.Signals;
using UnityEngine;
using Zenject;

namespace Game.Controllers
{
    [Serializable]
    public enum ECharacterState
    {
        InBox,
        Idle,
        Merge
    }

    [Serializable]
    public class UnitData
    {
        public UnitVo data;
        public ECharacterState state;
        public int level;
        public Vector2Int gridPosition;
        public bool isSpawned;
    }

    [Serializable]
    public class UnitInitParams
    {
        public UnitData data;
        public MergeNodeVo mergeNodeVo;
        public RoomData roomData;
        public Sprite boxSprite;
        public int level;
    }

    public class UnitController : MonoBehaviour
    {
        [SerializeField] private SoundSource onTapSound;
        [Space]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private BoxCollider2D boxCollider;
        [SerializeField] private Collider2D unitCollider;
        [SerializeField] private Mergeable mergeable;
        [SerializeField] private Animator animator;

        public Mergeable Mergeable => mergeable;
        public bool IsMovable { get; private set; }

        public event Action<UnitController> UnitMoveInterrupted = controller => {};

        private UnitData _data;
        private IState<ECharacterState> _rootState;
        private Sprite _startSprite;
        private UnitAnimationController _unitAnimation;
        private SignalBus _signalBus;

        [Inject]
        public void Construct(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public void Init(UnitInitParams initParams)
        {
            _data = initParams.data;
            _startSprite = spriteRenderer.sprite;
            mergeable.Init(initParams.mergeNodeVo);
            mergeable.Merged += OnMerged;
            mergeable.MergeChainEnded += OnMergeChainEnded;

            _unitAnimation = animator.GetBehaviour<UnitAnimationController>();
            _unitAnimation.UnitSpawned += FinishSpawnAnimation;

            InitStateMachine(initParams);
        }

        public void Tap()
        {
            _rootState.TriggerEvent("OnClick");
        }

        public void Touch()
        {
            onTapSound.Play(transform);
            _rootState.TriggerEvent("OnTouch");
        }

        public void StartDrag()
        {
            _rootState.TriggerEvent("OnDragged");
            _signalBus.Fire(new UnitSignal(new UnitSignalData(EUnitSignalType.StartDragged, this), this));
        }

        public void Drag()
        {
            _signalBus.Fire(new UnitSignal(new UnitSignalData(EUnitSignalType.Dragged, this), this));
        }

        public void ChangeState(ECharacterState newState)
        {
            _rootState.ChangeState(newState);

            _signalBus.Fire(new UnitDataChangedSignal());
        }

        public void FinishMove()
        {
            animator.SetTrigger("DragFinished");
            _signalBus.Fire(new UnitSignal(new UnitSignalData(EUnitSignalType.Moved, this), this));
        }

        public void FinishSpawnAnimation()
        {
            _data.isSpawned = true;
            _signalBus.Fire(new UnitDataChangedSignal());
        }

        public void SetLocalPosition(Vector2 position, Vector2Int gridPosition)
        {
            transform.localPosition = position;
            if (_data.gridPosition != gridPosition)
            {
                _signalBus.Fire(new UnitDataChangedSignal());
            }
            _data.gridPosition = gridPosition;
        }

        public void ResetLocalPosition()
        {
            transform.localPosition = Vector3.zero;
        }

        public void InterruptMove()
        {
            UnitMoveInterrupted?.Invoke(this);
            animator.SetTrigger("DragFinished");
        }

        private void InitStateMachine(UnitInitParams initParams)
        {
            var boxSprite = initParams.boxSprite;
            _rootState = new StateMachineBuilder<ECharacterState>()
                .State(ECharacterState.InBox)
                .Enter(state =>
                {
                    spriteRenderer.sprite = boxSprite;
                    unitCollider.enabled = false;
                    boxCollider.size = boxSprite.rect.size / boxSprite.pixelsPerUnit;
                    boxCollider.enabled = true;
                    mergeable.IsMergeble = false;
                    IsMovable = false;
                    _data.state = ECharacterState.InBox;

                    if (!_data.isSpawned)
                    {
                        animator.SetTrigger("Spawned");
                    }
                })
                .Event("OnClick", state =>
                {
                    if (_data.isSpawned)
                    {
                        animator.SetTrigger("Tapped");
                    }
                })
                .Event("OnTouch", state =>
                {
                    if (_data.isSpawned)
                    {
                        animator.SetTrigger("Tapped");
                        state.Parent.ChangeState(ECharacterState.Idle);
                    }
                })
                .End()
                .State(ECharacterState.Idle)
                .Enter(state =>
                {
                    spriteRenderer.sprite = _startSprite;
                    unitCollider.enabled = true;
                    boxCollider.enabled = false;
                    mergeable.IsMergeble = true;
                    IsMovable = true;
                    if (_data.state != ECharacterState.Idle)
                    {
                        _data.state = ECharacterState.Idle;
                        _signalBus.Fire(new UnitDataChangedSignal(true));
                    }
                })
                .Event("OnClick", state =>
                {
                    animator.SetTrigger("Tapped");
                    _signalBus.Fire(new UnitTappedSignal(new UnitTappedData(_data.level, transform.position), this));
                })
                .Event("OnDragged", state => { animator.SetTrigger("Dragged"); })
                .End()
                .State(ECharacterState.Merge)
                .Enter(state =>
                {
                    IsMovable = false;
                    mergeable.IsMergeble = false;
                    _data.state = ECharacterState.Merge;
                })
                .End()
                .Build();
            _rootState.ChangeState(_data.state);
        }

        private void OnMerged(MergedData data)
        {
            _signalBus.Fire(new MergedSignal(data, this));
        }

        private void OnMergeChainEnded(MergedData data)
        {
            _signalBus.Fire(new MergeChainEndedSignal(data, this));
        }
    }
}
