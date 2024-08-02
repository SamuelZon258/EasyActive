// ******************************************************************
// @file       UpdateEntityHideSystem.cs
// @brief      辅助隐藏父级还在隐藏着的物体
// @author     SamuelZon, zonsamuel@gmail.com
//             
// @Modified   2024-08-02
// @Copyright  Copyright (c) 2024
// ******************************************************************

using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;

namespace Script.Dots.Extensions.SetActive
{
    [UpdateInGroup(typeof(PresentationSystemGroup)), UpdateBefore(typeof(HybridLightBakingDataSystem))]
    public partial class UpdateEntityHideSystem : SystemBase
    {
        protected override void OnCreate() {
            RequireForUpdate<UpdateDisableInfoTag>();
        }

        protected override void OnUpdate() {
            var ecbSystem = new EntityCommandBuffer(Allocator.TempJob);
            var ecb = ecbSystem.AsParallelWriter();
            var disabledLookup = SystemAPI.GetComponentLookup<Disabled>(true);
            var disableActiveLookup = SystemAPI.GetComponentLookup<DisableActive>(true);
            var parentLookup = SystemAPI.GetComponentLookup<Parent>(true);
            disabledLookup.Update(this);
            disableActiveLookup.Update(this);
            parentLookup.Update(this);
            Dependency = Entities.WithAll<UpdateDisableInfoTag>().ForEach(
                    (int entityInQueryIndex, Entity entity) =>
                    {
                        var parentEntity = entity;
                        do {
                            if (parentLookup.TryGetComponent(parentEntity, out var newParent)) {
                                parentEntity = newParent.Value;
                            }
                            else {
                                parentEntity = Entity.Null;
                            }
                        } while (parentEntity != Entity.Null && !disableActiveLookup.HasComponent(parentEntity));

                        if (parentEntity != Entity.Null && disabledLookup.HasComponent(parentEntity)) {
                            ecb.SetEnabled(entityInQueryIndex, entity, false);
                        }

                        ecb.RemoveComponent<UpdateDisableInfoTag>(entityInQueryIndex, entity);
                    })
                .WithReadOnly(disabledLookup)
                .WithReadOnly(disableActiveLookup)
                .WithReadOnly(parentLookup)
                .ScheduleParallel(Dependency);
            Dependency.Complete();
            ecbSystem.Playback(EntityManager);
        }
    }
}