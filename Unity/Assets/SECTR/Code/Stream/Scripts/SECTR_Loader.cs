// Copyright (c) 2014 Make Code Now! LLC

using UnityEngine;
using System.Collections;

/// \ingroup Stream
/// Provides an abstract base class for classes that load
/// data from SECTR_Chunk components.
/// 
/// Classes are not required to derive from SECTR_Loader in
/// order to (un)load chunk data. This class merely provides
/// common functionality useful in many of the built in
/// Loaders.
public abstract class SECTR_Loader : MonoBehaviour 
{
	#region Private Details
	protected bool locked = false;
	#endregion

	#region Public Interface
	/// Returns true if all referenced Chunks are loaded. False, otherwise.
	public abstract bool Loaded { get; }
	#endregion

	#region Private Methods
	protected void LockSelf(bool lockSelf)
	{		
		if(lockSelf != locked)
		{
			/*
			Behaviour[] behaviors = GetComponentsInChildren<Behaviour>();
			int numBehaviors = behaviors.Length;
			for(int behaviorIndex = 0; behaviorIndex < numBehaviors; ++behaviorIndex)
			{
				Behaviour behavior = behaviors[behaviorIndex];
				if(behavior != this && behavior.GetType() != typeof(SECTR_Member))
				{
					behavior.enabled = !lockSelf;
				}
			}
			*/

			Rigidbody[] bodies =  GetComponentsInChildren<Rigidbody>();
			int numBodies = bodies.Length;
			for(int bodyIndex = 0; bodyIndex < numBodies; ++bodyIndex)
			{
				Rigidbody body = bodies[bodyIndex];
				if(lockSelf)
				{
					body.Sleep();
				}
				else
				{
					body.WakeUp();
				}
			}

			Collider[] colliders =  GetComponentsInChildren<Collider>();
			int numColliders = colliders.Length;
			for(int colliderIndex = 0; colliderIndex < numColliders; ++colliderIndex)
			{
				colliders[colliderIndex].enabled = !lockSelf;
			}

			locked = lockSelf;
		}
	}
	#endregion
}
