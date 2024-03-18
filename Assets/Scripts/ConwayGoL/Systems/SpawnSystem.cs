using ConwayGoL.Authoring;
using ConwayGoL.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

namespace ConwayGoL.Systems {
    
    [UpdateBefore(typeof(TransformSystemGroup))]
    public partial struct SpawnSystem : ISystem {
        
        private Entity _gridEntity;
        private EntityQuery _cellPositionsEntityQuery;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<GoLConfig>();
            state.RequireForUpdate<CellPrefabs>();
            state.RequireForUpdate<RandomValue>();
            _cellPositionsEntityQuery = new EntityQueryBuilder(Allocator.Temp).WithAll<CellPosition>().Build(ref state);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            state.Enabled = false;
            //Clean up previous grid
            CleanUp(ref state);
            
            var config = SystemAPI.GetSingleton<GoLConfig>();
            var prefabs = SystemAPI.GetSingleton<CellPrefabs>();
            var rands = SystemAPI.GetSingletonBuffer<RandomValue>();

            var cellsBuffer = state.EntityManager.GetBuffer<CellState>(_gridEntity);
            cellsBuffer.Length = config.GridSize * config.GridSize;
            state.EntityManager.Instantiate(prefabs.CellColorPrefab, cellsBuffer.Length, state.WorldUpdateAllocator);
            //Grid origin offset to keep it centered
            var gridOffset = (config.GridSize - 1) * config.CellSize / 2;
            var aliveColor = new URPMaterialPropertyBaseColor { Value = config.CellAliveColor };
            var deadColor = new URPMaterialPropertyBaseColor { Value = config.CellDeadColor };

            new RandomizeCellsStateJob() {
                CellStatesRW = cellsBuffer.AsNativeArray(),
                Randoms = rands.AsNativeArray(),
                AliveState = new CellState() { IsAlive = true },
                DeadState = new CellState() { IsAlive = false }
            }.Schedule(cellsBuffer.Length, 128, state.Dependency).Complete();

            state.Dependency = new PositionCellsJob() {
                CellStatesRO = cellsBuffer.AsNativeArray(),
                GridSize = config.GridSize,
                CellSize = config.CellSize,
                GridOffset = gridOffset,
                AliveColor = aliveColor,
                DeadColor = deadColor
            }.ScheduleParallel(state.Dependency);
            
            //Handle the entity for the simulation
            var simulationTypeEntity = state.EntityManager.CreateEntity();
            switch (config.ExecutionType) {
                case ExecutionType.MainThread:
                    state.EntityManager.AddComponent<MainThreadExecute>(simulationTypeEntity);
                    break;
                case ExecutionType.EntityJob:
                    state.EntityManager.AddComponent<JobEntityExecute>(simulationTypeEntity);
                    break;
            }
        }
        
        [BurstCompile]
        private struct RandomizeCellsStateJob : IJobParallelFor {
            
            public NativeArray<CellState> CellStatesRW;
            public CellState AliveState;
            public CellState DeadState;
            
            [NativeDisableParallelForRestriction]
            public NativeArray<RandomValue> Randoms;
            [NativeSetThreadIndex]
            private int _threadIndex;
            
            public void Execute(int index) {
                RandomValue randomValue = Randoms[_threadIndex];
                var isAlive = randomValue.Random.NextBool();
                CellStatesRW[index] = isAlive ? AliveState : DeadState;
                Randoms[_threadIndex] = randomValue;
            }
        }
        
        [WithAll(typeof(CellPosition))]
        [WithAll(typeof(URPMaterialPropertyBaseColor))]
        [BurstCompile]
        private partial struct PositionCellsJob : IJobEntity {
            [ReadOnly] public NativeArray<CellState> CellStatesRO;
            public int GridSize;
            public float CellSize;
            public float GridOffset;
            public URPMaterialPropertyBaseColor AliveColor;
            public URPMaterialPropertyBaseColor DeadColor;
            
            [BurstCompile]
            private void Execute([EntityIndexInQuery] int index, ref CellPosition cell, ref URPMaterialPropertyBaseColor color, ref LocalTransform transform) {
                var isAlive = CellStatesRO[index].IsAlive;
                color.Value = isAlive ? AliveColor.Value : DeadColor.Value;
                cell.X = (ushort) (index % GridSize);
                cell.Y = (ushort) (index / GridSize);
                transform.Position = new float3(cell.X * CellSize - GridOffset, cell.Y * CellSize - GridOffset, 0);
                transform.Scale = CellSize;
                transform.Rotation = quaternion.identity;
            }
        }

        
        [BurstCompile]
        private void CleanUp(ref SystemState state) {
            //remove all entities with CellPosition
            state.EntityManager.DestroyEntity(_cellPositionsEntityQuery);
            _gridEntity = _gridEntity == Entity.Null ? state.EntityManager.CreateEntity() : _gridEntity;
            if (!state.EntityManager.HasBuffer<CellState>(_gridEntity)) {
                state.EntityManager.AddBuffer<CellState>(_gridEntity);                
            } else {
                state.EntityManager.GetBuffer<CellState>(_gridEntity).Clear();
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {
        }
    }
}