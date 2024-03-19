using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

namespace GameOfLife
{
    [UpdateAfter(typeof(SimulateCellsSystem_MainThread))]
    [RequireMatchingQueriesForUpdate]
    public partial struct DrawCellsSystem_MainThread : ISystem
    {
        [BurstCompile]
        void ISystem.OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Execute.MainThread>();
        }

        [BurstCompile]
        void ISystem.OnUpdate(ref SystemState state)
        {
            foreach (var (cell, colorProperty) in SystemAPI.Query<RefRW<Cell>, RefRW<URPMaterialPropertyBaseColor>>())
            {
                colorProperty.ValueRW.Value = cell.ValueRO.IsAliveNext ? new float4(0f, 1f, 0f, 1f) : new float4(0f, 0f, 0f, 1f);
                cell.ValueRW.IsAlive = cell.ValueRO.IsAliveNext;
            }
        }
    }

    [UpdateAfter(typeof(SimulateCellsSystem_SingleThreadedJob))]
    [RequireMatchingQueriesForUpdate]
    public partial struct DrawCellsSystem_SingleThreadedJob : ISystem
    {
        [BurstCompile]
        void ISystem.OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Execute.SingleThreadedJob>();
        }

        [BurstCompile]
        void ISystem.OnUpdate(ref SystemState state)
        {
            state.Dependency = new DrawCellsJob().Schedule(state.Dependency);
        }

        [BurstCompile]
        private partial struct DrawCellsJob : IJobEntity
        {
            public void Execute(ref Cell cell, ref URPMaterialPropertyBaseColor colorProperty)
            {
                colorProperty.Value = cell.IsAliveNext ? new float4(0f, 1f, 0f, 1f) : new float4(0f, 0f, 0f, 1f);
                cell.IsAlive = cell.IsAliveNext;
            }
        }
    }

    [UpdateAfter(typeof(SimulateCellsSystem_ParallelJob))]
    [RequireMatchingQueriesForUpdate]
    public partial struct DrawCellsSystem_ParallelJob : ISystem
    {
        [BurstCompile]
        void ISystem.OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<Execute.ParallelJob>();
        }

        [BurstCompile]
        void ISystem.OnUpdate(ref SystemState state)
        {
            state.Dependency = new DrawCellsJob().ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        private partial struct DrawCellsJob : IJobEntity
        {
            public void Execute(ref Cell cell, ref URPMaterialPropertyBaseColor colorProperty)
            {
                colorProperty.Value = cell.IsAliveNext ? new float4(0f, 1f, 0f, 1f) : new float4(0f, 0f, 0f, 1f);
                cell.IsAlive = cell.IsAliveNext;
            }
        }
    }
}
