using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PandeaSaveOnMembershipChange : SaveOnMembershipChange<PandeaUser> {

    [SerializeField]
    private PandeaUserService _userService;

    private PandeaUser _user;
    private GameProgress _progress;

    // Use this for initialization
    public override void Start()
    {
        _user = _userService.User;
        _progress = _user.GameProgress;
        base.Start();
    }


    protected override UserService<PandeaUser> GetUserService()
    {
        return _userService;
    }

    protected override void OnMembershipChange(List<SECTR_Sector> left, List<SECTR_Sector> joined)
    {
        if (joined == null)
            return;

        if (joined.Count == 0)
        {
            base.OnMembershipChange(left, joined);
            return;
        }

        SECTR_Sector sector = joined[0];

        _progress.Sector = sector.name;

        base.OnMembershipChange(left, joined);
    }
}