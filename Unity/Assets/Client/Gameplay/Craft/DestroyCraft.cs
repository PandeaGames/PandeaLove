using UnityEngine;
using System.Collections;

public class DestroyCraft : MonoBehaviour
{
    [SerializeField]
    private CraftAnchor _craftAnchor;

    public void OnTriggerEnter2D(Collider2D other)
    {
        Craft craft = other.attachedRigidbody.GetComponent<Craft>();

        if (craft != null)
        {
            Destroy(_craftAnchor);
            craft.DestroyCraft();
        }
    }
}
