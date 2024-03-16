using Unity.Entities;
using UnityEngine;

namespace ConwayGoL.Authoring {
    public class CellAuthoring : MonoBehaviour {
        private class CellAuthoringBaker : Baker<CellAuthoring> {
            public override void Bake(CellAuthoring authoring) {
                var entity = GetEntity(authoring, TransformUsageFlags.Renderable);
                AddComponent(entity, new CellPosition());
            }
        }
    }
    
    public struct CellPosition: IComponentData {
        public ushort X;
        public ushort Y;
    }
}