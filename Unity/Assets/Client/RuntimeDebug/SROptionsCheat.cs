using System.ComponentModel;
using UnityEngine;

public partial class SROptions
{
    // Default Value for property
    private float _myProperty = 0.5f;

    // Options will be grouped by category
    [Category("My Category")]
    public float MyProperty
    {
        get { return _myProperty; }
        set { _myProperty = value; }
    }

    private float _myRangeProperty = 0f;

    // The NumberRange attribute will ensure that the value never leaves the range 0-10
    [SROptions.NumberRange(0, 10)]
    [Category("My Category")]
    public float MyRangeProperty
    {
        get { return _myRangeProperty; }
        set { _myRangeProperty = value; }
    }
}