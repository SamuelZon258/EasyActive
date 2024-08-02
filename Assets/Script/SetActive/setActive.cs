// ******************************************************************
// @file       setActive.cs
// @brief      辅助显示隐藏
// @author     SamuelZon, zonsamuel@gmail.com
//             
// @Modified   2024-08-02
// ******************************************************************

using System.Runtime.CompilerServices;
using Unity.Entities;

namespace Script.Dots.Extensions.SetActive
{
    public static partial class setActive
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetActive(this EntityManager manager, Entity entity, bool show) {
            if (entity == Entity.Null) {
                return;
            }

            if (show) {
                manager.RemoveComponent<DisableActive>(entity);
                manager.AddComponent<UpdateDisableInfoTag>(entity);
            }
            else {
                manager.AddComponent<DisableActive>(entity);
            }

            manager.SetEnabled(entity, show);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetActive(this EntityCommandBuffer ecb, Entity entity, bool show) {
            if (entity == Entity.Null) {
                return;
            }

            if (show) {
                ecb.RemoveComponent<DisableActive>(entity);
                ecb.AddComponent<UpdateDisableInfoTag>(entity);
            }
            else {
                ecb.AddComponent<DisableActive>(entity);
            }

            ecb.SetEnabled(entity, show);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetActive(this EntityCommandBuffer.ParallelWriter ecb, int sortKey, Entity entity,
            bool show) {
            if (entity == Entity.Null) {
                return;
            }

            if (show) {
                ecb.RemoveComponent<DisableActive>(sortKey, entity);
                ecb.AddComponent<UpdateDisableInfoTag>(sortKey, entity);
            }
            else {
                ecb.AddComponent<DisableActive>(sortKey, entity);
            }

            ecb.SetEnabled(sortKey, entity, show);
        }
    }
}