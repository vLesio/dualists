using UnityEngine;

namespace Singleton {
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour {
        public static T I { get; private set; }
        
        protected virtual void Awake() {
            if (I != null) {
                throw new SingletonOverrideException($"[Singleton] Tried to overwrite {typeof(T)} singleton.");
            }
            I = FindObjectOfType<T>();
            //Debug.Log($"[Singleton] Created singleton instance for {typeof(T)}", gameObject);
        }
    }
}

