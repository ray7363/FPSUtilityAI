using System;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Couleurs, styles et utilitaires partagés par les drawers Utility AI.
/// </summary>
internal static class UtilityEditorStyles
{
    // ---------- Couleurs ----------
    public static Color CardColor => EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.045f) : new Color(0f, 0f, 0f, 0.06f);
    public static Color HeaderColor => EditorGUIUtility.isProSkin ? new Color(0.17f, 0.17f, 0.17f, 1f) : new Color(0.72f, 0.72f, 0.72f, 1f);
    public static Color BodyColor => EditorGUIUtility.isProSkin ? new Color(0f, 0f, 0f, 0.15f) : new Color(0f, 0f, 0f, 0.05f);
    public static readonly Color ActionAccent = new Color(1f, 0.62f, 0.2f);
    public static readonly Color TrackColor = new Color(0f, 0f, 0f, 0.3f);

    /// <summary>Couleur stable dérivée de la clé (même clé = même couleur partout).</summary>
    public static Color KeyColor(string key)
    {
        if (string.IsNullOrEmpty(key)) return new Color(0.55f, 0.6f, 0.68f);
        uint h = 2166136261; // FNV-1a : déterministe, contrairement à string.GetHashCode
        foreach (char c in key) { h ^= c; h *= 16777619; }
        return Color.HSVToRGB((h % 360) / 360f, 0.55f, 0.95f);
    }

    // ---------- Styles (lazy : EditorStyles n'est prêt que pendant OnGUI) ----------
    static GUIStyle _placeholder, _rightMini, _centeredHint, _boldFoldout;
    static GUIContent _warnIcon;

    public static GUIStyle Placeholder => _placeholder ??= new GUIStyle(EditorStyles.label)
    {
        fontStyle = FontStyle.Italic,
        normal = { textColor = new Color(0.5f, 0.5f, 0.5f) },
        padding = new RectOffset(4, 0, 0, 0),
    };

    public static GUIStyle RightMiniLabel => _rightMini ??= new GUIStyle(EditorStyles.miniLabel)
    {
        alignment = TextAnchor.MiddleRight,
    };

    public static GUIStyle CenteredHint => _centeredHint ??= new GUIStyle(EditorStyles.centeredGreyMiniLabel)
    {
        fontStyle = FontStyle.Italic,
    };

    public static GUIStyle BoldFoldout => _boldFoldout ??= new GUIStyle(EditorStyles.foldout)
    {
        fontStyle = FontStyle.Bold,
    };

    public static GUIContent WarnIcon => _warnIcon ??= new GUIContent(
        EditorGUIUtility.IconContent("console.warnicon.sml").image,
        "La somme des poids vaut 0 : Evaluate() divisera par zéro (NaN).");

    // ---------- Mini aperçu de courbe ----------
    public static void DrawCurveThumb(Rect r, AnimationCurve curve, Color color)
    {
        if (Event.current.type != EventType.Repaint) return;
        EditorGUI.DrawRect(r, TrackColor);
        if (curve == null || curve.length == 0) return;

        const int n = 24;
        var pts = new Vector3[n];
        for (int i = 0; i < n; i++)
        {
            float t = i / (n - 1f);
            float v = Mathf.Clamp01(curve.Evaluate(t));
            pts[i] = new Vector3(r.x + t * r.width, r.yMax - v * r.height, 0f);
        }
        Color prev = Handles.color;
        Handles.color = color;
        Handles.DrawAAPolyLine(2f, pts);
        Handles.color = prev;
    }

    // ---------- Presets de courbes ----------
    static readonly (string name, Func<AnimationCurve> make)[] Presets =
    {
        ("Linéaire",            () => AnimationCurve.Linear(0, 0, 1, 1)),
        ("Linéaire inversée",   () => AnimationCurve.Linear(0, 1, 1, 0)),
        ("Ease In (x²)",        () => new AnimationCurve(new Keyframe(0, 0, 0, 0), new Keyframe(1, 1, 2, 2))),
        ("Ease Out",            () => new AnimationCurve(new Keyframe(0, 0, 2, 2), new Keyframe(1, 1, 0, 0))),
        ("Smoothstep (S)",      () => AnimationCurve.EaseInOut(0, 0, 1, 1)),
        ("Smoothstep inversé",  () => AnimationCurve.EaseInOut(0, 1, 1, 0)),
        ("Cloche",              () => new AnimationCurve(new Keyframe(0, 0, 0, 0), new Keyframe(0.5f, 1, 0, 0), new Keyframe(1, 0, 0, 0))),
        ("Seuil (0.5)",         () => new AnimationCurve(
                                        new Keyframe(0, 0, 0, float.PositiveInfinity),
                                        new Keyframe(0.5f, 1, float.PositiveInfinity, 0),
                                        new Keyframe(1, 1, 0, 0))),
        ("Constante (1)",       () => AnimationCurve.Constant(0, 1, 1)),
    };

    public static void ShowPresetMenu(Rect rect, SerializedProperty curveProp)
    {
        // On capture objet + chemin : la SerializedProperty elle-même n'est plus valide quand le menu répond.
        SerializedObject so = curveProp.serializedObject;
        string path = curveProp.propertyPath;

        var menu = new GenericMenu();
        foreach (var (name, make) in Presets)
        {
            menu.AddItem(new GUIContent(name), false, () =>
            {
                so.Update();
                so.FindProperty(path).animationCurveValue = make();
                so.ApplyModifiedProperties(); // gère aussi l'Undo
            });
        }
        menu.DropDown(rect);
    }
}
