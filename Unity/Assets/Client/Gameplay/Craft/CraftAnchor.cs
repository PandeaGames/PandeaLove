using UnityEngine;
using System.Collections;

public class CraftAnchor : MonoBehaviour
{
    [SerializeField]
    private Transform _craftTransform;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(_craftTransform.position.x, transform.position.y, transform.position.z);
    }
}
