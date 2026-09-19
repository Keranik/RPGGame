using RPGGame.Core;
using RPGGame.Core.Simulation;
using RPGGame.UI.Templates;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.DevOnly;
#nullable disable
public class DebugClockWindow : BaseWindow<DebugClockWindow> {
	private readonly GameLoop _gameLoop;
	private readonly GameTime k_gameTime;
	private Label _timeLabel;
	private Label _dateLabel;
	private Button _startButton;
	private Button _stopButton;
	private Button _pauseButton;
	private Button _resumeButton;

	public DebugClockWindow(GameLoop gameLoop) {
		_gameLoop = gameLoop;
		_gameLoop.OnSyncUpdate += SyncUpdate;
		_gameLoop.OnRenderUpdate += RenderUpdate;
		k_gameTime=_gameLoop.GameTime;
	}

	protected override void AddHeaderContent() {
		Label headerLabel = new Label("Current Time") {
			style = {
				fontSize = 64,
				color = new StyleColor(Color.cyan),
				unityTextAlign = TextAnchor.MiddleCenter,
				marginBottom = 10
			}
		};

		HeaderContainer.Add(headerLabel);
	}

	protected override void AddCustomContent() {
		// Time Label
		_timeLabel = new Label {
			style = {
				fontSize = 48,
				color = new StyleColor(Color.white),
				//unityTextAlign = TextAnchor.MiddleCenter,
				marginBottom = 5
			},
			text = "TimeText"
		};
		_timeLabel.dataSource = _gameLoop.GameTime;		
		_timeLabel.SetBinding(nameof(_timeLabel.text), new DataBinding {
			bindingMode = BindingMode.ToTarget,
			dataSourcePath = PropertyPath.FromName(nameof(GameTime.FormattedTime))

		});
		ContentContainer.Add(_timeLabel);
		// Date Label
		_dateLabel = new Label {
			style = {
				fontSize = 48,
				color = new StyleColor(Color.white),
				//unityTextAlign = TextAnchor.MiddleCenter,
				marginBottom = 10
			},
			text = "DateText"
		};
		_dateLabel.dataSource = _gameLoop.GameTime;		
		_dateLabel.SetBinding(nameof(_dateLabel.text), new DataBinding {
			bindingMode = BindingMode.ToTarget,
			dataSourcePath = PropertyPath.FromName(nameof(GameTime.FormattedDate))

		});
		ContentContainer.Add(_dateLabel);
		// DEBUG LOG
		IEnumerable<BindingInfo> binding = _dateLabel.GetBindingInfos();
		foreach (BindingInfo item in binding) {
			Debug.Log($"bindingId: {item.bindingId} binding: {item.binding} element: {item.targetElement}");

			Binding dataBinding = item.binding;
			dataBinding.MarkDirty();
		}
		// DEBUG LOG

		// Buttons Container
		VisualElement buttonsContainer = new VisualElement() {
			style = {
				flexDirection = FlexDirection.Row,
				justifyContent = Justify.SpaceAround,
				alignItems = Align.Center,
				marginBottom = 10
			}
		};

		// Start Button
		_startButton = new Button(() => StartSimulation()) {
			text = "Start",
			style = {
				backgroundColor = new StyleColor(Color.green),
				color = new StyleColor(Color.white),
				fontSize = 18,
				marginLeft = 5,
				marginRight = 5
			}
		};

		buttonsContainer.Add(_startButton);

		// Pause Button
		_pauseButton = new Button(() => PauseSimulation()) {
			text = "Pause",
			style = {
				backgroundColor = new StyleColor(Color.yellow),
				color = new StyleColor(Color.black),
				fontSize = 18,
				marginLeft = 5,
				marginRight = 5
			}
		};

		buttonsContainer.Add(_pauseButton);

		// Resume Button
		_resumeButton = new Button(() => ResumeSimulation()) {
			text = "Resume",
			style = {
				backgroundColor = new StyleColor(Color.cyan),
				color = new StyleColor(Color.white),
				fontSize = 18,
				marginLeft = 5,
				marginRight = 5
			}
		};

		buttonsContainer.Add(_resumeButton);

		// Stop Button
		_stopButton = new Button(() => StopSimulation()) {
			text = "Stop",
			style = {
				backgroundColor = new StyleColor(Color.red),
				color = new StyleColor(Color.white),
				fontSize = 18,
				marginLeft = 5,
				marginRight = 5
			}
		};

		buttonsContainer.Add(_stopButton);

		ContentContainer.Add(buttonsContainer);
	}

	private void StartSimulation() {
		_gameLoop.Start();
	}

	private void PauseSimulation() {
		_gameLoop.Pause();
	}

	private void ResumeSimulation() {
		_gameLoop.Resume();
	}

	private void StopSimulation() {
		_gameLoop.Stop();
	}
}
