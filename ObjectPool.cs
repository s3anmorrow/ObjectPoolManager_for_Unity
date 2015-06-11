using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectPool : MonoBehaviour {

    // public unity component properties
    [System.Serializable]
    public struct PrefabToPool {
        public GameObject prefab;
        public int maximum;
    }
    public PrefabToPool[] prefabsToPool;

    // singleton instance
    public static ObjectPool instance = null;

    // the master dictionary of all object pools
    private Dictionary<string, List<GameObject>> pools;
    // colleciton of all the prefabs for constructing more GameObjects if we run out
    private Dictionary<string, GameObject> prefabs;

    // --------------------------------------------------------------------- event handlers
	// Use this for initialization
	void Awake () {
        if (instance == null) {
            instance = this;
            // sets this to not be destroyed when reloading scene
            DontDestroyOnLoad(gameObject);
            // construct Dictionaries
            pools = new Dictionary<string, List<GameObject>>();
            prefabs = new Dictionary<string, GameObject>();

            // construct object pools
            for (int n = 0; n < prefabsToPool.Length; n++) {
                // construct new pool object
                List<GameObject> newPool = new List<GameObject>();

                // get properties from public component data
                int max = prefabsToPool[n].maximum;
                GameObject prefab = prefabsToPool[n].prefab;
                // store original prefab in dictionary
                prefabs.Add(prefab.name, prefab);

                // instantiate all game objects and add to new pool
                for (int i = 0; i < max; i++) {
                    GameObject obj = (GameObject)Instantiate(prefab);
                    // set it so that gameobjects aren't destroyed if scene changes (defeats whole purpose of object pooling!)
                    DontDestroyOnLoad(obj);
                    obj.SetActive(false);
                    newPool.Add(obj);
                }

                // add new pool (List) to the dictionary with key being the prefab's name
                pools.Add(prefab.name, newPool);
            }

        } else if (instance != this) {
            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy(gameObject);
        }
        
	}

    // -------------------------------------------------------------------- public methods
    // instantiate GameObject from object pool and return it
    public GameObject get(string prefabName) {
        // get pool out of dictionary
        List<GameObject> pool = pools[prefabName];

        // find gameObject that isn't active and return it
        foreach (GameObject obj in pool) {
            if (!obj.activeInHierarchy) {
                obj.SetActive(true);
                return obj;
            }
		}

        // we have run out of pooled game objects - danger, danger!
        GameObject prefab = prefabs[prefabName];
        GameObject newObj = (GameObject)Instantiate(prefab);
        // set it so that gameobjects aren't destroyed if scene changes (defeats whole purpose of object pooling!)
        DontDestroyOnLoad(newObj);
        newObj.SetActive(false);
        pool.Add(newObj);
        return get(prefabName);
    }

    // instantiate GameObject from object pool and position/rotate (much like Instantiate()) and return it
    public GameObject get(string prefabName, Vector3 position, Quaternion rotation) {
        GameObject obj = get(prefabName);
        // position and rotate just like Instantiate
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        return obj;
    }

    // instantiate GameObject from object pool but return one of its components based on the name
    public Component getComponent(string prefabName, string scriptName) {
        GameObject obj = get(prefabName);
        return obj.GetComponent(scriptName);
    }

    // return the gameobject back to the pool
    public void dispose(GameObject obj) {
        obj.SetActive(false);
    }

    // dispose all the gameObjects of the given prefabName
    public void dispose(string prefabName) {
        // get pool out of dictionary
        List<GameObject> pool = pools[prefabName];
        // find gameObject that isn't active and return it
        foreach (GameObject obj in pool) {
            dispose(obj);
        }
    }

}
