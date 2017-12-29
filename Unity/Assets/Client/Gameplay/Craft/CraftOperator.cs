using UnityEngine;
using System.Collections;

public class CraftOperator : MonoBehaviour
{
    [SerializeField]
    private InputGroup _inputGroup;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnCraftEntered(Craft craft)
    {
        gameObject.SetActive(false);
    }

    public void OnCraftExited(Craft craft)
    {
        gameObject.SetActive(true);
    }

    public void OnCollisionEnter2D(Collision2D coll)
    {
        Craft craft = coll.gameObject.GetComponent<Craft>();

        if (craft != null)
            craft.AttemptEnterCraft(this);
    }
}
