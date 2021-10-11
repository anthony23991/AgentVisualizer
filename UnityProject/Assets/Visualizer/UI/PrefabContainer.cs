using UnityEngine;

namespace Visualizer.UI
{
    public class PrefabContainer : MonoBehaviour
    {
        private static PrefabContainer _instance;

        public static PrefabContainer Instance
        {
            get
            {
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this) // more than one instance, suicide!!!
            {
                Destroy(this.gameObject);
            }
            else // we are the first instance, assign ourselves
            {
                _instance = this;
                DontDestroyOnLoad(this.gameObject); // persistent across scene loads
            }
        }
        
        public GameObject tilePrefab;
        public GameObject agentPrefab;
        public GameObject wallPrefab;
        public GameObject wallPrefabPreview;
        public GameObject dirtyPlanePrefab;
        public Texture dirtTexture;
        public GameObject mapReference;
    }
}