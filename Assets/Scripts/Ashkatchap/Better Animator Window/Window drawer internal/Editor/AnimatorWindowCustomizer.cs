using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;

public class AnimatorWindowCustomizer : EditorWindow {
	static AnimatorControllerTool animatorControllerWindow {
		get {
			var array = Resources.FindObjectsOfTypeAll(typeof(AnimatorControllerTool));
			EditorWindow editorWindow = (array.Length <= 0) ? null : ((EditorWindow) array[0]);
			return editorWindow != null ? editorWindow as AnimatorControllerTool : null;
		}
	}

	[InitializeOnLoadMethod]
	static void InitOnLoad() {
		// Execute after Unity's windows have loaded to get any existing Animator Window instead of creating one and breaking Unity
		EditorApplication.delayCall += InitOnLoadDelayed;
	}

	static void InitOnLoadDelayed() {
		if (EditorApplication.isPlayingOrWillChangePlaymode) return;
		// get current focused window to regain focus to it later.
		//var prevWindow = focusedWindow;
		//Debug.Log(prevWindow);
		SetGraphGUI(CreateInstance<CustomGraphGUI>());
		//Debug.Log(focusedWindow);
		//prevWindow.Show();
	}

	[MenuItem("Window/Animation/Animator Customizer")]
	static void Init() {
		var window = GetWindow<AnimatorWindowCustomizer>();
		window.Show();
		animatorControllerWindow?.Show();
	}

	private void OnEnable() {
		titleContent = new GUIContent("Animator Customizer");
		minSize = new Vector2(100, 20);
	}

	void OnGUI() {
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("On")) {
			SetGraphGUI(CreateInstance<CustomGraphGUI>());
		}
		if (GUILayout.Button("Off")) {
			SetGraphGUI(CreateInstance<UnityEditor.Graphs.AnimationStateMachine.GraphGUI>());
		}
		if (GUILayout.Button("Test G W")) {
			GetWindow<AnimatorControllerTool>(); // GetWindow also shows the window and focus it
		}
		GUILayout.EndHorizontal();
	}

	private static void SetGraphGUI(UnityEditor.Graphs.AnimationStateMachine.GraphGUI newGraphGUI) {
		if (animatorControllerWindow != null) {
			var currentGraphGUI = animatorControllerWindow.stateMachineGraphGUI;
			newGraphGUI.graph = currentGraphGUI.graph;
			newGraphGUI.hideFlags = HideFlags.HideAndDontSave;

			animatorControllerWindow.stateMachineGraphGUI = newGraphGUI;
		}
	}
}
