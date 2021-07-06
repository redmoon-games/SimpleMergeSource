using System;
using System.Collections.Generic;
using System.Linq;
using Game.Signals.FromWindow;
using UnityEngine;
using Zenject;

namespace Game.UI.BottomBar
{
    [Serializable]
    public class ButtonAnimator
    {
        public EDownBarButtonType type;
        public Animator animator;
    }

    public class BottomBarController : MonoBehaviour
    {
        [Space]
        [SerializeField] private List<ButtonAnimator> buttonAnimators;
        [Header("Names of animator parameters")]
        [SerializeField] private string show = "Show";

        public EDownBarButtonType CurrentHighlight { get; private set; }

        private SignalBus _signalBus;

        [Inject]
        private void Construct(SignalBus signalBus)
        {
            _signalBus = signalBus;
            SelectSpawn();
        }

        private void Awake()
        {
            _signalBus.Subscribe<AllClosedSignal>(UnSelectAll);
            _signalBus.Subscribe<DownBarShowedSignal>(SelectBy);
            _signalBus.Subscribe<DownBarClosedSignal>(UnSelectBy);
        }

        private void OnDestroy()
        {
            _signalBus.Unsubscribe<AllClosedSignal>(UnSelectAll);
            _signalBus.Unsubscribe<DownBarShowedSignal>(SelectBy);
            _signalBus.Unsubscribe<DownBarClosedSignal>(UnSelectBy);
        }

        public void SelectSpawn()
        {
            var spawnButton = buttonAnimators.FirstOrDefault(butAnim => butAnim.type == EDownBarButtonType.Spawn);

            if (spawnButton != default)
            {
                Select(spawnButton);
            }
            else
            {
                throw new Exception("Spawn button not assigned.");
            }
        }

        private void SelectBy(DownBarShowedSignal signal)
        {
            foreach (var buttonAnimator in buttonAnimators)
            {
                if (buttonAnimator.type == signal.ButtonType)
                {
                    Select(buttonAnimator);
                }
            }
        }

        private void UnSelectBy(DownBarClosedSignal signal)
        {
            foreach (var buttonAnimator in buttonAnimators)
            {
                if (buttonAnimator.type == signal.ButtonType)
                {
                    UnSelect(buttonAnimator.animator);
                }
            }
        }

        private void UnSelectAll()
        {
            buttonAnimators.ForEach(butAnim => UnSelect(butAnim.animator));
        }

        private void Select(ButtonAnimator buttonAnimator)
        {
            CurrentHighlight = buttonAnimator.type;
            buttonAnimator.animator.SetBool(show, true);
        }

        private void UnSelect(Animator animator)
        {
            animator.SetBool(show, false);
        }
    }
}