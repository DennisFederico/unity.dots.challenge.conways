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
    [WithAll(typeof(CellPosition))]
    [BurstCompile]
    partial struct ProcessIJobEntity : IJobEntity {

        public NativeArray<CellState> CellStatesRW;
        [ReadOnly] public NativeArray<bool> CellStatesRO;
        public int GridSize;
        public CellState AliveState;
        public CellState DeadState;
        public URPMaterialPropertyBaseColor AliveColor;
        public URPMaterialPropertyBaseColor DeadColor;
            
        [BurstCompile]
        private void Execute([EntityIndexInQuery] int index, in CellPosition cell, ref URPMaterialPropertyBaseColor color) {
            var isAlive = CellStatesRO[index];
            var neighbourCount = GoLEntityJobsSystem.GetNeighbourCountExplicit(cell.X, cell.Y, GridSize, ref CellStatesRO);
                        
            if (isAlive && neighbourCount is <= 1 or >= 4) {
                CellStatesRW[index] = DeadState;
                color.Value = DeadColor.Value;
            } else if (!isAlive && neighbourCount == 3) {
                CellStatesRW[index] = AliveState;
                color.Value = AliveColor.Value;
            }
        }
    }
    
    
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public partial struct GoLEntityJobsSystem : ISystem {

        
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<JobEntityExecute>();
            state.RequireForUpdate<GoLConfig>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var config = SystemAPI.GetSingleton<GoLConfig>();
            var cellsBuffer = SystemAPI.GetSingletonBuffer<CellState>();
            state.Dependency = new ProcessIJobEntity() {
                CellStatesRW = cellsBuffer.AsNativeArray(),
                CellStatesRO = cellsBuffer.ToNativeArray(state.WorldUpdateAllocator).Reinterpret<bool>(),
                GridSize = config.GridSize,
                AliveState = new CellState() { IsAlive = true },
                DeadState = new CellState() { IsAlive = false },
                AliveColor = new URPMaterialPropertyBaseColor { Value = config.CellAliveColor },
                DeadColor = new URPMaterialPropertyBaseColor { Value = config.CellDeadColor },
            }.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {

        }
        
        [BurstCompile]
        public static int GetNeighbourCount(int x, int y, int gridSize, ref NativeArray<bool> cellsGrid) {
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
        public static int GetNeighbourCountExplicit(int x, int y, int gridSize, ref NativeArray<bool> cellsGrid) {
            var count = 0;

            int dx = x - 1;
            int dy = y - 1;
            if (dx >= 0 && dx < gridSize && dy >= 0 && dy < gridSize) {
                count += cellsGrid[dy * gridSize + dx] ? 1 : 0;
            }
            
            dx = x - 1;
            dy = y;
            if (dx >= 0 && dx < gridSize && dy >= 0 && dy < gridSize) {
                count += cellsGrid[dy * gridSize + dx] ? 1 : 0;
            }
            
            dx = x - 1;
            dy = y + 1;
            if (dx >= 0 && dx < gridSize && dy >= 0 && dy < gridSize) {
                count += cellsGrid[dy * gridSize + dx] ? 1 : 0;
            }
            
            dx = x;
            dy = y - 1;
            if (dx >= 0 && dx < gridSize && dy >= 0 && dy < gridSize) {
                count += cellsGrid[dy * gridSize + dx] ? 1 : 0;
            }
            
            dx = x;
            dy = y + 1;
            if (dx >= 0 && dx < gridSize && dy >= 0 && dy < gridSize) {
                count += cellsGrid[dy * gridSize + dx] ? 1 : 0;
            }
            
            dx = x + 1;
            dy = y - 1;
            if (dx >= 0 && dx < gridSize && dy >= 0 && dy < gridSize) {
                count += cellsGrid[dy * gridSize + dx] ? 1 : 0;
            }
            
            dx = x + 1;
            dy = y;
            if (dx >= 0 && dx < gridSize && dy >= 0 && dy < gridSize) {
                count += cellsGrid[dy * gridSize + dx] ? 1 : 0;
            }
            
            dx = x + 1;
            dy = y + 1;
            if (dx >= 0 && dx < gridSize && dy >= 0 && dy < gridSize) {
                count += cellsGrid[dy * gridSize + dx] ? 1 : 0;
            }
            return count;
        }
    }
}