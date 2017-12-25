using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SaveOnMembershipChange<T> : MonoBehaviour where T : User {

    [SerializeField]
    private SECTR_Member _member;

    protected abstract UserService<T> GetUserService();

    // Use this for initialization
    void Start () {
        _member.Changed += OnMembershipChange;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnMembershipChange(List<SECTR_Sector> left, List<SECTR_Sector> joined)
    {
        GetUserService().Save();
    }
}
