using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpriteRendererGroupTint : MonoBehaviour
{
    [SerializeField]
    private Color _color;

    private SpriteRenderer[] _spriteRenderers;

    // Use this for initialization
    void Awake()
    {
        _spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
    }

    // Use this for initialization
    void Start()
    {
        Tint();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Tint()
    {
        foreach (SpriteRenderer spriteRenderer in _spriteRenderers)
        {
            spriteRenderer.color = _color;
        }
    }
}
