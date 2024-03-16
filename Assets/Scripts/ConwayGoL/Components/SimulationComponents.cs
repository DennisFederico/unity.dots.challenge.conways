using Unity.Entities;
using Unity.Mathematics;

namespace ConwayGoL.Components {

    public struct GoLConfig : IComponentData {
        public ExecutionType ExecutionType;
        public int GridSize;
        public float CellSize;
        // public bool FlipOnDeadOrAlive;
        public float4 CellDeadColor;
        public float4 CellAliveColor;
    }
    
    public enum ExecutionType {
        EntityJob,
        MainThread,
    }
    
    public struct CellState : IBufferElementData {
        public bool IsAlive;
    }
    
    public struct MainThreadExecute : IComponentData {
    }
    
    public struct JobEntityExecute : IComponentData {
    }
}