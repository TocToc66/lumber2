using UnityEngine;

namespace Lumber.Core
{
    /// Boots the whole game world automatically the moment Play starts, with zero
    /// manual scene setup: no prefabs, no GameObjects to wire in the Inspector.
    public static class GameBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (Object.FindObjectOfType<GameManager>() != null)
                return;

            var root = new GameObject("GameManager");
            root.AddComponent<GameManager>();
        }
    }
}
