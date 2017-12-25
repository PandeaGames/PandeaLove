// Copyright (c) 2014 Make Code Now! LLC

using UnityEngine;
using System.Collections.Generic;

/// \ingroup Stream
/// Automatically enables and disables components on itself when the SECTR_Sector it's part of are
/// (un)loaded.
/// 
/// It's often useful to have global objects that are always instantiated, even when their
/// part of the scene is not loaded. However, because their area is not loaded, these objets may
/// not want to update until their areas are active again. SECTR_Hibernator takes care of
/// this behavior automatically, taking care of physics and behaviors, while providing optional
/// Events to notify anyone who might be interested in things that happen while hibernated.
[RequireComponent(typeof(SECTR_Member))]
[AddComponentMenu("SECTR/Stream/SECTR Hibernator")]
public class SECTR_Hibernator : MonoBehaviour 
{
	#region Private Details
	private bool hibernating = false;
	private SECTR_Member cachedMember = null;
	private Dictionary<SECTR_Chunk, SECTR_Chunk> chunks = new Dictionary<SECTR_Chunk, SECTR_Chunk>(4);
	private int numLoadedSectors = 0;
	#endregion

	#region Public Interface
	[SECTR_ToolTip("Hibernate components on children as well as ones on this game object.")]
	public bool HibernateChildren = true;
	[SECTR_ToolTip("Disable Behavior components during hibernation.")] 
	public bool HibernateBehaviors = true;
	[SECTR_ToolTip("Disable Collder components during hibernation.")]
	public bool HibernateColliders = true;
	[SECTR_ToolTip("Disable RigidBody components during hibernation.")]
	public bool HibernateRigidBodies = true;
	[SECTR_ToolTip("Hide Render components during hibernation.")]
	public bool HibernateRenderers = true;
	[SECTR_ToolTip("Apply hibernation to an alternate entity.")]
	public GameObject HibernateTarget = null;

	/// Delegate delcaration for anyone who wants to be notified on hibernation related events.
	public delegate void HibernateCallback();
	
	/// Event handler for when we go from hiberanted to awake.
	public event HibernateCallback Awoke;
	/// Event handler for when we go from awake to hibernate.
	public event HibernateCallback Hibernated;
	/// Event handler for updates during hibernation. Use judiciously.
	public event HibernateCallback HibernateUpdate;
	#endregion

	#region Unity Interface
	void OnEnable()
	{
		cachedMember = GetComponent<SECTR_Member>();
		cachedMember.Changed += _MembershipChanged;
		chunks.Clear();
	}

	void OnDisable()
	{
		cachedMember.Changed -= _MembershipChanged;
		cachedMember = null;
		chunks.Clear();
	}
	#endregion

	#region Private Methods
	void _ChunkChanged(SECTR_Chunk source, bool loaded)
	{
		if(loaded)
		{
			++numLoadedSectors;
		}
		else
		{
			--numLoadedSectors;
		}

		_HibernationChanged();
	}

	void _MembershipChanged(List<SECTR_Sector> left, List<SECTR_Sector> joined)
	{
		// Add ref to all of the new objects first so that we don't unload and then immeditately load again.
		if(joined != null)
		{
			int numJoined = joined.Count;
			for(int sectorIndex = 0; sectorIndex < numJoined; ++sectorIndex)
			{
				SECTR_Sector sector = joined[sectorIndex];
				if(sector)
				{
					SECTR_Chunk chunk = sector.GetComponent<SECTR_Chunk>();
					if(chunk && !chunks.ContainsKey(chunk))
					{
						chunk.Changed += _ChunkChanged;
						chunks[chunk] = chunk;
						if(chunk.IsLoaded())
						{
							++numLoadedSectors;
						}
					}
				}
			}
		}

		// Dec ref any sectors we're no longer in.
		if(left != null)
		{
			int numLeft = left.Count;
			for(int sectorIndex = 0; sectorIndex < numLeft; ++sectorIndex)
			{
				SECTR_Sector sector = left[sectorIndex];
				if(sector)
				{
					SECTR_Chunk chunk = sector.GetComponent<SECTR_Chunk>();
					if(chunk && chunks.ContainsKey(chunk))
					{
						chunk.Changed -= _ChunkChanged;
						chunks.Remove(chunk);
						if(chunk.IsLoaded())
						{
							--numLoadedSectors;
						}
					}
				}
			}
		}

		// Always check to see if our hibernation state has changed,
		// to catch startup cases as well as after startup.
		_HibernationChanged();
	}

	void _HibernationChanged()
	{
		if(numLoadedSectors == 0 && !hibernating)
		{
			_Hibernate();
		}
		else if(numLoadedSectors > 0 && hibernating)
		{
			_WakeUp();
		}

		if(hibernating && HibernateUpdate != null)
		{
			HibernateUpdate();
		}
	}

	void _WakeUp()
	{
		if(hibernating)
		{
			hibernating = false;
			_UpdateComponents();
			if(Awoke != null)
			{
				Awoke();
			}
		}
	}

	void _Hibernate()
	{
		if(!hibernating)
		{
			hibernating = true;
			_UpdateComponents();
			if(Hibernated != null)
			{
				Hibernated();
			}
		}
	}

	void _UpdateComponents()
	{
		GameObject hibernatedObject = HibernateTarget ? HibernateTarget : gameObject;

		if(HibernateBehaviors)
		{
			Behaviour[] behaviors = HibernateChildren ? hibernatedObject.GetComponentsInChildren<Behaviour>() : hibernatedObject.GetComponents<Behaviour>();
			int numBehaviors = behaviors.Length;
			for(int behaviorIndex = 0; behaviorIndex < numBehaviors; ++behaviorIndex)
			{
				Behaviour behavior = behaviors[behaviorIndex];
				if(behavior.GetType() != typeof(SECTR_Hibernator) && behavior.GetType() != typeof(SECTR_Member))
				{
					behavior.enabled = !hibernating;
				}
			}
		}

		if(HibernateRigidBodies)
		{
			Rigidbody[] bodies =  HibernateChildren ? hibernatedObject.GetComponentsInChildren<Rigidbody>() : hibernatedObject.GetComponents<Rigidbody>();
			int numBodies = bodies.Length;
			for(int bodyIndex = 0; bodyIndex < numBodies; ++bodyIndex)
			{
				Rigidbody body = bodies[bodyIndex];
				if(hibernating)
				{
					body.Sleep();
				}
				else
				{
					body.WakeUp();
				}
			}
		}

		if(HibernateColliders)
		{
			Collider[] colliders =  HibernateChildren ? hibernatedObject.GetComponentsInChildren<Collider>() : hibernatedObject.GetComponents<Collider>();
			int numColliders = colliders.Length;
			for(int colliderIndex = 0; colliderIndex < numColliders; ++colliderIndex)
			{
				colliders[colliderIndex].enabled = !hibernating;
			}
		}

		if(HibernateRenderers)
		{
			Renderer[] renderers =  HibernateChildren ? hibernatedObject.GetComponentsInChildren<Renderer>() : hibernatedObject.GetComponents<Renderer>();
			int numRenderers = renderers.Length;
			for(int rendererIndex = 0; rendererIndex < numRenderers; ++rendererIndex)
			{
				renderers[rendererIndex].enabled = !hibernating;
			}
		}
	}
	#endregion
}
