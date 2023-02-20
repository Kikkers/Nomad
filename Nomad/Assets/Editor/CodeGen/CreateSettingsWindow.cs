using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public class CreateSettingsInstanceWindow : EditorWindow
{
	private const string scriptableObjectFileContents = "" +
		"using UnityEngine;\r\n" +
		"\r\n" +
		"[CreateAssetMenu(fileName = \"{0}Settings\", menuName = \"Settings/{0}\")]\r\n" +
		"public class {0}Settings : ScriptableObject\r\n" +
		"{{\r\n" +
		"\t[field: SerializeField] public bool RenameMe {{ get; private set; }}\r\n" +
		"}}\r\n";

	private const string instanceFileContents = "" +
		"%YAML 1.1\r\n" +
		"%TAG !u! tag:unity3d.com,2011:\r\n" +
		"--- !u!114 &11400000\r\n" +
		"MonoBehaviour:\r\n" +
		"  m_ObjectHideFlags: 0\r\n" +
		"  m_CorrespondingSourceObject: {{fileID: 0}}\r\n" +
		"  m_PrefabInstance: {{fileID: 0}}\r\n" +
		"  m_PrefabAsset: {{fileID: 0}}\r\n" +
		"  m_GameObject: {{fileID: 0}}\r\n" +
		"  m_Enabled: 1\r\n" +
		"  m_EditorHideFlags: 0\r\n" +
		"  m_Script: {{fileID: 11500000, guid: {1}, type: 3}}\r\n" +
		"  m_Name: {0}\r\n" +
		"  m_EditorClassIdentifier: \r\n" +
		"  <RenameMe>k__BackingField: 0\r\n";


	private string filename = string.Empty;
	private bool firstFocus = false;
	private string assetPath;

	[MenuItem("Assets/Custom Create/Settings ScriptableObject and single instance")]
	private static void CreateWindow()
	{
		CreateSettingsInstanceWindow instance = CreateInstance<CreateSettingsInstanceWindow>();
		instance.name = "Create Settings and Asset Instance Window";
		instance.ShowModal();
	}

	private void OnEnable()
	{
		assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
		if (File.Exists(assetPath))
		{
			// should not be a file, but assume the name is intended
			FileInfo file = new(assetPath);
			filename = file.Name.Split('.')[0];

			int splitIndex = assetPath.LastIndexOf('/');
			assetPath = assetPath.Substring(0, splitIndex);
		}
	}

	private void OnGUI()
	{
		const string inputControlName = "InputControlName";

		if (!firstFocus)
		{
			GUI.SetNextControlName(inputControlName);
		}
		filename = EditorGUILayout.TextField("ScriptableObject Name", filename);
		if (!firstFocus)
		{
			EditorGUI.FocusTextInControl(inputControlName);
			firstFocus = true;
		}

		if (IsValidFileName)
		{
			if (GUILayout.Button(string.Format("Create {0}Settings.cs\nCreate {0}Settings.asset", CapitalizedFilename)))
			{
				TryCreateProcess();
			}
		}
		else
		{
			GUILayout.Button("Please enter a valid name");
		}
	}

	private bool IsValidFileName => filename.Length > 1 && filename.IndexOfAny(Path.GetInvalidPathChars()) < 0;

	private string CapitalizedFilename => char.ToUpper(filename[0]) + filename.Substring(1);

	private void TryCreateProcess()
	{
		try
		{
			CreateProcess();
		}
		catch (Exception e)
		{
			Debug.LogException(e);
		}
		Close();
	}

	private void CreateProcess()
	{
		if (!IsValidFileName)
		{
			throw new Exception("Invalid: bad file name");
		}

		// auto capitalize
		filename = CapitalizedFilename;
		filename = filename.Split('.')[0];

		string formattedScriptableObjectFileContents = string.Format(scriptableObjectFileContents, filename);
		string newScriptableObjectFilePath = assetPath + "/" + filename + "Settings.cs";
		File.WriteAllText(newScriptableObjectFilePath, formattedScriptableObjectFileContents);

		AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
		string guid = AssetDatabase.GUIDFromAssetPath(newScriptableObjectFilePath).ToString();

		string formattedInstanceFileContents = string.Format(instanceFileContents, filename + "Settings", guid);
		string newInstanceFilePath = assetPath + "/" + filename + "Settings.asset";
		File.WriteAllText(newInstanceFilePath, formattedInstanceFileContents);

		AssetDatabase.Refresh();
	}
}
