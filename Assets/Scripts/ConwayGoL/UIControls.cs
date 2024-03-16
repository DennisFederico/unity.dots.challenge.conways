using System;
using System.Linq;
using ConwayGoL.Components;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ConwayGoL {
    public class UIControls : MonoBehaviour {
        
        public static UIControls Instance { get; private set; }
        
        [SerializeField] private TMP_InputField gridSize;
        [SerializeField] private TMP_Dropdown executionDropDown;
        [SerializeField] private Slider gridSlider;
        [SerializeField] private Button startButton;
        [SerializeField] private Button stopButton;
        [SerializeField] private Button exitButton;

        public event Action<GameStartStopEvent> GameStartStopEvent;

        private void Awake() {
            if (Instance == null) {
                Instance = this;
            } else {
                Destroy(gameObject);
            }
            
            ChangeControlState(false);
            executionDropDown.options = Enum.GetNames(typeof(ExecutionType))
                .Select(value => new TMP_Dropdown.OptionData(value))
                .ToList();
            gridSize.text = $"{(int)gridSlider.minValue}";
            gridSize.contentType = TMP_InputField.ContentType.IntegerNumber;
        }

        private void OnEnable() {
            gridSlider.onValueChanged.AddListener(value => gridSize.text = $"{(int)value}");
            gridSize.onValueChanged.AddListener(value => {
                if (int.TryParse(value, out var result)) {
                    gridSlider.value = result;
                }
            });
            startButton.onClick.AddListener(ProcessStartButtonClick);
            stopButton.onClick.AddListener(ProcessStopButtonClick);
            exitButton.onClick.AddListener(QuitApplication);
        }

        private void QuitApplication() {
            Application.Quit();
        }

        private void OnDisable() {
            gridSlider.onValueChanged.RemoveAllListeners();
            gridSize.onValueChanged.RemoveAllListeners();
            stopButton.onClick.RemoveAllListeners();
            startButton.onClick.RemoveAllListeners();
        }

        private void ProcessStartButtonClick() {
            ChangeControlState(true);
            GameStartStopEvent?.Invoke(new GameStartStopEvent {
                GridSize = int.Parse(gridSize.text),
                ExecutionType = (ExecutionType)executionDropDown.value,
                IsStartEvent = true
            });
        }

        private void ProcessStopButtonClick() {
            ChangeControlState(false);
            GameStartStopEvent?.Invoke(new GameStartStopEvent {
                GridSize = int.Parse(gridSize.text),
                ExecutionType = (ExecutionType)executionDropDown.value,
                IsStartEvent = false
            });
        }

        private void ChangeControlState(bool isRunning) {
            stopButton.interactable = isRunning;
            startButton.interactable = !isRunning;
            gridSize.interactable = !isRunning;
            gridSlider.interactable = !isRunning;
            executionDropDown.interactable = !isRunning;
        }
    }

    public struct GameStartStopEvent {
        public int GridSize;
        public ExecutionType ExecutionType;
        public bool IsStartEvent;

        public override string ToString() {
            return $"GridSize: {GridSize}, ExecutionType: {ExecutionType}, IsStartEvent: {IsStartEvent}";
        }

    }
}