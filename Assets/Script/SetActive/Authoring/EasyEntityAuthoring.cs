using Unity.Entities;
using Unity.Entities.Hybrid.Baking;
using UnityEngine;

namespace Script.Dots.Extensions.SetActive.Authoring
{
    [RequireComponent(typeof(LinkedEntityGroupAuthoring))]
    public class EasyEntityAuthoring : MonoBehaviour
    {
        public bool show;

        private class EasyEntityAuthoringBaker : Baker<EasyEntityAuthoring>
        {
            public override void Bake(EasyEntityAuthoring authoring) {
                var entity = GetEntity();
                AddComponent<EasyEntityData>(entity);
                SetComponentEnabled<EasyEntityData>(entity, authoring.show);
            }
        }
    }
}