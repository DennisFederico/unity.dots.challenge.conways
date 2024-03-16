using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ConwayGoL {
    public class CameraController : MonoBehaviour {

        [SerializeField] private float moveSpeed;
        [SerializeField] private float zoomSpeed;
        private Camera _camera;
        private bool _isDragging;
        private Vector2 _startDragPosition;

        void Awake() {
            _camera = Camera.main;
        }
        
        private void OnEnable() {
            StartCoroutine(WaitEnable(
                () => UIControls.Instance,
                () => UIControls.Instance.GameStartStopEvent += OnGameStartStopEvent
            ));
        }

        private static IEnumerator WaitEnable(Func<bool> condition, Action action) {
            yield return new WaitUntil(condition);
            action();
            yield return action;
        }

        private void OnDisable() {
            UIControls.Instance.GameStartStopEvent -= OnGameStartStopEvent;
        }

        private void OnGameStartStopEvent(GameStartStopEvent eventArg) {
            if (!eventArg.IsStartEvent) return;
            _camera.transform.position = new Vector3(0, 0, -10);
            //TODO Fetch the CellSize from the GameController or the UIControls
            _camera.orthographicSize = (eventArg.GridSize * 0.1f / 2f) + 0.1f;
        }

        void Update() {
            if (Mouse.current.leftButton.wasPressedThisFrame) {
                _startDragPosition = Mouse.current.position.ReadValue();
            }
            
            if (Mouse.current.leftButton.isPressed && !_isDragging) {
                if (math.distance(_startDragPosition, Mouse.current.position.ReadValue()) > 5) {
                    _isDragging = true;
                }
            }

            if (Mouse.current.leftButton.isPressed && _isDragging) {
                var newPosition = Mouse.current.position.ReadValue();
                var delta = _startDragPosition - newPosition;
                _camera.transform.Translate(delta*_camera.orthographicSize / 1000 * moveSpeed);
                _startDragPosition = newPosition;
            }

            if (Mouse.current.leftButton.wasReleasedThisFrame) {
                _isDragging = false;
            }
            
            if (Mouse.current.scroll.ReadValue().y != 0) {
                _camera.orthographicSize = Math.Max(_camera.orthographicSize - Mouse.current.scroll.ReadValue().y * zoomSpeed * Time.deltaTime, 1f);
            }
        }
    }
}