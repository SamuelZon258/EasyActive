// ******************************************************************
// @file       EasyActiveSystem.cs
// @brief      UI中简单的设置实体开关
// @author     SamuelZon, zonsamuel@gmail.com
//             
// @Modified   2024-08-02
// @Copyright  Copyright (c) 2024
// ******************************************************************

using Unity.Collections;
using Unity.Entities;

namespace Script.Dots.Extensions.SetActive
{
    public partial struct EasyActiveJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<Disabled> DisabledLookUp;
        [ReadOnly] public ComponentLookup<EasyEntityData> EasyEntityDataLookUp;
        [ReadOnly] public ComponentLookup<DisableActive> DisableActiveLookUp;
        public EntityCommandBuffer.ParallelWriter ECB;

        void Execute([EntityIndexInQuery] int entityInQueryIndex, Entity entity,
            DynamicBuffer<LinkedEntityGroup> group) {
         
            var enable = EasyEntityDataLookUp.GetEnabledRefRO<EasyEntityData>(entity).ValueRO;
            if (enable) {
                if (DisabledLookUp.HasComponent(entity)) {
                    ECB.SetActive(entityInQueryIndex, entity, enable);
                    foreach (var linkedEntityGroup in group) {
                        if (EasyEntityDataLookUp.HasComponent(linkedEntityGroup.Value) &&
                            !DisableActiveLookUp.HasComponent(linkedEntityGroup.Value)) {
                            ECB.SetComponentEnabled<EasyEntityData>(entityInQueryIndex, linkedEntityGroup.Value,
                                enable);
                        }
                    }
                }
            }
            else {
                if (!DisabledLookUp.HasComponent(entity)) {
                    ECB.SetActive(entityInQueryIndex, entity, enable);
                    foreach (var linkedEntityGroup in group) {
                        if (EasyEntityDataLookUp.HasComponent(linkedEntityGroup.Value)) {
                            ECB.SetComponentEnabled<EasyEntityData>(entityInQueryIndex, linkedEntityGroup.Value,
                                enable);
                        }
                    }
                }
            }
        }
    }

    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public partial class EasyActiveSystem : SystemBase
    {
        private NativeArray<EntityQuery> _query;
        private EndSimulationEntityCommandBufferSystem _ecbSystem;

        protected override void OnCreate() {
            _query = new NativeArray<EntityQuery>(2, Allocator.Persistent);
            _query[0] = GetEntityQuery(new EntityQueryBuilder(Allocator.Temp).WithAll<LinkedEntityGroup>()
                .WithAll<EasyEntityData>()
                .WithAll<Disabled>());
            _query[1] = GetEntityQuery(new EntityQueryBuilder(Allocator.Temp).WithAll<LinkedEntityGroup>()
                .WithDisabled<EasyEntityData>());
            _ecbSystem = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnDestroy() {
            if (_query.IsCreated) {
                _query.Dispose(Dependency);
            }
        }

        protected override void OnUpdate() {
            var disabledLookUp = SystemAPI.GetComponentLookup<Disabled>(true);
            var easyEntityDataLookUp = SystemAPI.GetComponentLookup<EasyEntityData>(true);
            var disableActiveLookUp = SystemAPI.GetComponentLookup<DisableActive>(true);
            disabledLookUp.Update(this);
            easyEntityDataLookUp.Update(this);
            disableActiveLookUp.Update(this);
            foreach (var entityQuery in _query) {
                if (entityQuery.CalculateEntityCount() > 0) {
                    var job = new EasyActiveJob
                    {
                        ECB = _ecbSystem.CreateCommandBuffer().AsParallelWriter(),
                        DisabledLookUp = disabledLookUp,
                        EasyEntityDataLookUp = easyEntityDataLookUp,
                        DisableActiveLookUp = disableActiveLookUp,
                    };
                    Dependency = job.ScheduleParallel(entityQuery, Dependency);
                }
            }

            _ecbSystem.AddJobHandleForProducer(Dependency);
        }
    }
}