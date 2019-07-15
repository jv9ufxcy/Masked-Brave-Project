using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEditor.Graphs.AnimationStateMachine;
using UnityEngine;

class CustomEdgeGUI : IEdgeGUI {
	private const float kEdgeWidth = 5f;
	private const float kArrowEdgeWidth = 2f;
	private const float kArrowSize = 5f;
	private const float kArrowLength = 13f;
	private const float kEdgeClickWidth = 10f;
	private const float kEdgeToSelfOffset = 30f;
	private const float kEdgeCrossDistance = 5f;
	private const float kMinSizeZoomMultiplier = 0.25f;
	private const float kMinDistanceZoomMultiplier = 0.33f;

	private Edge m_DraggingEdge;

	private static Slot s_TargetDraggingSlot;

	public UnityEditor.Graphs.GraphGUI host { get; set; }

	private UnityEditor.Graphs.AnimationStateMachine.GraphGUI smHost => host as UnityEditor.Graphs.AnimationStateMachine.GraphGUI;

	public List<int> edgeSelection { get; set; }

	private static Vector3 edgeToSelfOffsetVector => new Vector3(0f, 30f, 0f);

	private static Color selectedEdgeColor = new Color(0.42f, 0.7f, 1f, 1f);

	private static Color selectorTransitionColor = new Color(0.5f, 0.5f, 0.5f, 1f);

	private static Color defaultTransitionColor = new Color(0.6f, 0.4f, 0f, 1f);

	private static Color transitionColorToExit = new Color(1f, 0f, 0f, 0.5f);
	private static Color transitionColorFromEntry = new Color(0f, 1f, 0f, 1f);
	private static Color transitionColorFromAnyState = new Color(0.454902f, 0.7921569f, 0.7294118f, 1f);

	private float edgeSizeMultiplier => 1f / Mathf.Max(0.25f, host.zoomLevel);

	private float edgeDistanceMultiplier => 1f / Mathf.Max(0.33f, host.zoomLevel);

	public CustomEdgeGUI() {
		edgeSelection = new List<int>();
	}

	public void DoEdges() {
		if (Event.current.type == EventType.Repaint) {
			int num = 0;
			foreach (Edge edge in host.graph.edges) {
				Texture2D tex = (Texture2D) Styles.connectionTexture.image;
				Color color = edge.color;
				EdgeInfo edgeInfo = smHost.stateMachineGraph.GetEdgeInfo(edge);
				if (edgeInfo != null) {
					if (edgeInfo.hasDefaultState) {
						color = defaultTransitionColor;
					}
					else if (edgeInfo.edgeType == EdgeType.Transition) {
						color = selectorTransitionColor;
					}
					else {
						var firstTransition = edgeInfo.transitions[0];

						if (firstTransition.isAnyStateTransition) {
							color = transitionColorFromAnyState;
						}
						else if (firstTransition.transition.isExit) {
							color = transitionColorToExit;
						}
					}
				}

				for (int i = 0; i < edgeSelection.Count; i++) {
					if (edgeSelection[i] == num) {
						color = selectedEdgeColor;
						break;
					}
				}
				DrawEdge(edge, tex, color, edgeInfo);
				num++;
			}
		}
		if (IsDragging()) {
			s_TargetDraggingSlot = null;
			Event.current.Use();
		}
		if (ShouldStopDragging()) {
			EndDragging();
			Event.current.Use();
		}
	}

	public void EndDragging() {
		if (m_DraggingEdge != null) {
			host.graph.RemoveEdge(m_DraggingEdge);
			m_DraggingEdge = null;
			smHost.tool.Repaint();
		}
	}

	private bool IsDragging() {
		return Event.current.type == EventType.MouseMove && m_DraggingEdge != null;
	}

	private bool ShouldStopDragging() {
		return Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape && m_DraggingEdge != null;
	}

	private void DrawEdge(Edge edge, Texture2D tex, Color color, EdgeInfo info) {
		Vector3 cross;
		Vector3[] edgePoints = GetEdgePoints(edge, out cross);
		float arrowSize = 5f * edgeSizeMultiplier;
		float outlineWidth = 2f * edgeSizeMultiplier;
		float arrowLength = 13f * edgeSizeMultiplier;
		Handles.color = color;
		if (edgePoints[0] == edgePoints[1]) {
			DrawArrows(color, -Vector3.right, new Vector3[2]
			{
				edgePoints[0] + new Vector3(0f, 31f, 0f),
				edgePoints[0] + new Vector3(0f, 30f, 0f)
			}, info, true, arrowSize, outlineWidth, arrowLength);
			return;
		}
		float width = 10f * edgeSizeMultiplier;
		Handles.DrawAAPolyLine(tex, width, edgePoints[0], edgePoints[1]);
		DrawArrows(color, cross, edgePoints, info, false, arrowSize, outlineWidth, arrowLength);
		if (info == null) {
			return;
		}
		UnityEditor.Graphs.AnimationStateMachine.GraphGUI.LiveLinkInfo liveLinkInfo = smHost.liveLinkInfo;
		bool flag = liveLinkInfo.srcNode == edge.fromSlot.node;
		UnityEditor.Graphs.AnimationStateMachine.GraphGUI.LiveLinkInfo liveLinkInfo2 = smHost.liveLinkInfo;
		bool flag2 = liveLinkInfo2.dstNode == edge.toSlot.node;
		if (smHost.liveLinkInfo.transitionInfo.entry ? ((flag2 && edge.fromSlot.node is EntryNode) || (flag && flag2 && edge.toSlot.node is StateMachineNode)) : (smHost.liveLinkInfo.transitionInfo.anyState ? (flag2 && edge.fromSlot.node is AnyStateNode) : ((!smHost.liveLinkInfo.transitionInfo.exit) ? (flag && flag2) : ((flag && edge.toSlot.node is ExitNode) || (flag2 && edge.fromSlot.node is EntryNode))))) {
			float num = smHost.liveLinkInfo.transitionInfo.normalizedTime;
			if (smHost.liveLinkInfo.currentStateMachine != smHost.liveLinkInfo.nextStateMachine && !smHost.liveLinkInfo.transitionInfo.anyState) {
				num = num % 0.5f / 0.5f;
			}
			num = Mathf.Clamp(num, 0f, 1f);
			Handles.color = selectedEdgeColor;
			Handles.DrawAAPolyLine(width, edgePoints[0], edgePoints[1] * num + edgePoints[0] * (1f - num));
		}
	}

	private static void DrawArrows(Color color, Vector3 cross, Vector3[] edgePoints, EdgeInfo info, bool isSelf, float arrowSize, float outlineWidth, float arrowLength) {
		Vector3 a = edgePoints[1] - edgePoints[0];
		Vector3 normalized = a.normalized;
		Vector3 a2 = a * 0.5f + edgePoints[0];
		a2 -= cross * 0.5f;
		float num = Mathf.Min(arrowLength * 1.5f, (a2 - edgePoints[0]).magnitude) * 0.66f;
		int num2 = 1;
		if (info != null && info.hasMultipleTransitions) {
			num2 = 3;
		}
		for (int i = 0; i < num2; i++) {
			Color color2 = color;
			if (info != null) {
				if (info.debugState == EdgeDebugState.MuteAll) {
					color2 = Color.red;
				}
				else if (info.debugState == EdgeDebugState.SoloAll) {
					color2 = Color.green;
				}
				else {
					switch (i) {
						case 0:
							if (info.debugState == EdgeDebugState.MuteSome || info.debugState == EdgeDebugState.MuteAndSolo) {
								color2 = Color.red;
							}
							if (info.debugState == EdgeDebugState.SoloSome) {
								color2 = Color.green;
							}
							break;
						case 2:
							if (info.debugState == EdgeDebugState.MuteAndSolo) {
								color2 = Color.green;
							}
							break;
					}
				}
				if (i == 1 && info.edgeType == EdgeType.MixedTransition) {
					color2 = selectorTransitionColor;
				}
			}
			Vector3 center = a2 + (float) ((num2 != 1) ? (i - 1) : i) * num * ((!isSelf) ? normalized : cross);
			DrawArrow(color2, cross, normalized, center, arrowSize, outlineWidth);
		}
	}

	private static void DrawArrow(Color color, Vector3 cross, Vector3 direction, Vector3 center, float arrowSize, float outlineWidth) {
		if (Event.current.type == EventType.Repaint) {
			Vector3[] array = new Vector3[4];
			array[0] = center + direction * arrowSize;
			array[1] = center - direction * arrowSize + cross * arrowSize;
			array[2] = center - direction * arrowSize - cross * arrowSize;
			array[3] = array[0];
			Shader.SetGlobalColor("_HandleColor", color);
			HandleUtility.ApplyWireMaterial();
			GL.Begin(4);
			GL.Color(color);
			GL.Vertex(array[0]);
			GL.Vertex(array[1]);
			GL.Vertex(array[2]);
			GL.End();
			Handles.color = color;
			Handles.DrawAAPolyLine((Texture2D) Styles.connectionTexture.image, outlineWidth, array);
		}
	}

	private Vector3[] GetEdgePoints(Edge edge) {
		Vector3 cross;
		return GetEdgePoints(edge, out cross);
	}

	private Vector3[] GetEdgePoints(Edge edge, out Vector3 cross) {
		float d = 5f * edgeDistanceMultiplier;
		Vector3[] array = new Vector3[2]
		{
			GetEdgeStartPosition(edge),
			GetEdgeEndPosition(edge)
		};
		cross = Vector3.Cross((array[0] - array[1]).normalized, Vector3.forward);
		array[0] += cross * d;
		if (!IsEdgeBeingDragged(edge)) {
			array[1] += cross * d;
		}
		return array;
	}

	private static Vector3 GetEdgeStartPosition(Edge edge) {
		return GetNodeCenterFromSlot(edge.fromSlot);
	}

	private static Vector3 GetEdgeEndPosition(Edge edge) {
		if (IsEdgeBeingDragged(edge)) {
			if (s_TargetDraggingSlot != null) {
				return GetNodeCenterFromSlot(s_TargetDraggingSlot);
			}
			return Event.current.mousePosition;
		}
		return GetNodeCenterFromSlot(edge.toSlot);
	}

	private static bool IsEdgeBeingDragged(Edge edge) {
		return edge.toSlot == null;
	}

	private static Vector3 GetNodeCenterFromSlot(Slot slot) {
		return slot.node.position.center;
	}

	public void DoDraggedEdge() {
	}

	public void BeginSlotDragging(Slot slot, bool allowStartDrag, bool allowEndDrag) {
		EndDragging();
		Edge edge = new Edge(slot, null);
		host.graph.edges.Add(edge);
		m_DraggingEdge = edge;
		smHost.tool.wantsMouseMove = true;
	}

	public void SlotDragging(Slot slot, bool allowEndDrag, bool allowMultiple) {
		if (m_DraggingEdge != null && !(slot.node is AnyStateNode)) {
			s_TargetDraggingSlot = slot;
			Event.current.Use();
		}
	}

	public void EndSlotDragging(Slot slot, bool allowMultiple) {
		if (m_DraggingEdge != null && !(slot.node is AnyStateNode)) {
			UnityEditor.Graphs.AnimationStateMachine.Node node = m_DraggingEdge.fromSlot.node as UnityEditor.Graphs.AnimationStateMachine.Node;
			UnityEditor.Graphs.AnimationStateMachine.Node toNode = slot.node as UnityEditor.Graphs.AnimationStateMachine.Node;
			if (slot == m_DraggingEdge.fromSlot) {
				host.graph.RemoveEdge(m_DraggingEdge);
			}
			else {
				m_DraggingEdge.toSlot = slot;
				host.selection.Clear();
				host.selection.Add(node);
				Selection.activeObject = node.selectionObject;
				node.Connect(toNode, m_DraggingEdge);
			}
			m_DraggingEdge = null;
			s_TargetDraggingSlot = null;
			Event.current.Use();
			smHost.tool.wantsMouseMove = false;
			AnimatorControllerTool.tool.RebuildGraph();
		}
	}

	public Edge FindClosestEdge() {
		Edge result = null;
		float num = float.PositiveInfinity;
		Vector3 vector = Event.current.mousePosition;
		foreach (Edge edge in host.graph.edges) {
			Vector3[] edgePoints = GetEdgePoints(edge);
			float num2 = float.PositiveInfinity;
			num2 = ((!(edgePoints[0] == edgePoints[1])) ? HandleUtility.DistancePointLine(vector, edgePoints[0], edgePoints[1]) : Vector3.Distance(edgeToSelfOffsetVector + edgePoints[0], vector));
			if (num2 < num && num2 < 10f) {
				num = num2;
				result = edge;
			}
		}
		return result;
	}
}
