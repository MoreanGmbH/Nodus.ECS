using Friflo.Engine.Unity;
using UnityEngine;

namespace Nodus.ECS {
    [DefaultExecutionOrder(-500)]
    [RequireComponent(typeof(ECSStore))]
    public sealed class EcsStoreBinding : MonoBehaviour {
        [SerializeField] string storeId = "Store";
        public string StoreId => storeId;

        // Registers this ECSStore under a stable name used by entity definitions and JSON.
        void Awake() => EcsRegistry.RegisterStore(storeId, GetComponent<ECSStore>().EntityStore);

        void OnDestroy() {
            var store = GetComponent<ECSStore>();
            if (store)
                EcsRegistry.UnregisterStore(storeId, store.EntityStore);
        }
    }
}
