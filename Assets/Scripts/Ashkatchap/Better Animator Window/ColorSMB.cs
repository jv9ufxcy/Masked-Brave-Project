using System.Collections.Generic;
using UnityEngine;

#if !UNITY_EDITOR
[SharedBetweenAnimators] // ColorSMB must exits in builds because StateMachineBehaviour desn't support HideFlags.DontSaveInBuild. This attribute is used to avoid using extra resources
#endif
public class ColorSMB : StateMachineBehaviour
#if UNITY_EDITOR
	, IColorSMB
#endif
	{
	[ColorUsage(false, false)]
	[SerializeField] private Color color = Color.white;

	private Dictionary<GUIStyle, GUIStyle> modifiedStyles;
	public GUIStyle GetCachedModifiedStyle(GUIStyle source) {
		if (modifiedStyles == null) modifiedStyles = new Dictionary<GUIStyle, GUIStyle>();
		GUIStyle style;
		if (!modifiedStyles.TryGetValue(source, out style)) {
			style = new GUIStyle(source);
			modifiedStyles.Add(source, style);
		}
		return style;
	}

	public Color GetBgColor() {
		return color;
	}

	public Color GetContentColor() {
		return Color.HSVToRGB(0, 0, (color.r * 299 + color.g * 587 + color.b * 114) / 1000 < 0.5f ? 0.95f : 0.1f);
	}
}
