using System;
using UnityEngine;

namespace Nodus.ECS {
    // Optional entry point for the same ECS JSON used by buttons and tests.
    public sealed class EcsUrlLauncher : MonoBehaviour {
        [SerializeField] public Nodus.ECS.Definitions.Catalog Catalog;
        [SerializeField] public bool AllowEmbeddedData;

        void Start() {
            var url = Application.absoluteURL;
            if (string.IsNullOrEmpty(url))
                return;

            var reference = url.Contains("?definition=") || url.Contains("&definition=");
            var embedded = url.Contains("?data=") || url.Contains("&data=");
            if (!reference && !embedded)
                return;

            if (embedded && !AllowEmbeddedData) {
                Debug.LogWarning("Nodus.ECS: Embedded URL actions disabled. Enable AllowEmbeddedData to run JSON from URLs.", this);
                return;
            }

            try {
                EcsRuntime.Spawn(EcsUrls.Parse(url, Catalog));
            } catch (Exception exception) {
                Debug.LogException(exception, this);
            }
        }
    }
}
