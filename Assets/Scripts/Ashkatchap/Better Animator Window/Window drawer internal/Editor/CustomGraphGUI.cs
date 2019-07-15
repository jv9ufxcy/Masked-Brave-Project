// UnityEditor.Graphs.AnimationStateMachine.GraphGUI
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Graphs;
using UnityEditor.Graphs.AnimationStateMachine;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

internal class CustomGraphGUI : UnityEditor.Graphs.AnimationStateMachine.GraphGUI {
	private static readonly FieldInfo cachedFieldm_HoveredStateMachineNode = typeof(UnityEditor.Graphs.AnimationStateMachine.GraphGUI).GetField("m_HoveredStateMachineNode", BindingFlags.Instance | BindingFlags.NonPublic);
	private StateMachineNode m_HoveredStateMachineNode {
		get {
			return (StateMachineNode) cachedFieldm_HoveredStateMachineNode.GetValue(this);
		}
		set {
			cachedFieldm_HoveredStateMachineNode.SetValue(this, value);
		}
	}

	private bool isSelectionMoving => selection.Count > 0 && selection[0].isDragging;

	private static readonly FieldInfo cachedFieldLiveLinkInfo = typeof(UnityEditor.Graphs.AnimationStateMachine.GraphGUI).GetField("m_LiveLinkInfo", BindingFlags.Instance | BindingFlags.NonPublic);
	public void SetLiveLinkInfo(in LiveLinkInfo v) {
		cachedFieldLiveLinkInfo.SetValue(this, v);
	}

	public override IEdgeGUI edgeGUI {
		get {
			if (m_EdgeGUI == null) {
				m_EdgeGUI = new CustomEdgeGUI { host = this };
			}
			return m_EdgeGUI;
		}
	}

	public override void NodeGUI(UnityEditor.Graphs.Node n) {
		GUILayoutUtility.GetRect(150f, 0f);
		SelectNode(n);
		n.NodeUI(this);
		DragNodes();
	}

	private void SetHoveredStateMachine() {
		Vector2 mousePosition = Event.current.mousePosition;
		m_HoveredStateMachineNode = null;
		foreach (UnityEditor.Graphs.AnimationStateMachine.Node node in m_Graph.nodes) {
			StateMachineNode stateMachineNode = node as StateMachineNode;
			if (stateMachineNode && stateMachineNode.position.Contains(mousePosition) && !selection.Contains(stateMachineNode)) {
				m_HoveredStateMachineNode = stateMachineNode;
				break;
			}
		}
	}

	protected override Vector2 GetCenterPosition() {
		return base.GetCenterPosition();
	}

	private bool IsCurrentStateMachineNodeLiveLinked(UnityEditor.Graphs.AnimationStateMachine.Node n) {
		StateMachineNode stateMachineNode = n as StateMachineNode;
		if (stateMachineNode != null) {
			AnimatorState currentState = liveLinkInfo.currentState;
			if (currentState == null) {
				return false;
			}
			bool flag = activeStateMachine.HasState(currentState, recursive: true);
			bool flag2 = stateMachineNode.stateMachine.HasState(currentState, recursive: true);
			bool flag3 = stateMachineNode.stateMachine.HasStateMachine(activeStateMachine, recursive: false);
			if ((flag3 && flag2 && !flag) || (!flag3 && flag2)) {
				return true;
			}
		}
		return false;
	}

	public override void OnGraphGUI() {
		if (stateMachineGraph.DisplayDirty()) {
			stateMachineGraph.RebuildGraph();
		}
		SyncGraphToUnitySelection();
		LiveLink();
		SetHoveredStateMachine();
		m_Host.BeginWindows();
		bool liveLink = tool.liveLink;
		foreach (UnityEditor.Graphs.AnimationStateMachine.Node node in m_Graph.nodes) {
			bool on = selection.Contains(node);
			GUIStyle style = Styles.GetNodeStyle(node.style, (liveLink && IsCurrentStateMachineNodeLiveLinked(node)) ? Styles.Color.Blue : node.color, on);

			StateMachineBehaviour[] behaviours =
				node is StateMachineNode ? ((StateMachineNode) node).stateMachine.behaviours :
				node is StateNode ? ((StateNode) node).state.behaviours :
				null;

			IColorSMB colorSMB = null;
			if (behaviours != null) {
				for (int i = 0; i < behaviours.Length; i++) {
					colorSMB = behaviours[i] as IColorSMB;
					if (colorSMB != null)
						break;
				}
			}

			DrawNode(node, style, colorSMB);

			if (Event.current.type == EventType.MouseMove && node.position.Contains(Event.current.mousePosition)) {
				edgeGUI.SlotDragging(node.inputSlots.First(), true, true);
			}
		}
		edgeGUI.DoEdges();
		m_Host.EndWindows();
		if (Event.current.type == EventType.MouseDown && Event.current.button != 2) {
			edgeGUI.EndDragging();
		}
		HandleEvents();
		HandleContextMenu();
		HandleObjectDragging();
		DragSelection();
	}

	// Colorize nodes using IColorSMB
	private void DrawNode(UnityEditor.Graphs.AnimationStateMachine.Node node, GUIStyle style, IColorSMB colorSMB) {
		var originalBackgroundColor = GUI.backgroundColor;

		if (colorSMB != null) {
			style = colorSMB.GetCachedModifiedStyle(style);
			style.normal.textColor = colorSMB.GetContentColor();
			GUI.backgroundColor = colorSMB.GetBgColor();
		}

		node.position = GUILayout.Window(node.GetInstanceID(), node.position, delegate {
			NodeGUI(node);
		}, node.title, style, GUILayout.Width(0f), GUILayout.Height(0f));

		if (colorSMB != null) {
			GUI.backgroundColor = originalBackgroundColor;
		}
	}

	private List<AnimationClip> ComputeDraggedClipsFromModelImporter() {
		List<AnimationClip> list = new List<AnimationClip>();
		List<GameObject> list2 = DragAndDrop.objectReferences.OfType<GameObject>().ToList();
		for (int i = 0; i < list2.Count; i++) {
			ModelImporter modelImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(list2[i])) as ModelImporter;
			if (!(bool) modelImporter) {
				continue;
			}
			Object[] array = AssetDatabase.LoadAllAssetsAtPath(modelImporter.assetPath);
			Object[] array2 = array;
			foreach (Object @object in array2) {
				if ((@object.hideFlags & HideFlags.HideInHierarchy) == HideFlags.None) {
					AnimationClip animationClip = @object as AnimationClip;
					if ((bool) animationClip) {
						list.Add(animationClip);
					}
				}
			}
		}
		return list;
	}

	private void HandleObjectDragging() {
		Event current = Event.current;
		List<Motion> list = DragAndDrop.objectReferences.OfType<Motion>().ToList();
		List<AnimatorState> list2 = DragAndDrop.objectReferences.OfType<AnimatorState>().ToList();
		List<AnimatorStateMachine> list3 = DragAndDrop.objectReferences.OfType<AnimatorStateMachine>().ToList();
		List<AnimationClip> list4 = ComputeDraggedClipsFromModelImporter();
		switch (current.type) {
			case EventType.DragUpdated:
			case EventType.DragPerform:
				if (list.Count > 0 || list2.Count > 0 || list3.Count > 0 || list4.Count > 0) {
					DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
				}
				else {
					DragAndDrop.visualMode = DragAndDropVisualMode.None;
				}
				if (current.type == EventType.DragPerform && DragAndDrop.visualMode != 0) {
					DragAndDrop.AcceptDrag();
					Undo.RegisterCompleteObjectUndo(activeStateMachine, "Drag motion to state machine.");
					for (int i = 0; i < list.Count; i++) {
						AnimatorState state = activeStateMachine.AddState(list[i].name, current.mousePosition + new Vector2(12f, 12f) * i);
						tool.animatorController.SetStateEffectiveMotion(state, list[i], tool.selectedLayerIndex);
					}
					for (int i = 0; i < list2.Count; i++) {
						activeStateMachine.AddState(list2[i], current.mousePosition + new Vector2(12f, 12f) * i);
					}
					for (int i = 0; i < list3.Count; i++) {
						activeStateMachine.AddStateMachine(list3[i], current.mousePosition + new Vector2(12f, 12f) * i);
					}
					for (int i = 0; i < list4.Count; i++) {
						AnimatorState state2 = activeStateMachine.AddState(list4[i].name, current.mousePosition + new Vector2(12f, 12f) * i);
						tool.animatorController.SetStateEffectiveMotion(state2, list4[i], tool.selectedLayerIndex);
					}
					stateMachineGraph.RebuildGraph();
				}
				current.Use();
				break;
			case EventType.DragExited:
				current.Use();
				break;
			case EventType.Repaint:
				if (isSelectionMoving && m_HoveredStateMachineNode && !selection.Contains(m_HoveredStateMachineNode) && !SelectionOnlyUpNode()) {
					EditorGUIUtility.AddCursorRect(m_HoveredStateMachineNode.position, MouseCursor.ArrowPlus);
				}
				break;
		}
	}

	private bool SelectionOnlyUpNode() {
		return selection.Count == 1 && selection[0] is StateMachineNode &&
			(selection[0] as StateMachineNode).stateMachine == parentStateMachine;
	}

	private void LiveLink() {
		var localLiveLinkInfo = liveLinkInfo;

		localLiveLinkInfo.Clear();
		if (!tool.liveLink) {
			SetLiveLinkInfo(localLiveLinkInfo);
			return;
		}
		AnimatorControllerPlayable animatorControllerPlayable = AnimatorController.FindAnimatorControllerPlayable(tool.previewAnimator, tool.animatorController);
		if (!animatorControllerPlayable.IsValid()) {
			return;
		}
		AnimatorStateInfo currentAnimatorStateInfo = animatorControllerPlayable.GetCurrentAnimatorStateInfo(AnimatorControllerTool.tool.selectedLayerIndex);
		AnimatorStateInfo nextAnimatorStateInfo = animatorControllerPlayable.GetNextAnimatorStateInfo(AnimatorControllerTool.tool.selectedLayerIndex);
		AnimatorTransitionInfo animatorTransitionInfo = animatorControllerPlayable.GetAnimatorTransitionInfo(AnimatorControllerTool.tool.selectedLayerIndex);
		int shortNameHash = currentAnimatorStateInfo.shortNameHash;
		int shortNameHash2 = nextAnimatorStateInfo.shortNameHash;
		localLiveLinkInfo.currentStateMachine = ((shortNameHash == 0) ? null : rootStateMachine.FindStateMachine(animatorControllerPlayable.ResolveHash(currentAnimatorStateInfo.fullPathHash)));
		localLiveLinkInfo.currentState = ((shortNameHash == 0) ? null : localLiveLinkInfo.currentStateMachine.FindState(shortNameHash).state);
		localLiveLinkInfo.currentStateNormalizedTime = currentAnimatorStateInfo.normalizedTime;
		localLiveLinkInfo.currentStateLoopTime = currentAnimatorStateInfo.loop;
		if (localLiveLinkInfo.currentState == null) {
			SetLiveLinkInfo(localLiveLinkInfo);
			return;
		}
		localLiveLinkInfo.nextStateMachine = ((shortNameHash2 == 0) ? null : rootStateMachine.FindStateMachine(animatorControllerPlayable.ResolveHash(nextAnimatorStateInfo.fullPathHash)));
		localLiveLinkInfo.nextState = ((shortNameHash2 == 0) ? null : localLiveLinkInfo.nextStateMachine.FindState(shortNameHash2).state);
		localLiveLinkInfo.nextStateNormalizedTime = nextAnimatorStateInfo.normalizedTime;
		localLiveLinkInfo.nextStateLoopTime = nextAnimatorStateInfo.loop;
		localLiveLinkInfo.srcNode = stateMachineGraph.FindNode(localLiveLinkInfo.currentState);
		localLiveLinkInfo.dstNode = ((!(bool) localLiveLinkInfo.nextState) ? null : stateMachineGraph.FindNode(localLiveLinkInfo.nextState));
		localLiveLinkInfo.transitionInfo = animatorTransitionInfo;
		if (!tool.autoLiveLink) {
			SetLiveLinkInfo(localLiveLinkInfo);
			return;
		}
		AnimatorStateMachine animatorStateMachine = localLiveLinkInfo.currentStateMachine;
		if (localLiveLinkInfo.currentState != null && localLiveLinkInfo.nextState != null && (localLiveLinkInfo.transitionInfo.normalizedTime > 0.5 || animatorTransitionInfo.anyState)) {
			animatorStateMachine = localLiveLinkInfo.nextStateMachine;
		}
		if (Event.current.type == EventType.Repaint && shortNameHash != 0) {
			if (animatorStateMachine != activeStateMachine) {
				List<AnimatorStateMachine> hierarchy = new List<AnimatorStateMachine>();
				MecanimUtilities.StateMachineRelativePath(rootStateMachine, animatorStateMachine, ref hierarchy);
				tool.BuildBreadCrumbsFromSMHierarchy(hierarchy);
			}
			else if (tool.liveLinkFollowTransitions) {
				tool.CenterViewOnFocus();
			}
		}
		SetLiveLinkInfo(localLiveLinkInfo);
	}

	private void HandleEvents() {
		Event current = Event.current;
		switch (current.type) {
			case EventType.KeyDown:
				if (current.keyCode == KeyCode.Delete) {
					DeleteSelection();
					current.Use();
				}
				break;
			case EventType.ValidateCommand:
				if (current.commandName == "SoftDelete" || current.commandName == "Delete" || current.commandName == "Copy" || current.commandName == "Paste" || current.commandName == "Duplicate" || current.commandName == "SelectAll") {
					current.Use();
				}
				break;
			case EventType.ExecuteCommand:
				if (current.commandName == "SoftDelete" || current.commandName == "Delete") {
					DeleteSelection();
					current.Use();
				}
				if (current.commandName == "Copy") {
					CopySelectionToPasteboard();
					current.Use();
				}
				else if (current.commandName == "Paste") {
					Undo.IncrementCurrentGroup();
					int currentGroup = Undo.GetCurrentGroup();
					Undo.SetCurrentGroupName("Paste State Machine Data");
					Unsupported.PasteToStateMachineFromPasteboard(activeStateMachine, tool.animatorController, tool.selectedLayerIndex, Vector3.zero);
					Undo.CollapseUndoOperations(currentGroup);
					current.Use();
				}
				else if (current.commandName == "Duplicate") {
					if (CopySelectionToPasteboard()) {
						Vector3 zero = Vector3.zero;
						if (selection.Count > 0) {
							zero.Set(selection[0].position.x, selection[0].position.y, 0f);
						}
						Unsupported.PasteToStateMachineFromPasteboard(activeStateMachine, tool.animatorController, tool.selectedLayerIndex, zero + new Vector3(40f, 40f, 0f));
						current.Use();
					}
				}
				else if (current.commandName == "SelectAll") {
					ClearSelection();
					selection.AddRange(m_Graph.nodes);
					UpdateUnitySelection();
					current.Use();
				}
				break;
		}
	}

	private void HandleContextMenu() {
		Event current = Event.current;
		if (current.type == EventType.ContextClick) {
			GenericMenu genericMenu = new GenericMenu();
			genericMenu.AddItem(EditorGUIUtility.TrTextContent("Create State/Empty"), on: false, AddStateEmptyCallback, Event.current.mousePosition);
			if (Selection.activeObject is Motion) {
				genericMenu.AddItem(EditorGUIUtility.TrTextContent("Create State/From Selected Clip"), on: false, AddStateFromSelectedMotionCallback, Event.current.mousePosition);
			}
			else {
				genericMenu.AddDisabledItem(EditorGUIUtility.TrTextContent("Create State/From Selected Clip"));
			}
			genericMenu.AddItem(EditorGUIUtility.TrTextContent("Create State/From New Blend Tree"), on: false, AddStateFromNewBlendTreeCallback, Event.current.mousePosition);
			genericMenu.AddItem(EditorGUIUtility.TrTextContent("Create Sub-State Machine"), on: false, AddStateMachineCallback, Event.current.mousePosition);
			if (Unsupported.HasStateMachineDataInPasteboard()) {
				genericMenu.AddItem(EditorGUIUtility.TrTextContent("Paste"), on: false, PasteCallback, Event.current.mousePosition);
			}
			else {
				genericMenu.AddDisabledItem(EditorGUIUtility.TrTextContent("Paste"));
			}
			genericMenu.AddItem(EditorGUIUtility.TrTextContent("Copy current StateMachine"), on: false, CopyStateMachineCallback, Event.current.mousePosition);
			genericMenu.ShowAsContext();
		}
	}

	private void AddStateMachineCallback(object data) {
		Undo.RegisterCompleteObjectUndo(activeStateMachine, "Sub-State Machine Added");
		activeStateMachine.AddStateMachine("New StateMachine", (Vector2) data);
		AnimatorControllerTool.tool.RebuildGraph();
	}

	private void PasteCallback(object data) {
		Undo.RegisterCompleteObjectUndo(activeStateMachine, "Paste");
		Unsupported.PasteToStateMachineFromPasteboard(activeStateMachine, tool.animatorController, tool.selectedLayerIndex, (Vector2) data);
		AnimatorControllerTool.tool.RebuildGraph();
	}

	private void AddStateFromNewBlendTreeCallback(object data) {
		Undo.RegisterCompleteObjectUndo(activeStateMachine, "Blend Tree State Added");
		AnimatorState state = activeStateMachine.AddState("Blend Tree", (Vector2) data);
		BlendTree blendTree = new BlendTree();
		blendTree.hideFlags = HideFlags.HideInHierarchy;
		if (AssetDatabase.GetAssetPath(tool.animatorController) != "") {
			AssetDatabase.AddObjectToAsset(blendTree, AssetDatabase.GetAssetPath(tool.animatorController));
		}
		blendTree.name = "Blend Tree";
		string text2 = blendTree.blendParameter = (blendTree.blendParameterY = tool.animatorController.GetDefaultBlendTreeParameter());
		tool.animatorController.SetStateEffectiveMotion(state, blendTree, tool.selectedLayerIndex);
		AnimatorControllerTool.tool.RebuildGraph();
	}

	private void AddStateFromSelectedMotionCallback(object data) {
		AnimationClip animationClip = Selection.activeObject as AnimationClip;
		AnimatorState state = activeStateMachine.AddState(animationClip.name, (Vector2) data);
		tool.animatorController.SetStateEffectiveMotion(state, animationClip, tool.selectedLayerIndex);
		AnimatorControllerTool.tool.RebuildGraph();
	}

	private void AddStateEmptyCallback(object data) {
		activeStateMachine.AddState("New State", (Vector2) data);
		AnimatorControllerTool.tool.RebuildGraph();
	}

	private void CopyStateMachineCallback(object data) {
		Unsupported.CopyStateMachineDataToPasteboard(activeStateMachine, AnimatorControllerTool.tool.animatorController, AnimatorControllerTool.tool.selectedLayerIndex);
	}
}
