using UnityEditor;
using UnityEngine;

/// <summary>
/// Affiche une Consideration sous forme de "carte" :
///  [Clé ............] [Presets ▾] [Poids 1.0] 33%
///  ▬▬▬▬▬▬▬▬▬▬▬ (part du poids dans l'action)
///  ┌──────────── courbe 0..1 ────────────┐
/// </summary>
[CustomPropertyDrawer(typeof(Consideration))]
public class ConsiderationDrawer : PropertyDrawer
{
    const float Pad = 4f;
    const float Gap = 3f;
    const float BarHeight = 3f;
    const float CurveHeight = 56f;
    const float StripeWidth = 4f;

    const float PresetW = 66f;
    const float WeightW = 84f;
    const float ShareW = 34f;
    const float Space = 4f;

    static float Line => EditorGUIUtility.singleLineHeight;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        => Pad + Line + Gap + BarHeight + Gap + CurveHeight + Pad;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty keyProp = property.FindPropertyRelative(nameof(Consideration.Key));
        SerializedProperty curveProp = property.FindPropertyRelative(nameof(Consideration.Curve));
        SerializedProperty weightProp = property.FindPropertyRelative(nameof(Consideration.Weight));

        EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.IndentedRect(position);
        int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        Color accent = UtilityEditorStyles.KeyColor(keyProp.stringValue);

        // ---- Carte + bande de couleur ----
        if (Event.current.type == EventType.Repaint)
        {
            EditorGUI.DrawRect(position, UtilityEditorStyles.CardColor);
            EditorGUI.DrawRect(new Rect(position.x, position.y, StripeWidth, position.height), accent);
        }

        Rect inner = new Rect(
            position.x + StripeWidth + Pad, position.y + Pad,
            position.width - StripeWidth - Pad * 2f, position.height - Pad * 2f);

        // ---- Ligne 1 : clé / presets / poids / % ----
        float keyW = Mathf.Max(40f, inner.width - PresetW - WeightW - ShareW - Space * 3f);
        Rect keyRect = new Rect(inner.x, inner.y, keyW, Line);
        Rect presetRect = new Rect(keyRect.xMax + Space, inner.y, PresetW, Line);
        Rect weightRect = new Rect(presetRect.xMax + Space, inner.y, WeightW, Line);
        Rect shareRect = new Rect(weightRect.xMax + Space, inner.y, ShareW, Line);

        EditorGUI.PropertyField(keyRect, keyProp, GUIContent.none);
        if (string.IsNullOrEmpty(keyProp.stringValue) && Event.current.type == EventType.Repaint)
            UtilityEditorStyles.Placeholder.Draw(keyRect, new GUIContent("Clé…"), false, false, false, false);

        if (EditorGUI.DropdownButton(presetRect, new GUIContent("Presets", "Remplacer la courbe par un preset"),
                FocusType.Passive, EditorStyles.popup))
        {
            UtilityEditorStyles.ShowPresetMenu(presetRect, curveProp);
        }

        float prevLabelWidth = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 38f; // label court : on peut glisser dessus pour changer la valeur
        EditorGUI.BeginChangeCheck();
        EditorGUI.PropertyField(weightRect, weightProp, new GUIContent("Poids", "Poids relatif dans l'action (≥ 0)"));
        if (EditorGUI.EndChangeCheck() && weightProp.floatValue < 0f)
            weightProp.floatValue = 0f;
        EditorGUIUtility.labelWidth = prevLabelWidth;

        float share = GetWeightShare(property, weightProp.floatValue);
        if (share >= 0f)
            GUI.Label(shareRect, new GUIContent($"{share * 100f:0}%", "Part de ce poids dans la somme des poids de l'action"),
                UtilityEditorStyles.RightMiniLabel);

        // ---- Barre de part du poids ----
        Rect bar = new Rect(inner.x, keyRect.yMax + Gap, inner.width, BarHeight);
        if (Event.current.type == EventType.Repaint)
        {
            EditorGUI.DrawRect(bar, UtilityEditorStyles.TrackColor);
            if (share > 0f)
                EditorGUI.DrawRect(new Rect(bar.x, bar.y, bar.width * share, bar.height), accent);
        }

        // ---- Courbe (bornée 0..1 sur les deux axes) ----
        Rect curveRect = new Rect(inner.x, bar.yMax + Gap, inner.width, CurveHeight);
        EditorGUI.CurveField(curveRect, curveProp, accent, new Rect(0f, 0f, 1f, 1f));

        if (Event.current.type == EventType.Repaint && !curveProp.hasMultipleDifferentValues)
        {
            AnimationCurve c = curveProp.animationCurveValue;
            if (c == null || c.length == 0)
                GUI.Label(curveRect, "Courbe vide — choisissez un preset", UtilityEditorStyles.CenteredHint);
        }

        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }

    /// <summary>
    /// Retrouve la liste parente (".../Considerations.Array.data[i]") et renvoie
    /// poids / somme des poids. -1 si la Consideration n'est pas dans une liste.
    /// </summary>
    static float GetWeightShare(SerializedProperty property, float weight)
    {
        string path = property.propertyPath;
        int idx = path.LastIndexOf(".Array.data[", System.StringComparison.Ordinal);
        if (idx < 0) return -1f;

        SerializedProperty array = property.serializedObject.FindProperty(path.Substring(0, idx));
        if (array == null || !array.isArray) return -1f;

        float sum = 0f;
        for (int i = 0; i < array.arraySize; i++)
        {
            SerializedProperty w = array.GetArrayElementAtIndex(i).FindPropertyRelative(nameof(Consideration.Weight));
            if (w != null) sum += Mathf.Max(0f, w.floatValue);
        }
        return sum > 0f ? Mathf.Max(0f, weight) / sum : 0f;
    }
}
