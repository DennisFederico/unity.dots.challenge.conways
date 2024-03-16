using System;
using System.Collections;
using ConwayGoL.Components;
using ConwayGoL.Systems;
using Unity.Entities;
using UnityEngine;

namespace ConwayGoL {
    public class GameController : MonoBehaviour {
        // [SerializeField] private UIControls uiControls;
        [SerializeField] private Color cellDeadColor = Color.black;
        [SerializeField] private Color cellAliveColor = Color.green;
        private const float CellSize = 0.1f;
        private World _world;
        private Entity _configEntity;
        
        private void OnEnable() {
            _world = World.DefaultGameObjectInjectionWorld;
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
            //TODO Has it changed? do we need to stop and start again?
            if (eventArg.IsStartEvent) {
                StartSimulation(eventArg);
            } else {
                StopSimulation();
            }
        }

        private void StartSimulation(GameStartStopEvent eventArg) {
            //What a config baker would do...
            _configEntity = _world.EntityManager.CreateEntity();
            _world.EntityManager.AddComponentData(_configEntity, new GoLConfig {
                ExecutionType = eventArg.ExecutionType,
                GridSize = eventArg.GridSize,
                CellSize = CellSize,
                CellDeadColor = (Vector4) cellDeadColor,
                CellAliveColor = (Vector4) cellAliveColor
            });

            if (_world.IsCreated) {
                SystemHandle spawnSystem = _world.GetExistingSystem<SpawnSystem>();
                _world.Unmanaged.ResolveSystemStateRef(spawnSystem).Enabled = true;
            }
        }

        private void StopSimulation() {
            //remove the config entity and any other that triggers system updates
            if (_world.IsCreated) {
                if (_world.EntityManager.Exists(_configEntity)) {
                    _world.EntityManager.DestroyEntity(_configEntity);
                }

                _world.EntityManager.DestroyEntity(_world.EntityManager.CreateEntityQuery(typeof(MainThreadExecute)));
                _world.EntityManager.DestroyEntity(_world.EntityManager.CreateEntityQuery(typeof(JobEntityExecute)));
            }
        }

        private void OnDestroy() {
            StopSimulation();
        }
    }
}
