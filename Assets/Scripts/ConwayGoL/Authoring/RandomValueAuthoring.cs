using System.Linq;
using Unity.Entities;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;

namespace ConwayGoL.Authoring {
    public class RandomValueAuthoring : MonoBehaviour {

        [SerializeField] private bool randomizeSeed;
        [SerializeField] private string seed;

        private class RandomValueAuthoringBaker : Baker<RandomValueAuthoring> {
            public override void Bake(RandomValueAuthoring authoring) {
                var seedValue = !string.IsNullOrEmpty(authoring.seed) && !authoring.randomizeSeed ?
                    authoring.seed.Aggregate(0u, (total, next) => total + next) :
                    (uint)Random.Range(1, uint.MaxValue);
                var rand = new Unity.Mathematics.Random(seedValue);
                var entity = GetEntity(TransformUsageFlags.None);
                var buffer = AddBuffer<RandomValue>(entity);
                buffer.Length = JobsUtility.MaxJobThreadCount;
                for (var i = 0; i < buffer.Length; i++) {
                    buffer[i] = new RandomValue { Random = Unity.Mathematics.Random.CreateFromIndex(rand.NextUInt()) };
                }
            }
        }
    }
    
    public struct RandomValue : IBufferElementData {
        public Unity.Mathematics.Random Random;
    }
}