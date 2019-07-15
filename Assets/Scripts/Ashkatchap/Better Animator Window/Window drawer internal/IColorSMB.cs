using UnityEngine;

public interface IColorSMB {
	GUIStyle GetCachedModifiedStyle(GUIStyle source);
	Color GetBgColor();
	Color GetContentColor();
}
