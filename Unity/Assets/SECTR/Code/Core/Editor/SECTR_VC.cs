// Copyright (c) 2014 Make Code Now! LLC
#if !(UNITY_4_0 || UNITY_4_1)
#define UNITY_VC
#endif

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public static class SECTR_VC
{
	public static bool HasVC()
	{
#if UNITY_VC
		return UnityEditor.VersionControl.Provider.enabled && UnityEditor.VersionControl.Provider.isActive;
#else
		return false;
#endif
	}
	
	public static void WaitForVC()
	{
#if UNITY_VC
		if(HasVC())
		{
			while(UnityEditor.VersionControl.Provider.activeTask != null)
			{
				UnityEditor.VersionControl.Provider.activeTask.Wait();
			}
		}
#endif
		AssetDatabase.Refresh();
		AssetDatabase.SaveAssets();

	}
	
	public static bool CheckOut(string path)
	{
#if UNITY_VC
		if(HasVC())
		{
			UnityEditor.VersionControl.Asset vcAsset = UnityEditor.VersionControl.Provider.GetAssetByPath(path);
			if(vcAsset != null)
			{
				UnityEditor.VersionControl.Task task = UnityEditor.VersionControl.Provider.Checkout(vcAsset, UnityEditor.VersionControl.CheckoutMode.Both);
				task.Wait();
			}
		}
		return IsEditable(path);
#else
		return true;
#endif
	}
	
	public static void Revert(string path)
	{
#if UNITY_VC
		if(HasVC())
		{
			UnityEditor.VersionControl.Asset vcAsset = UnityEditor.VersionControl.Provider.GetAssetByPath(path);
			if(vcAsset != null)
			{
				UnityEditor.VersionControl.Task task = UnityEditor.VersionControl.Provider.Revert(vcAsset, UnityEditor.VersionControl.RevertMode.Normal);
				task.Wait();
				AssetDatabase.Refresh();
			}
		}
#endif
	}
	
	public static bool IsEditable(string path)
	{
#if UNITY_VC
		if(HasVC())
		{
			UnityEditor.VersionControl.Asset vcAsset = UnityEditor.VersionControl.Provider.GetAssetByPath(path);
			return vcAsset != null ? UnityEditor.VersionControl.Provider.IsOpenForEdit(vcAsset) : true;
		}
		else
		{
			return true;
		}
#else
		return true;

#endif
	}
}
