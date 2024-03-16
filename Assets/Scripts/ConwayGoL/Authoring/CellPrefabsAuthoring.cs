using Unity.Entities;
using UnityEngine;

namespace ConwayGoL.Authoring {
    public class CellPrefabsAuthoring : MonoBehaviour {
        
        [SerializeField] private GameObject colorCellPrefab;

        private class ConfigAuthoringBaker : Baker<CellPrefabsAuthoring> {
            public override void Bake(CellPrefabsAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new CellPrefabs {
                    CellColorPrefab = GetEntity(authoring.colorCellPrefab, TransformUsageFlags.Renderable)
                });
            }
        }
    }
    
    public struct CellPrefabs : IComponentData {
        public Entity CellColorPrefab;
    }
}