// Copyright (c) 2014 Make Code Now! LLC
#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7
#define UNITY_4
#endif

#if UNITY_4 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 || UNITY_5_3
#define UNITY_OLD_PARTICLES
#endif

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;

/// \ingroup Core
/// Member represents anything that can be part of a SECTR_Sector, including Sectors themselves. 
/// 
/// Member's primary job is to figure out which Sectors a given GameObject belongs to. In
/// order to accomplish this goal, it needs to compute a bounding box (actually several) and to
/// periodically check to see which Sectors overlap that box. SectorMember also caches information
/// that other clients are interested in, such as the aggregate render bounds of its children,
/// or a list of child Light components.
/// 
/// Members may be dynamic or static, and will save some per-frame CPU work if marked as
/// static. Care should be taken that static Members by accurately marked, as moving children
/// of a static Member may produce unexpected results.
/// 
/// Note that Members may have children that are themselves Members. The code will
/// automatically detect this case and not include the child Member's children in its list.
/// This can be a very convienient way to control the granuality of scene partitioning, especially
/// for culling and streaming.
[ExecuteInEditMode]
[AddComponentMenu("SECTR/Core/SECTR Member")]
public class SECTR_Member : MonoBehaviour
{	
	#region Private Details
	[SerializeField] [HideInInspector] private List<Child> children = new List<Child>(16);
	[SerializeField] [HideInInspector] private List<Child> renderers = new List<Child>(16);
	[SerializeField] [HideInInspector] private List<Child> lights = new List<Child>(16);
	[SerializeField] [HideInInspector] private List<Child> terrains = new List<Child>(2);
	[SerializeField] [HideInInspector] private List<Child> shadowLights = SECTR_Modules.VIS ? new List<Child>(16) : null;
	[SerializeField] [HideInInspector] private List<Child> shadowCasters = SECTR_Modules.VIS ? new List<Child>(16) : null;

	[SerializeField] [HideInInspector] private Bounds totalBounds;
	[SerializeField] [HideInInspector] private Bounds renderBounds;
	[SerializeField] [HideInInspector] private Bounds lightBounds;
	[SerializeField] [HideInInspector] private bool hasRenderBounds = false;
	[SerializeField] [HideInInspector] private bool hasLightBounds = false;
	[SerializeField] [HideInInspector] private bool shadowCaster = false;
	[SerializeField] [HideInInspector] private bool shadowLight = false;

	// These definitely need to be serialized
	[SerializeField] [HideInInspector] private bool frozen = false;
	[SerializeField] [HideInInspector] private bool neverJoin = false;
	[SerializeField] [HideInInspector] protected List<Light> bakedOnlyLights = SECTR_Modules.VIS ? new List<Light>(8) : null;

	// Transient state data, should not be serialized.
	protected bool isSector = false;
	protected SECTR_Member childProxy = null;
	protected bool hasChildProxy = false;
	private bool started = false;
	private bool usedStartSector = false;
	private List<SECTR_Sector> sectors = new List<SECTR_Sector>(4);
	private List<SECTR_Sector> newSectors = new List<SECTR_Sector>(4);
	private List<SECTR_Sector> leftSectors = new List<SECTR_Sector>(4);
	private Dictionary<Transform, Child> childTable = new Dictionary<Transform, Child>(8);
	private Dictionary<Light, Light> bakedOnlyTable = null;
	private Vector3 lastPosition = Vector3.zero;
	private Stack<Child> childPool = new Stack<Child>(32);

	private static List<SECTR_Member> allMembers = new List<SECTR_Member>(256);
	private static Dictionary<Transform, SECTR_Member> allMemberTable = new Dictionary<Transform, SECTR_Member>(256);
	#endregion

	#region Public Interface
	/// Simple data structure to represent the important information about one of the
	/// children of a SECTR_Member.
	[System.Serializable]
	public class Child
	{
		public void Init(GameObject gameObject, Renderer renderer, Light light, Terrain terrain, SECTR_Member member, bool dirShadowCaster, Vector3 shadowVec)
		{
			this.gameObject = gameObject;
			this.gameObjectHash = this.gameObject.GetInstanceID();
			this.member = member;
			this.renderer = renderer && (renderCulled || renderer.enabled) ? renderer : null;
			this.light = (light && (lightCulled || light.enabled) && (light.type == LightType.Point || light.type == LightType.Spot)) ? light : null;
			this.terrain = (terrain && (terrainCulled || terrain.enabled)) ? terrain : null;
			rendererBounds = this.renderer ? this.renderer.bounds : new Bounds();
			lightBounds = this.light ? SECTR_Geometry.ComputeBounds(this.light) : new Bounds();
			terrainBounds = this.terrain ? SECTR_Geometry.ComputeBounds(this.terrain) : new Bounds();
			layer = gameObject.layer;

			if(SECTR_Modules.VIS)
			{
				this.renderHash = this.renderer ? this.renderer.GetInstanceID() : 0;
				this.lightHash = this.light ? this.light.GetInstanceID() : 0;
				this.terrainHash = this.terrain ? this.terrain.GetInstanceID() : 0;

				bool liveLightmaps;
				#if UNITY_4
				liveLightmaps = LightmapSettings.lightmapsMode == LightmapsMode.Dual;
				#else
				liveLightmaps = true;
				#endif

				#if UNITY_4_0 || UNITY_4_1
				shadowLight = this.light && light.shadows != LightShadows.None;
				#elif UNITY_5_4_OR_NEWER
				shadowLight = this.light && light.shadows != LightShadows.None && (!light.isBaked || liveLightmaps);
				#else
				shadowLight = this.light && light.shadows != LightShadows.None && (!light.alreadyLightmapped || liveLightmaps);
				#endif
#if UNITY_4
				rendererCastsShadows = this.renderer && renderer.castShadows && (renderer.lightmapIndex == -1 || liveLightmaps);
#else
				rendererCastsShadows = this.renderer && renderer.shadowCastingMode != UnityEngine.Rendering.ShadowCastingMode.Off && (renderer.lightmapIndex == -1 || liveLightmaps);
#endif
				terrainCastsShadows = this.terrain && terrain.castShadows && (terrain.lightmapIndex == -1 || liveLightmaps);

				if(dirShadowCaster)
				{
					if(rendererCastsShadows)
					{
						rendererBounds = SECTR_Geometry.ProjectBounds(rendererBounds, shadowVec);
					}
					if(terrainCastsShadows)
					{
						terrainBounds = SECTR_Geometry.ProjectBounds(terrainBounds, shadowVec);
					}
				}

				if(shadowLight)
				{
					shadowLightPosition = light.transform.position;
					shadowLightRange = light.range;
					shadowLightType = light.type;
					shadowCullingMask = light.cullingMask;
				}
				else
				{
					shadowLightPosition = Vector3.zero;
					shadowLightRange = 0;
					shadowLightType = LightType.Area;
					shadowCullingMask = 0;
				}
			}
			else
			{
				this.renderHash = 0;
				this.lightHash = 0;
				this.terrainHash = 0;
				shadowLight = false;
				rendererCastsShadows = false;
				terrainCastsShadows = false;
				shadowLightPosition = Vector3.zero;
				shadowLightRange = 0;
				shadowLightType = LightType.Area;
				shadowCullingMask = 0;
			}
			processed = true;
		}

		public override bool Equals(System.Object obj) 
		{
			return obj is Child && this == (Child)obj;
		}

		public override int GetHashCode()
		{
			return gameObjectHash;
		}

		public static bool operator ==(Child x, Child y) 
		{
			if((object)x == null && (object)y == null)
			{
				return true;
			}
			else if((object)x == null || (object)y == null)
			{
				return false;
			}
			else
			{
				return x.gameObjectHash == y.gameObjectHash;
			}
		}

		public static bool operator !=(Child x, Child y) 
		{
			return !(x == y);
		}
			
		/// The game object this member was created from.
		public GameObject gameObject;
		/// Hash of GameObject ID.
		public int gameObjectHash;
		/// The Member this child belongs to.
		public SECTR_Member member;
		/// Renderer component of Member child. Can be null. 
		public Renderer renderer;
		/// Hash of render component ID.
		public int renderHash;
		/// Light component of Member child. Can be null.
		public Light light;
		/// Hash of light component ID.
		public int lightHash;
		/// Terrain component of Member child. Can be null.
		public Terrain terrain;
		/// Hash of terrain component ID
		public int terrainHash;
		/// Cached world space bounds of the renderer component.
		public Bounds rendererBounds;
		/// Cached world space bounds of the light component.
		public Bounds lightBounds;
		/// Cached world space bounds of the terrain component.
		public Bounds terrainBounds;
		/// Cached value of ability to create dynamic shadows.
		public bool shadowLight;
		/// Cached value renderer dynamic shadow casting.
		public bool rendererCastsShadows;
		/// Cached value of terrain ability to cast dynamic shadows.
		public bool terrainCastsShadows;
		// Cached data of shadow casting lights, for use in VIS.
		public LayerMask layer;
		public Vector3 shadowLightPosition;
		public float shadowLightRange;
		public LightType shadowLightType;
		public int shadowCullingMask;
		// Update bookkeeping logic
		public bool processed = false;
		public bool renderCulled = false;
		public bool lightCulled = false;
		public bool terrainCulled = false;
	};

	/// Rules for how often to update the children and bounds.
	public enum BoundsUpdateModes
	{
		/// Compute children on start. Update bounds on movement.
		Start,			
		/// Compute children and bounds on movement.
		Movement,		
		/// Compute children and bounds on every update.
		Always,			
		/// Compute children and bounds on only on start.
		Static,
		/// Children have no effect on the computation of the bounds.
		SelfOnly,
	};

	/// Modes for how to cull children (VIS only)
	public enum ChildCullModes
	{
		/// Cull Sector children individually, Member children as a group.
		Default,
		/// Cull children as a group.
		Group,
		/// Cull each child individually.
		Individual
	};

	[SECTR_ToolTip("Set to true if Sector membership should only change when crossing a portal.")]
	public bool PortalDetermined = false;
	[SECTR_ToolTip("If set, forces the initial Sector to be the specified Sector.", "PortalDetermined")]
	public SECTR_Sector ForceStartSector = null;
	[SECTR_ToolTip("Determines how often the bounds are recomputed. More frequent updates requires more CPU.")]
	public BoundsUpdateModes BoundsUpdateMode = BoundsUpdateModes.Always;
	[SECTR_ToolTip("Adds a buffer on bounding box to compensate for minor imprecisions.")]
	public float ExtraBounds = SECTR_Geometry.kBOUNDS_CHEAT;
	[SECTR_ToolTip("Override computed bounds with the user specified bounds. Advanced users only.")]
	public bool OverrideBounds = false;
	[SECTR_ToolTip("User specified override bounds. Auto-populated with the current bounds when override is inactive.", "OverrideBounds")]
	public Bounds BoundsOverride;
	[SECTR_ToolTip("Optional shadow casting directional light to use in membership calculations. Bounds will be extruded away from light, if set.")]
	public Light DirShadowCaster;
	[SECTR_ToolTip("Distance by which to extend the bounds away from the shadow casting light.", "DirShadowCaster")]
	public float DirShadowDistance = 100;
	[SECTR_ToolTip("Determines if this SectorCuller should cull individual children, or cull all children based on the aggregate bounds.")]
	public ChildCullModes ChildCulling = ChildCullModes.Default;

	[HideInInspector] [System.NonSerialized]
	public int LastVisibleFrameNumber = 0;

	/// Returns a list of all enabled Members.
	public static List<SECTR_Member> All
	{
		get { return allMembers; }
	}

	/// Returns true if each child should be culled individually.
	public bool CullEachChild
	{
		get { return ChildCulling == ChildCullModes.Individual || (ChildCulling == ChildCullModes.Default && isSector); }
	}

	/// Returns all the Sectors that this object belongs to.
	public List<SECTR_Sector> Sectors
	{
		get { return sectors; }
	}

	/// Returns a flattened list of all relevant children.
	public List<Child> Children
	{
		get { return hasChildProxy ? childProxy.children : children; }
	}

	/// Returns the subset of the Children list that contains Renderers.
	public List<Child> Renderers
	{
		get { return hasChildProxy ? childProxy.renderers : renderers; }
	}

	/// Returns true if any child renderer components casts shadows.
	public bool ShadowCaster
	{
		get { return hasChildProxy ? childProxy.shadowCaster : shadowCaster; }
	}

	/// Returns the subset of the Children list that cast shadows.
	public List<Child> ShadowCasters
	{
		get { return hasChildProxy ? childProxy.shadowCasters : shadowCasters; }
	}

	/// Returns the subset of the Children list that contains Lights.
	public List<Child> Lights
	{
		get { return hasChildProxy ? childProxy.lights : lights; }
	}

	/// Returns true if any child light components create shadows.
	public bool ShadowLight
	{
		get { return hasChildProxy ? childProxy.shadowLight : shadowLight; }
	}

	/// Returns the subset of the Children list that create shadows.
	public List<Child> ShadowLights
	{
		get { return hasChildProxy ? childProxy.shadowLights : shadowLights; }
	}

	/// Returns the subset of the Children list that contains Lights.
	public List<Child> Terrains
	{
		get { return hasChildProxy ? childProxy.terrains : terrains; }
	}

	/// Returns the union of the RenderBounds and LightBounds
	public Bounds TotalBounds
	{
		// childProxy is intentionally omitted here so the master scene structure 
		// is preserved when chunks are loaded. Special case, but seems to work ok.
		get { return totalBounds; }
	}

	/// Returns the union of the Bounds of all child Renderers.
	public Bounds RenderBounds
	{
		get { return hasChildProxy ? childProxy.renderBounds : renderBounds; }
	}

	/// Returns true if the RenderBounds contains valid data.
	public bool HasRenderBounds
	{
		get { return hasChildProxy ? childProxy.hasRenderBounds : hasRenderBounds; }
	}

	/// Returns the union of the Bounds of all child Lights.
	public Bounds LightBounds
	{
		get { return hasChildProxy ? childProxy.lightBounds : lightBounds; }
	}

	/// Returns true if the LightBounds contains valid data.
	public bool HasLightBounds
	{
		get { return hasChildProxy ? childProxy.hasLightBounds : hasLightBounds; }
	}

	public bool IsVisibleThisFrame()
	{
		return LastVisibleFrameNumber == Time.frameCount;
	}

	public bool WasVisibleLastFrame() 
	{
		return LastVisibleFrameNumber == Time.frameCount - 1;
	}

	/// (Un)Freezes (i.e. disables updates and preserves bounds).
	public bool Frozen
	{
		set
		{
			// Currently intended for sectors only.
			if(isSector)
			{
				frozen = value;
			}
		}
		get { return frozen; }
	}

	/// Setting the ChildProxy will force this Member to use the children
	/// of another node as if they were its own.
	/// Only to be used in very specific circumstances.
	public SECTR_Member ChildProxy
	{
		set { childProxy = value; hasChildProxy = (childProxy != null); }
	}

	/// Setting to true will prevent this Member from joining any sectors.
	/// Only to be used in very specific circumstances.
	public bool NeverJoin
	{
		set { neverJoin = true; }
	}

	/// Returs if this Member is really a Sector.
	public bool IsSector
	{
		get { return isSector; }
	}

	/// Forces an update. Used only in special cases.
	public void ForceUpdate(bool updateChildren)
	{
		if(updateChildren)
		{
			_UpdateChildren();
		}
		lastPosition = transform.position;
		if(!isSector && !neverJoin)
		{
			_UpdateSectorMembership();
		}
	}

	/// Called when a Sector is being destroyed 
	public void SectorDisabled(SECTR_Sector sector)
	{
		if(sector)
		{
			sectors.Remove(sector);
			if(Changed != null)
			{
				leftSectors.Clear();
				leftSectors.Add(sector);
				Changed(leftSectors, null);
			}
		}
	}

	/// Delegate delcaration for anyone who wants to be notified when membership changes.
	public delegate void MembershipChanged(List<SECTR_Sector> left, List<SECTR_Sector> joined);

	/// Event handler for membership changed callbacks.
	public event MembershipChanged Changed;

#if UNITY_EDITOR
	// Debug Rendering stuff
	public static Color MemberColor = Color.white;
#endif
	#endregion

	#region Unity Interface
	void Start()
	{
		started = true;
		ForceUpdate(children.Count == 0);
	}

	protected virtual void OnEnable()
	{
		allMembers.Add(this);
		allMemberTable.Add(transform, this);
		if(bakedOnlyLights != null)
		{
			int numBakedOnlyLights = bakedOnlyLights.Count;
			bakedOnlyTable = new Dictionary<Light, Light>(numBakedOnlyLights);
			for(int lightIndex = 0; lightIndex < numBakedOnlyLights; ++lightIndex)
			{
				Light bakedOnlyLight = bakedOnlyLights[lightIndex];
				if(bakedOnlyLight)
				{
					bakedOnlyTable[bakedOnlyLight] = bakedOnlyLight;
				}
			}
		}
		if(started)
		{
			ForceUpdate(true);
		}
	}
	
	protected virtual void OnDisable()
	{
		if(Changed != null && sectors.Count > 0)
		{
			Changed(sectors, null);
		}

		if(!isSector && !neverJoin)
		{
			int numSectors = sectors.Count;
			for(int sectorIndex = 0; sectorIndex < numSectors; ++sectorIndex)
			{
				SECTR_Sector sector = sectors[sectorIndex];
				if(sector)
				{
					sector.Deregister(this);
				}
			}
			sectors.Clear();
		}

		int numChildren = children.Count;
		for(int childIndex = 0; childIndex < numChildren; ++childIndex)
		{
			Child child = children[childIndex];
			child.processed = false;
			childPool.Push(child);
		}
		children.Clear();
		childTable.Clear();
		renderers.Clear();
		lights.Clear();
		terrains.Clear();
		if(SECTR_Modules.VIS)
		{
			shadowLights.Clear();
			shadowCasters.Clear();
		}
		
		bakedOnlyTable = null;
		allMembers.Remove(this);
		allMemberTable.Remove(transform);
	}

	#if UNITY_EDITOR
	protected virtual void OnDrawGizmosSelected() 
	{
		// Render a cube of the aggregate bounds.
		if(enabled && !isSector)
		{
			// Draw the bounds
			Gizmos.color = MemberColor;
			Gizmos.DrawWireCube( totalBounds.center, totalBounds.size ); 

			// Render links to neighbor Sectors.
			Gizmos.color = SECTR_Sector.SectorColor;
			int numSectors = sectors.Count;
			for(int sectorIndex = 0; sectorIndex < numSectors; ++sectorIndex)
			{
				SECTR_Sector sector = sectors[sectorIndex];
				Gizmos.DrawLine(totalBounds.center, sector.TotalBounds.center);
			}
		}
	}
	#endif
	
	void LateUpdate()
	{
#if UNITY_EDITOR
		if(gameObject.isStatic && Application.isEditor && !Application.isPlaying)
		{
			BoundsUpdateMode = BoundsUpdateModes.Static;
		}
#endif

		// In the Editor, we're always dynamic, all the time, because
		// users modify static objects constantly.
		if(
#if UNITY_4_0
		BoundsUpdateMode != BoundsUpdateModes.Static
#else
		BoundsUpdateMode != BoundsUpdateModes.Static && (BoundsUpdateMode == BoundsUpdateModes.Always || transform.hasChanged)
#endif
#if UNITY_EDITOR
		|| (Application.isEditor && !Application.isPlaying)
#endif
		)
		{
			_UpdateChildren();
			if(!isSector && !neverJoin)
			{			
				_UpdateSectorMembership();
			}
			lastPosition = transform.position;
#if !UNITY_4
			transform.hasChanged = false;
#endif
		}
	}
	#endregion
	
	#region Private Utility Methods
	// Recomputes all Child related data.
	private void _UpdateChildren()
	{
		if(frozen || childProxy)
		{
			return;
		}

		bool dirShadowCaster = SECTR_Modules.VIS && DirShadowCaster && DirShadowCaster.type == LightType.Directional && DirShadowCaster.shadows != LightShadows.None;
		Vector3 shadowVec = dirShadowCaster ? DirShadowCaster.transform.forward * DirShadowDistance : Vector3.zero;
		int numChildren = children.Count;
		int childIndex = 0;

		hasLightBounds = false;
		hasRenderBounds = false;
		shadowCaster = false;
		shadowLight = false;
		// Intentionally not clearing Children
		renderers.Clear();
		lights.Clear();
		terrains.Clear();
		if(SECTR_Modules.VIS)
		{
			shadowCasters.Clear();
			shadowLights.Clear();
		}
			
		// Start mode is a fast path that assumes children do not change, so we don't need to find them
		// every frame. Instead, use the children we have to update the bounds accordingly.
		// SelfOnly is similar, but we never add children other than our main game object.
		if((BoundsUpdateMode == BoundsUpdateModes.Start || BoundsUpdateMode == BoundsUpdateModes.SelfOnly) && numChildren > 0
#if UNITY_EDITOR
		   && (!Application.isEditor || Application.isPlaying)
#endif
		) 
		{
			while(childIndex < numChildren)
			{
				Child child = children[childIndex];
				if(!child.gameObject)
				{
					children.RemoveAt(childIndex);
					--numChildren;
					continue;
				}

				child.Init(child.gameObject, child.renderer, child.light, child.terrain, child.member, dirShadowCaster, shadowVec);
				if(child.renderer)
				{
					if(!hasRenderBounds)
					{
						renderBounds = child.rendererBounds;
						hasRenderBounds = true;
					}
					else
					{
						renderBounds.Encapsulate(child.rendererBounds);
					}
					renderers.Add(child);
				}

				if(child.terrain)
				{
					if(!hasRenderBounds)
					{
						renderBounds = child.terrainBounds;
						hasRenderBounds = true;
					}
					else
					{
						renderBounds.Encapsulate(child.terrainBounds);
					}
					terrains.Add(child);
				}

				if(child.light)
				{
					if(SECTR_Modules.VIS && child.shadowLight)
					{
						shadowLights.Add(child);
						shadowLight = true;
					}

					if(!hasLightBounds)
					{
						lightBounds = child.lightBounds;
						hasLightBounds = true;
					}
					else
					{
						lightBounds.Encapsulate(child.lightBounds);
					}
					lights.Add(child);
				}

				if(SECTR_Modules.VIS && (child.terrainCastsShadows || child.rendererCastsShadows))
				{
					shadowCasters.Add(child);
					shadowCaster = true;
				}
				++childIndex;
			}
		}
		else
		{
#if UNITY_EDITOR
			if(bakedOnlyLights != null)
			{
				bakedOnlyLights.Clear();
			}
#endif
			for(childIndex = 0; childIndex < numChildren; ++childIndex)
			{
				children[childIndex].processed = false;
			}

			_AddChildren(transform, dirShadowCaster, shadowVec);

			childIndex = 0;
			numChildren = children.Count;
			while(childIndex < numChildren)
			{
				Child child = children[childIndex];
				if(!child.processed)
				{
					childPool.Push(child);
					children.RemoveAt(childIndex);
					--numChildren;
				}
				else
				{
					++childIndex;
				}
			}
		}

		Bounds zeroBounds = new Bounds(transform.position, Vector3.zero);
		if(hasRenderBounds && (isSector || neverJoin))
		{
			totalBounds = renderBounds;
		}
		else if(hasRenderBounds && hasLightBounds)
		{
			totalBounds = renderBounds;
			totalBounds.Encapsulate(lightBounds);
		}
		else if(hasRenderBounds)
		{
			totalBounds = renderBounds;
			lightBounds = zeroBounds;
		}
		else if(hasLightBounds)
		{
			totalBounds = lightBounds;
			renderBounds = zeroBounds;
		}
		else
		{
			totalBounds = zeroBounds;
			lightBounds = zeroBounds;
			renderBounds = zeroBounds;
		}
		totalBounds.Expand(ExtraBounds);

		// Bounds override only affects the total member bounds.
		// We'll keep light and render bounds honest (for now).
		if(OverrideBounds)
		{
			totalBounds = BoundsOverride;
		}
#if UNITY_EDITOR
		else
		{
			BoundsOverride = totalBounds;
		}
#endif
	}

	// Recursive utility function to walk the tree of objects.
	private void _AddChildren(Transform childTransform, bool dirShadowCaster, Vector3 shadowVec)
	{
		// We always stop the descent when we encounter another SectorMember
		if(childTransform.gameObject.activeSelf && 
		   (childTransform == transform || !allMemberTable.ContainsKey(childTransform)))
		{
			Child cachedChild = null;
			childTable.TryGetValue(childTransform, out cachedChild);
#if UNITY_4
			Light childLight = cachedChild != null ? cachedChild.light : childTransform.light;
			Renderer childRenderer = cachedChild != null ? cachedChild.renderer : childTransform.renderer;
#else
			Light childLight = cachedChild != null ? cachedChild.light : childTransform.GetComponent<Light>();
			Renderer childRenderer = cachedChild != null ? cachedChild.renderer : childTransform.GetComponent<Renderer>();
#endif
			Terrain childTerrain = null;

			// Perform some more expensive Sector specific logic.
			if(isSector || neverJoin)
			{
				// Avoid an expensive GetComponent check as it's very unlikely to have Terrain's outside of Sectors.
				childTerrain = cachedChild != null ? cachedChild.terrain : childTransform.GetComponent<Terrain>();
			}

			// Baked only lights are a waste of CPU time for visibility culling.
			// So we want to ignore them as far as Member's are concerned, but
			// doing so requires a bit of a hack.
			// TODO: See if there is such a thing as baked only lights in 5, non-legacy mode.
			if(bakedOnlyLights != null && 
				childLight && 
#if UNITY_5_4_OR_NEWER
				childLight.isBaked &&
#elif !UNITY_4_0 && !UNITY_4_1

				childLight.alreadyLightmapped && 
#endif
				LightmapSettings.lightmaps.Length > 0)
			{
				// Unfortunately, the only way to find out if a light is baked-only is in the Editor,
				// so we create and store a list of all baked lights.
#if UNITY_EDITOR
				SerializedObject serialObj = new SerializedObject(childLight);
				SerializedProperty lightmapProp = serialObj.FindProperty("m_Lightmapping");
				if(lightmapProp != null && 
#if UNITY_4
				   lightmapProp.enumValueIndex >= 0 && lightmapProp.enumValueIndex < lightmapProp.enumNames.Length &&
				   lightmapProp.enumNames[lightmapProp.enumValueIndex] == "BakedOnly"
#else
				   lightmapProp.intValue == 2 // 2 is Baked
#endif
				   )
				{
					bakedOnlyLights.Add(childLight);
					if(bakedOnlyTable == null)
					{
						bakedOnlyTable = new Dictionary<Light, Light>();
					}
					bakedOnlyTable[childLight] = childLight;
				}
				else if(bakedOnlyTable != null)
				{
					bakedOnlyTable.Remove(childLight);
				}
#endif

				// In editor and in game, we skip over any lights contained in the list
				// of baked only lights. We use a dictionary to compute things extra quickly.
				if(bakedOnlyTable != null && bakedOnlyTable.ContainsKey(childLight))
				{
					childLight = null;
				}
			}

			Child newChild = cachedChild;
			if(newChild == null)
			{
				if(childPool.Count > 0)
				{
					newChild = childPool.Pop();
				}
				else
				{
					newChild = new Child();
				}
				childTable[childTransform] = newChild;
				children.Add(newChild);
			}
			newChild.Init(childTransform.gameObject, childRenderer, childLight, childTerrain, this, dirShadowCaster, shadowVec);
			
			if(newChild.renderer)
			{	
				bool includeBounds = true;
				if(isSector)
				{
					// Filter particle systems out of Sectors as their bounds are simply too unreliable.
					System.Type renderType = childRenderer.GetType();
					if(renderType == typeof(ParticleSystemRenderer) 
						#if UNITY_OLD_PARTICLES
						|| renderType == typeof(ParticleRenderer)
						#endif
					)
					{
						includeBounds = false;
					}
				}

				if(includeBounds)
				{
					if(!hasRenderBounds)
					{
						renderBounds = newChild.rendererBounds;
						hasRenderBounds = true;
					}
					else
					{
						renderBounds.Encapsulate(newChild.rendererBounds);
					}
				}
				renderers.Add(newChild);
			}
			if(newChild.light)
			{
				if(SECTR_Modules.VIS && newChild.shadowLight)
				{
					shadowLights.Add(newChild);
					shadowLight = true;
				}

				if(!hasLightBounds)
				{
					lightBounds = newChild.lightBounds;
					hasLightBounds = true;
				}
				else
				{
					lightBounds.Encapsulate(newChild.lightBounds);
				}
				lights.Add(newChild);
			}
			if(newChild.terrain)
			{					
				if(!hasRenderBounds)
				{
					renderBounds = newChild.terrainBounds;
					hasRenderBounds = true;
				}
				else
				{
					renderBounds.Encapsulate(newChild.terrainBounds);
				}
				terrains.Add(newChild);
			}

			if(SECTR_Modules.VIS && (newChild.terrainCastsShadows || newChild.rendererCastsShadows))
			{
				shadowCasters.Add(newChild);
				shadowCaster = true;
			}

			if(BoundsUpdateMode != BoundsUpdateModes.SelfOnly)
			{
				int numChildren = childTransform.transform.childCount;
				for(int childIndex = 0; childIndex < numChildren; ++childIndex)
				{
					_AddChildren(childTransform.GetChild(childIndex), dirShadowCaster, shadowVec);
				}
			}
		}
	}

	// Recomputes the list of Sectors that this Member is a part of. Also
	// ensures that Sectors are appropriately notified of the changes.
	private void _UpdateSectorMembership()
	{
		if(frozen || isSector || neverJoin)
		{
			return;
		}

		newSectors.Clear();
		leftSectors.Clear();

		if(PortalDetermined && sectors.Count > 0)
		{
			int numSectors = sectors.Count;
			for(int sectorIndex = 0; sectorIndex < numSectors; ++sectorIndex)
			{
				SECTR_Sector sector = sectors[sectorIndex];
				SECTR_Portal crossedPortal = _CrossedPortal(sector);
				if(crossedPortal)
				{
					SECTR_Sector newSector = crossedPortal.FrontSector == sector ? crossedPortal.BackSector : crossedPortal.FrontSector;
					if(!newSectors.Contains(newSector))
					{
						newSectors.Add(newSector);
					}
					leftSectors.Add(sector);
				}
			}

			numSectors = newSectors.Count;
			for(int sectorIndex = 0; sectorIndex < numSectors; ++sectorIndex)
			{
				SECTR_Sector newSector = newSectors[sectorIndex];
				newSector.Register(this);
				sectors.Add(newSector);
			}

			numSectors = leftSectors.Count;
			for(int sectorIndex = 0; sectorIndex < numSectors; ++sectorIndex)
			{
				SECTR_Sector leftSector = leftSectors[sectorIndex];
				leftSector.Deregister(this);
				sectors.Remove(leftSector);
			}
		}
		else if(PortalDetermined && ForceStartSector && !usedStartSector)
		{
			ForceStartSector.Register(this);
			sectors.Add(ForceStartSector);
			newSectors.Add(ForceStartSector);
			usedStartSector = true;
		}
		else
		{
			SECTR_Sector.GetContaining(ref newSectors, TotalBounds);

			int sectorIndex = 0;
			int numSectors = sectors.Count;
			while(sectorIndex < numSectors)
			{
				SECTR_Sector oldSector = sectors[sectorIndex];
				if(newSectors.Contains(oldSector))
				{
					newSectors.Remove(oldSector);
					++sectorIndex;
				}
				else
				{
					oldSector.Deregister(this);
					leftSectors.Add(oldSector);
					sectors.RemoveAt(sectorIndex);
					--numSectors;
				}
			}
			
			numSectors = newSectors.Count;
			if(numSectors > 0)
			{
				for(sectorIndex = 0; sectorIndex < numSectors; ++sectorIndex)
				{
					SECTR_Sector newSector = newSectors[sectorIndex];
					newSector.Register(this);
					sectors.Add(newSector);
				}
			}
		}

		if(Changed != null && (leftSectors.Count > 0 || newSectors.Count > 0))
		{
			Changed(leftSectors, newSectors);
		}
	}

	private SECTR_Portal _CrossedPortal(SECTR_Sector sector)
	{
		if(sector)
		{
			Vector3 rayDirection = transform.position - lastPosition;
			int numPortals = sector.Portals.Count;
			for(int portalIndex = 0; portalIndex < numPortals; ++portalIndex)
			{
				SECTR_Portal portal = sector.Portals[portalIndex];
				if(portal)
				{
					bool forwardTraversal = (portal.FrontSector == sector);
					Plane portalPlane = forwardTraversal ? portal.HullPlane : portal.ReverseHullPlane;
					SECTR_Sector oppositeSector = forwardTraversal ? portal.BackSector : portal.FrontSector;
					if(oppositeSector &&
					   Vector3.Dot(rayDirection, portalPlane.normal) < 0f && 
					   portalPlane.GetSide(transform.position) != portalPlane.GetSide(lastPosition) &&
					   portal.IsPointInHull(transform.position, rayDirection.magnitude))
					{
						return portal;
					}
				}
			}
		}
		return null;
	}
	#endregion
}
