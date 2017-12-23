// Copyright (c) 2014 Make Code Now! LLC

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// \ingroup Demo
/// A simple FPS style character controller.
/// 
/// Extends the FP Controller to translate input 
/// input into movement that a Character Motor can understand. 
[RequireComponent(typeof(SECTR_CharacterMotor))]
[AddComponentMenu("SECTR/Demos/SECTR Character Controller")]
public class SECTR_FPSController : SECTR_FPController
{
	#region Private Details
    private SECTR_CharacterMotor cachedMotor;
	#endregion

	#region Unity Interface
    void Awake()
    {
        cachedMotor = GetComponent<SECTR_CharacterMotor>();
    }

    // Update is called once per frame
    protected override void Update()
    {
		base.Update();

		Vector3 directionVector;
		if(Input.multiTouchEnabled && !Application.isEditor)
		{
			directionVector = GetScreenJoystick(false);
		}
		else
		{
			// Get the input vector from keyboard or analog stick
			directionVector = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);
		}

        if (directionVector != Vector3.zero)
        {
            // Get the length of the directon vector and then normalize it
            // Dividing by the length is cheaper than normalizing when we already have the length anyway
            var directionLength = directionVector.magnitude;
            directionVector = directionVector / directionLength;

            // Make sure the length is no bigger than 1
            directionLength = Mathf.Min(1, directionLength);

            // Make the input vector more sensitive towards the extremes and less sensitive in the middle
            // This makes it easier to control slow speeds when using analog sticks
            directionLength = directionLength * directionLength;

            // Multiply the normalized direction vector by the modified length
            directionVector = directionVector * directionLength;
        }

        // Rotate the input vector into local space so up is lcoal up and right is local right
        directionVector = transform.rotation * directionVector;

        // Rotate input vector to be perpendicular to character's up vector
        Quaternion camToCharacterSpace = Quaternion.FromToRotation(-transform.forward, transform.up);
        directionVector = (camToCharacterSpace * directionVector);

        // Apply the direction to the CharacterMotor
        cachedMotor.inputMoveDirection = directionVector;
        cachedMotor.inputJump = Input.GetButton("Jump");
    }
	#endregion

	#region Private Methods
    private Vector3 ProjectOntoPlane(Vector3 v, Vector3 normal)
    {
        return v - Vector3.Project(v, normal);
    }

    private Vector3 ConstantSlerp(Vector3 from, Vector3 to, float angle)
    {
        float value = Mathf.Min(1, angle / Vector3.Angle(from, to));
        return Vector3.Slerp(from, to, value);
    }
	#endregion
}