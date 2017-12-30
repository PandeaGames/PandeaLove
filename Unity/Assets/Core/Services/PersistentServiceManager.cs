using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistentServiceManager : ServiceManager {

	// Use this for initialization
	public override void Awake () {
        DontDestroyOnLoad(gameObject);
        base.Awake();
	}
}
