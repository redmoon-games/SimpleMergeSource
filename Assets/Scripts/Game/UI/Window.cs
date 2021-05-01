﻿using System;
using Game.Services;
using Plugins.WindowsManager;
using UnityEngine;
using Zenject;

namespace Game.Ui
{
	public class Window<TDerived> : Window where TDerived : Window<TDerived>
	{
		private bool _isClosed;
		private ActivatableState _activatableState = ActivatableState.Inactive;

		protected OptionService _optionService;
		
		[Inject]
		public void Construct(
			OptionService optionService
		)
		{
			_optionService = optionService;
		}

		public override void ResetClosed()
		{
			_isClosed = false;
		}

		public override string WindowId => GetType().Name;
		public override bool HasOwnCanvas => false;

		public override void Activate(bool immediately = false)
		{
			throw new NotImplementedException();
		}

		public override void Deactivate(bool immediately = false)
		{
			throw new NotImplementedException();
		}

		public override ActivatableState ActivatableState
		{
			get => _activatableState;
			protected set
			{
				if (value == _activatableState) return;
				var args = new ActivatableStateChangedEventArgs(value, _activatableState);
				_activatableState = value;
				ActivatableStateChangedEvent?.Invoke(this, args);
				TriggerEvents(value);
			}
		}

		private void TriggerEvents(ActivatableState value)
		{
			switch (value)
			{
				case ActivatableState.Active:
					OnWindowOpen();
					break;
				case ActivatableState.Inactive:
					OnWindowClose();
					break;
			}
		}

		protected void OnWindowOpen()
		{
			_optionService.groupCanMove.Value = false;
		}

		protected void OnWindowClose()
		{
			_optionService.groupCanMove.Value = true;
		}

		public override event EventHandler<ActivatableStateChangedEventArgs> ActivatableStateChangedEvent;

		public override bool Close(bool immediately = false)
		{
			if (_isClosed || this.IsInactiveOrDeactivated()) return false;

			if (!this.IsActive() && this.IsActiveOrActivated())
			{
				Debug.LogWarningFormat("Trying to close window {0} before it was activated.", GetType().FullName);

				void OnActivatableStateChanged(object sender, EventArgs args)
				{
					var activatableStateChangedEventArgs = (ActivatableStateChangedEventArgs) args;
					if (activatableStateChangedEventArgs.CurrentState != ActivatableState.Active) return;
					ActivatableStateChangedEvent -= OnActivatableStateChanged;
					Close(immediately);
				}

				ActivatableStateChangedEvent += OnActivatableStateChanged;
				return true;
			}

			_isClosed = true;
			CloseWindowEvent?.Invoke(this, null);
			Deactivate(immediately);
			return true;
		}
		
		
		protected virtual void OnDestroy()
		{
			ActivatableStateChangedEvent = null;
			CloseWindowEvent = null;

			DestroyWindowEvent?.Invoke(this, null);
			DestroyWindowEvent = null;
		}

		public override void SetArgs(object[] args)
		{
			throw new NotImplementedException();
		}

		public override event EventHandler<WindowResultEventArgs> CloseWindowEvent;

		public override event EventHandler<WindowResultEventArgs> DestroyWindowEvent;
	}
}