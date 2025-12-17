using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Generic object pool for reusing GameObjects.
/// </summary>
public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance { get; private set; }
    
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }
    
    [SerializeField] private List<Pool> _pools;
    
    private Dictionary<string, Queue<GameObject>> _poolDictionary;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        InitializePools();
    }
    
    private void InitializePools()
    {
        _poolDictionary = new Dictionary<string, Queue<GameObject>>();
        
        foreach (var pool in _pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();
            
            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab, transform);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }
            
            _poolDictionary.Add(pool.tag, objectPool);
        }
    }
    
    public GameObject Spawn(string tag, Vector3 position, Quaternion rotation)
    {
        if (!_poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"[ObjectPool] Pool with tag '{tag}' doesn't exist.");
            return null;
        }
        
        Queue<GameObject> pool = _poolDictionary[tag];
        
        GameObject obj;
        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
        }
        else
        {
            // Pool exhausted, find the prefab and create new
            Pool poolData = _pools.Find(p => p.tag == tag);
            if (poolData == null) return null;
            obj = Instantiate(poolData.prefab, transform);
        }
        
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);
        
        return obj;
    }
    
    public void Return(string tag, GameObject obj)
    {
        if (!_poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"[ObjectPool] Pool with tag '{tag}' doesn't exist. Destroying object.");
            Destroy(obj);
            return;
        }
        
        obj.SetActive(false);
        _poolDictionary[tag].Enqueue(obj);
    }
}
