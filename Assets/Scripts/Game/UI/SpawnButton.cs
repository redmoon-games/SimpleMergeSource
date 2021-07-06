using System;
using Game.Signals.FromWindow;
using Game.UI.BottomBar;
using Game.UI.Popups.BuildingGame;
using Plugins.WindowsManager;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Game.UI
{
    public class SpawnButton : MonoBehaviour
    {

        [SerializeField] private GameObject Counter;
        [SerializeField] private TextMeshProUGUI CounterText;
        [SerializeField] private TextMeshProUGUI RoomFullText;
        [SerializeField] private Image ButtonImage;
        [SerializeField] private Animator ButtonAnimator;
        [SerializeField] private BottomBarController barController;
		[SerializeField] private WindowManager windowManager;
        
        public Action<float> ButtonTaped;
        
        private float _maxTime = 2f;
        private bool _isRoomFull;

		private SignalBus _signalBus;

	    [Inject]
	    private void Construct(SignalBus signalBus)
	    {
	        _signalBus = signalBus;
	    }
        
        public void Tap()
        {
            windowManager.CloseAll();
        	_signalBus.Fire(new AllClosedSignal());
	        if (!_isRoomFull && barController.CurrentHighlight == EDownBarButtonType.Spawn)
	        {
	            ButtonAnimator.SetBool("Show", true);
	            ButtonTaped?.Invoke(_maxTime);
	        }
        }

        public void SetIsRoomFull(bool isRoomFull)
        {
            _isRoomFull = isRoomFull;

            Counter.gameObject.SetActive(!isRoomFull);
            ButtonImage.gameObject.SetActive(!isRoomFull);
            RoomFullText.gameObject.SetActive(isRoomFull);
        }

        public void SetFillAmount(float fillAmount)
        {
            float counterSeconds = Mathf.Ceil((1 - fillAmount) * _maxTime);

            if (counterSeconds < 1)
            {
                return;
            }

            ButtonImage.fillAmount = fillAmount;
            CounterText.text = counterSeconds.ToString("0");
        }

        public void SetMaxTime(float maxTime)
        {
            _maxTime = maxTime;
        }
    }
}
