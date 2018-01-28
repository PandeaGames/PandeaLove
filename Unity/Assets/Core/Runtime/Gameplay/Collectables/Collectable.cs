using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour {

    public delegate void CollectedDelegate(GameObject debry);
    public event CollectedDelegate OnCollected;

    [SerializeField]
    private ServiceManager _serviceManager;
    [SerializeField]
    private GameObject _debryPrefab;

    private CollectionService _collectionService;

	// Use this for initialization
	void Start ()
    {
        _collectionService = _serviceManager.GetService<CollectionService>();
	}

    public void Collect()
    {
        _collectionService.TryCollect(this);
    }

    public void Collected()
    {
        GameObject debry = null;

        if (_debryPrefab)
            debry = Instantiate(_debryPrefab, transform.position, transform.rotation, transform.parent);

        if (OnCollected != null)
            OnCollected(debry);

        Destroy(gameObject);
    }
}
