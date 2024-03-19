using Unity.Entities;
using UnityEngine;

namespace GameOfLife
{
    public class SpawnCellsConfigAuthoring : MonoBehaviour
    {
        [SerializeField] private GameObject cellPrefab;
        [SerializeField] private int width;
        [SerializeField] private int height;

        private class Baker : Baker<SpawnCellsConfigAuthoring>
        {
            public override void Bake(SpawnCellsConfigAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new SpawnCellsConfig()
                {
                    CellPrefabEntity = GetEntity(authoring.cellPrefab, TransformUsageFlags.Dynamic),
                    Width = authoring.width,
                    Height = authoring.height,
                });
            }
        }
    }

    public struct SpawnCellsConfig : IComponentData
    {
        public Entity CellPrefabEntity;
        public int Width;
        public int Height;
    }
}
