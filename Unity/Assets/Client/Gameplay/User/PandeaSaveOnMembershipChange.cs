using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PandeaSaveOnMembershipChange : SaveOnMembershipChange<PandeaUser> {

    [SerializeField]
    private PandeaUserService _userService;

    protected override UserService<PandeaUser> GetUserService()
    {
        return _userService;
    }
}
