// ******************************************************************
// @file       UpdateEntityActiveSystem.cs
// @brief      隐藏不该显示的物体
// @author     SamuelZon, zonsamuel@gmail.com
//             
// @Modified   2024-08-02
// @Copyright  Copyright (c) 2024
// ******************************************************************

using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;

namespace Script.Dots.Extensions.SetActive
{
    [UpdateInGroup(typeof(PresentationSystemGroup)), UpdateBefore(typeof(HybridLightBakingDataSystem))]
    public partial struct UpdateEntityActiveSystem : ISystem
    {
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<DisableActive>();
        }

        public void OnUpdate(ref SystemState state) {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (_, entity) in SystemAPI.Query<RefRO<DisableActive>>().WithEntityAccess()) {
                ecb.SetEnabled(entity, false);
            }

            ecb.Playback(state.EntityManager);
        }
    }
}