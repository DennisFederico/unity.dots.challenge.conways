using ConwayGoL.Authoring;
using ConwayGoL.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;

namespace ConwayGoL.Systems {
    
    /// <summary>
    /// Standard Rules of Conway's Game of Life:
    /// Cells are arranged in a square grid
    /// Cells can be considered live or dead
    /// Cells live or die each generation based on neighboring cells (up, down, left, right, 4 diagonal corners)
    /// A generation is a single update of all cells in the simulation
    /// A live cell with 0 or 1 live neighbors dies next generation due to underpopulation
    /// A live cell with 2 or 3 live neighbors lives on to the next generation
    /// A live cell with 4 or more live neighbors dies next generation due to overpopulation
    /// A dead cell with exactly 3 live neighbors becomes a live cell next generation due to reproduction
    /// </summary>

    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public partial struct GoLMainThreadSystem : ISystem {

        private ComponentLookup<URPMaterialPropertyBaseColor> _colorComponentLookup;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<MainThreadExecute>();
            state.RequireForUpdate<GoLConfig>();
            _colorComponentLookup = state.GetComponentLookup<URPMaterialPropertyBaseColor>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            ChangeColor(ref state);    
        }
        
        [BurstCompile]
        private void ChangeColor(ref SystemState state) {
            var config = SystemAPI.GetSingleton<GoLConfig>();
            var cellsBuffer = SystemAPI.GetSingletonBuffer<CellState>();
            var cellsGrid = cellsBuffer.ToNativeArray(Allocator.Temp).Reinterpret<bool>();
            
            var aliveState = new CellState() { IsAlive = true };
            var deadState = new CellState() { IsAlive = false };
            var aliveColor = new URPMaterialPropertyBaseColor { Value = config.CellAliveColor };
            var deadColor = new URPMaterialPropertyBaseColor { Value = config.CellDeadColor };

            _colorComponentLookup.Update(ref state);
            foreach (var (cellPos, entity) in SystemAPI.Query<RefRO<CellPosition>>().WithEntityAccess()) {
                var index = GetFlatIndex(cellPos.ValueRO.X, cellPos.ValueRO.Y, config.GridSize);
                var isAlive = cellsGrid[index];
                var neighbourCount = GetNeighbourCount(cellPos.ValueRO.X, cellPos.ValueRO.Y, config.GridSize, cellsGrid);
                
                if (isAlive && neighbourCount is <= 1 or >= 4) {
                    cellsBuffer[index] = deadState;
                    _colorComponentLookup[entity] = deadColor;
                } else if (!isAlive && neighbourCount == 3) {
                    cellsBuffer[index] = aliveState;
                    _colorComponentLookup[entity] = aliveColor;
                }
            }
        }
        
        // [BurstCompile]
        // private void UpdateCellState(ref SystemState state) {
        //     var config = SystemAPI.GetSingleton<GoLConfig>();
        //     var cellsBuffer = SystemAPI.GetSingletonBuffer<CellState>();
        //     var cellsGrid = cellsBuffer.ToNativeArray(Allocator.Temp).Reinterpret<bool>();
        //     
        //     var aliveState = new CellState() { IsAlive = true };
        //     var deadState = new CellState() { IsAlive = false };
        //     
        //     _colorComponentLookup.Update(ref state);
        //     for (var y = 0; y < config.GridSize; y++) {
        //         for (var x = 0; x < config.GridSize; x++) {
        //             var index = GetFlatIndex(x, y, config.GridSize);
        //             var isAlive = cellsGrid[index];
        //             var neighbourCount = GetNeighbourCount(x, y, config.GridSize, cellsGrid);
        //             
        //             if (isAlive && neighbourCount is <= 1 or >= 4) {
        //                 cellsBuffer[index] = deadState;
        //             } else if (!isAlive && neighbourCount == 3) {
        //                 cellsBuffer[index] = aliveState;
        //             }
        //         }
        //     }
        // }
        
        // [BurstCompile]
        // private void FlipCell(ref SystemState state) {
        //     var config = SystemAPI.GetSingleton<GoLConfig>();
        //     var cellsBuffer = SystemAPI.GetSingletonBuffer<CellState>();
        //     var cellsGrid = cellsBuffer.ToNativeArray(Allocator.Temp).Reinterpret<bool>();
        //     
        //     var aliveState = new CellState() { IsAlive = true };
        //     var deadState = new CellState() { IsAlive = false };
        //
        //     foreach (var (cell, transform) in SystemAPI.Query<RefRO<CellPosition>, RefRW<LocalTransform>>()) {
        //         var index = GetFlatIndex(cell.ValueRO.X, cell.ValueRO.Y, config.GridSize);
        //         var isAlive = cellsGrid[index];
        //         var neighbourCount = GetNeighbourCount(cell.ValueRO.X, cell.ValueRO.Y, config.GridSize, cellsGrid);
        //         
        //         if (isAlive && neighbourCount is <= 1 or >= 4) {
        //             cellsBuffer[index] = deadState;
        //             var transformRO = transform.ValueRO;
        //             transform.ValueRW = LocalTransform.FromPositionRotationScale(transformRO.Position, quaternion.RotateX(180), transformRO.Scale);
        //         } else if (!isAlive && neighbourCount == 3) {
        //             cellsBuffer[index] = aliveState;
        //             var transformRO = transform.ValueRO;
        //             transform.ValueRW = LocalTransform.FromPositionRotationScale(transformRO.Position, quaternion.RotateX(0), transformRO.Scale);
        //         }
        //     }
        // }
        
        private int GetNeighbourCount(int x, int y, int gridSize, NativeArray<bool> cellsGrid) {
            var count = 0;
            for (var dy = -1; dy <= 1; dy++) {
                for (var dx = -1; dx <= 1; dx++) {
                    if (dx == 0 && dy == 0) continue;
                    var nx = x + dx;
                    var ny = y + dy;
                    if (nx >= 0 && nx < gridSize && ny >= 0 && ny < gridSize) {
                        count += cellsGrid[ny * gridSize + nx] ? 1 : 0;
                    }
                }
            }
            return count;
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {

        }
     
        [BurstCompile]
        private int GetFlatIndex(int x, int y, int width) => y * width + x;
        // private int2 GetXY(int index, int width) => new int2(index % width, index / width);
    }
}