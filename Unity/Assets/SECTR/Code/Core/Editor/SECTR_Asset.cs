#if !(UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2)
#define UNITY_MSE
#endif

using UnityEngine;
using UnityEditor;
#if UNITY_MSE
using UnityEditor.SceneManagement;
#endif
using System.IO;
using System.Collections.Generic;

public static class SECTR_Asset
{
	#region Public Interface
	public static string GetProjectName()
	{
		string[] dataPathParts = Application.dataPath.Split('/');
		string projectName = dataPathParts[dataPathParts.Length - 2];
		return projectName;
	}

	public static void SplitPath(string path, out string dirPath, out string fileName)
	{
		string[] pathParts = path.Split('/');
		if(pathParts.Length > 0)
		{
			fileName = pathParts[pathParts.Length - 1];
			dirPath = path.Replace(fileName, "");
		}
		else
		{
			dirPath = null;
			fileName = null;
		}
	}
	
	public static string UnityToOSPath(string path)
	{
		return string.IsNullOrEmpty(path) ? null : Application.dataPath + path.Replace("Assets", "");
	}
	
	public static string OSToUnityPath(string path)
	{
		return string.IsNullOrEmpty(path) ? null : path.Replace(Application.dataPath, "Assets");
	}

	public static string CurrentScene()
	{
		#if UNITY_MSE
		return EditorSceneManager.GetActiveScene().path;
		#else
		return EditorApplication.currentScene;
		#endif
	}

	public static bool GetCurrentSceneParts(out string sceneDir, out string sceneName)
	{
		string currentScene = CurrentScene();
		if(!string.IsNullOrEmpty(currentScene))
		{
			SplitPath(currentScene, out sceneDir, out sceneName);
			sceneName = sceneName.Replace(".unity", "");
			return true;
		}
		else
		{
			sceneDir = null;
			sceneName = null;
			return false;
		}
	}

	public static string MakeExportFolder(string subFolder, bool clearSubfolder, out string sceneDir, out string sceneName)
	{
		if(GetCurrentSceneParts(out sceneDir, out sceneName))
		{
			string folderPath = sceneDir;
			string baseFolder = UnityToOSPath(folderPath) + sceneName;
			if(!System.IO.Directory.Exists(baseFolder))
			{
				// Create new folder. Use substring b/c Unity dislikes the trailing /
				AssetDatabase.CreateFolder(folderPath.Substring(0, folderPath.Length - 1), sceneName);
				SECTR_VC.WaitForVC();
			}
			folderPath += sceneName + "/";
			
			// Inside that "standard" folder, make a Chunks subfolder, so that we can manage the contents,
			// but first, remove previous export, if there was one.
			string osPath = UnityToOSPath(folderPath) + subFolder;
			bool createSubFolder = !string.IsNullOrEmpty(subFolder) && !System.IO.Directory.Exists(osPath);
			if(clearSubfolder && !createSubFolder)
			{
				AssetDatabase.DeleteAsset(folderPath + subFolder);
				createSubFolder = true;
			}
			if(createSubFolder)
			{
				// Create new folder. Use substring b/c Unity dislikes the trailing /
				AssetDatabase.CreateFolder(folderPath.Substring(0, folderPath.Length - 1), subFolder);
				SECTR_VC.WaitForVC();
			}
			if(!string.IsNullOrEmpty(subFolder))
			{
				folderPath += subFolder + "/";
			}
			
			return folderPath;
		}
		else
		{
			sceneDir = null;
			sceneName = null;
			return null;
		}
	}

	public static T Create<T>(string folder, string name, ref string assetPath) where T : ScriptableObject
	{
		T asset = ScriptableObject.CreateInstance<T>();
		
		string path = folder;
		
		if(string.IsNullOrEmpty(path))
		{
			path = AssetDatabase.GetAssetPath(Selection.activeObject);
			if(string.IsNullOrEmpty(path)) 
			{
				path = "Assets";
			}
			else if(Path.GetExtension(path) != "") 
			{
				path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
			}
		}
		
		string fileName = name;
		if(string.IsNullOrEmpty(fileName))
		{
			fileName = "New" + typeof(T).ToString();
		}
		
		assetPath = AssetDatabase.GenerateUniqueAssetPath(path + "/" + fileName + ".asset");
		AssetDatabase.CreateAsset(asset, assetPath);
		AssetDatabase.SaveAssets();
		return asset;
	}

	public static T Create<T>(string folder, string name, T asset) where T : UnityEngine.Object
	{
		string path = folder;
		
		if(string.IsNullOrEmpty(path))
		{
			path = AssetDatabase.GetAssetPath(Selection.activeObject);
			if(string.IsNullOrEmpty(path)) 
			{
				path = "Assets";
			}
			else if(Path.GetExtension(path) != "") 
			{
				path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
			}
		}
		
		string fileName = name;
		if(string.IsNullOrEmpty(fileName))
		{
			fileName = "New" + typeof(T).ToString();
		}
		
		string assetPath = AssetDatabase.GenerateUniqueAssetPath(path + "/" + fileName + ".asset");
		AssetDatabase.CreateAsset(asset, assetPath);
		AssetDatabase.SaveAssets();
		return asset;
	}
	
	public static List<T> GetAll<T>(string rootPath, List<string> extensions, ref List<string> paths, bool pathsOnly) where T : UnityEngine.Object
	{	
		string progressTitle = "Finding " + typeof(T).Name;
		EditorUtility.DisplayProgressBar(progressTitle, "Starting Search", 0f);

		List<T> assetRefs = new List<T>(128);
		paths.Clear();

#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4
		DirectoryInfo dirInfo = new DirectoryInfo(string.IsNullOrEmpty(rootPath) ? Application.dataPath : rootPath);
		int numExtensions = extensions.Count;
		for(int extensionIndex = 0; extensionIndex < numExtensions; ++extensionIndex)
		{
			string extension = extensions[extensionIndex];
			FileInfo[] files = dirInfo.GetFiles("*" + extension, SearchOption.AllDirectories);
			int numFiles = files.Length;
			for(int fileIndex = 0; fileIndex < numFiles; ++fileIndex)
			{
				FileInfo fi = files[fileIndex];
				if(fi.Name.Length > 0 && fi.Name[0] != '.')
				{
					string relativePath = fi.FullName.Replace('\\', '/');
					relativePath = relativePath.Replace(Application.dataPath, "Assets");
					EditorUtility.DisplayProgressBar(progressTitle, "Searching " + relativePath, fileIndex / (float)numFiles);
					if(pathsOnly)
					{
						paths.Add(relativePath);
					}
					else
					{
						T asset = Load<T>(relativePath);
						if(asset)
						{
							assetRefs.Add(asset);
							paths.Add(relativePath);
						}
					}
				}
			}
		}
#else
		string[] searchPaths = null;
		if(!string.IsNullOrEmpty(rootPath))
		{
			searchPaths = new string[1];
			searchPaths[0] = OSToUnityPath(rootPath);
		}
		string typeString = "t:";
		// Hack to match internal Unity class name with C# class name.
		if(typeof(T) == typeof(AudioClip))
		{
			typeString += "audioClip";
		}
		else
		{
			typeString += typeof(T);
		}
		paths.AddRange(AssetDatabase.FindAssets(typeString, searchPaths));
		int numPaths = paths.Count;
		for(int pathIndex = 0; pathIndex < numPaths; ++pathIndex)
		{
			paths[pathIndex] = AssetDatabase.GUIDToAssetPath(paths[pathIndex]);
			if(!pathsOnly)
			{
				assetRefs.Add(Load<T>(paths[pathIndex]));
			}
		}
#endif
		Resources.UnloadUnusedAssets();
		EditorUtility.ClearProgressBar();
		return assetRefs;
	}

	public static T Find<T>(string assetName) where T : UnityEngine.Object
	{
		// get every single one of the files in the Assets folder.
		DirectoryInfo dirInfo = new DirectoryInfo(Application.dataPath);
		FileInfo[] files = dirInfo.GetFiles(assetName, SearchOption.AllDirectories);
		int numFiles = files.Length;
		for(int fileIndex = 0; fileIndex < numFiles; ++fileIndex)
		{
			FileInfo fi = files[fileIndex];
			if(fi.Name == assetName) // Unity ignores dotfiles.
			{
				string relativePath = fi.FullName.Replace('\\', '/');
				relativePath = relativePath.Replace(Application.dataPath, "Assets");
				return Load<T>(relativePath);
			}
		}
		return null;
	}
	
	public static T Load<T>(string path) where T : UnityEngine.Object
	{
		if(!string.IsNullOrEmpty(path))
		{
			T asset = AssetDatabase.LoadAssetAtPath(path, typeof(T)) as T;
			if(asset)
			{
				return asset;
			}
		}
		return null;
	}
	#endregion
}
