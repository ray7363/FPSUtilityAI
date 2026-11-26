using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

/// <summary>
/// Affiche une UtilityAction avec :
///  - un en-tête repliable où le Name s'édite directement (+ mini-aperçus des courbes,
///    nb de considérations, alerte si Σ poids = 0)
///  - une ReorderableList de considérations (dessinées par ConsiderationDrawer)
/// </summary>
[CustomPropertyDrawer(typeof(UtilityAction))]
public class UtilityActionDrawer : PropertyDrawer
{
    const float HeaderHeight = 24f;
    const float Spacing = 4f;
    const float BodyPad = 6f;
    const float FoldoutW = 14f;
    const float NameMinW = 120f;
    const float NameMaxW = 240f;
    const float ThumbW = 28f;
    const float ThumbH = 14f;
    const float ThumbGap = 3f;

    static float Line => EditorGUIUtility.singleLineHeight;

    static GUIStyle _nameField;
    static GUIStyle NameField => _nameField ??= new GUIStyle(EditorStyles.textField) { fontStyle = FontStyle.Bold };

    // Un drawer peut servir pour plusieurs éléments d'une liste : une ReorderableList par chemin.
    readonly Dictionary<string, ReorderableList> _lists = new();

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float h = HeaderHeight;
        if (property.isExpanded)
            h += Spacing + GetList(property).GetHeight() + Spacing;
        return h;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        position = EditorGUI.IndentedRect(position);
        int indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 0;

        DrawHeader(position, property, label);

        if (property.isExpanded)
        {
            ReorderableList list = GetList(property);
            Rect body = new Rect(position.x, position.y + HeaderHeight, position.width, position.height - HeaderHeight);
            if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(body, UtilityEditorStyles.BodyColor);
                EditorGUI.DrawRect(new Rect(body.x, body.y, 3f, body.height),
                    UtilityEditorStyles.ActionAccent * new Color(1f, 1f, 1f, 0.35f));
            }

            Rect listRect = new Rect(body.x + BodyPad, body.y + Spacing, body.width - BodyPad * 2f, list.GetHeight());
            list.DoList(listRect);
        }

        EditorGUI.indentLevel = indent;
        EditorGUI.EndProperty();
    }

    // ------------------------------------------------------------------ En-tête

    void DrawHeader(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty nameProp = property.FindPropertyRelative(nameof(UtilityAction.Name));
        SerializedProperty considerations = property.FindPropertyRelative(nameof(UtilityAction.Considerations));

        Rect header = new Rect(position.x, position.y, position.width, HeaderHeight);
        if (Event.current.type == EventType.Repaint)
        {
            EditorGUI.DrawRect(header, UtilityEditorStyles.HeaderColor);
            EditorGUI.DrawRect(new Rect(header.x, header.y, 3f, header.height), UtilityEditorStyles.ActionAccent);
        }

        int count = considerations.arraySize;
        float weightSum = 0f;
        for (int i = 0; i < count; i++)
            weightSum += Mathf.Max(0f, considerations.GetArrayElementAtIndex(i)
                .FindPropertyRelative(nameof(Consideration.Weight)).floatValue);

        float lineY = header.y + (HeaderHeight - Line) * 0.5f;

        // ---- Côté droit (de droite à gauche) : compteur puis éventuelle alerte ----
        float right = header.xMax - 6f;
        string countText = count == 0 ? "aucune considération" : $"{count} considération{(count > 1 ? "s" : "")}";
        Rect countRect = new Rect(right - 110f, header.y, 110f, header.height);
        GUI.Label(countRect, countText, UtilityEditorStyles.RightMiniLabel);
        right = countRect.x - 4f;

        if (count > 0 && weightSum <= 0f)
        {
            Rect warnRect = new Rect(right - 18f, header.y + (HeaderHeight - 18f) * 0.5f, 18f, 18f);
            GUI.Label(warnRect, UtilityEditorStyles.WarnIcon);
            right = warnRect.x - 4f;
        }

        // ---- Flèche de foldout ----
        Rect foldRect = new Rect(header.x + 8f, lineY, FoldoutW, Line);
        property.isExpanded = EditorGUI.Foldout(foldRect, property.isExpanded, GUIContent.none, true);

        // ---- Nom éditable directement dans l'en-tête ----
        string fallback = label.text.StartsWith("Element ") ? "Action " + label.text.Substring(8) : label.text;
        float nameX = foldRect.xMax + 4f;
        float wanted = NameField.CalcSize(new GUIContent(string.IsNullOrEmpty(nameProp.stringValue) ? fallback : nameProp.stringValue)).x + 16f;
        float nameW = Mathf.Min(Mathf.Clamp(wanted, NameMinW, NameMaxW), Mathf.Max(40f, right - nameX));
        Rect nameRect = new Rect(nameX, lineY, nameW, Line);

        EditorGUI.BeginChangeCheck();
        EditorGUI.showMixedValue = nameProp.hasMultipleDifferentValues;
        string newName = EditorGUI.TextField(nameRect, nameProp.stringValue, NameField);
        EditorGUI.showMixedValue = false;
        if (EditorGUI.EndChangeCheck())
            nameProp.stringValue = newName;
        if (string.IsNullOrEmpty(nameProp.stringValue) && Event.current.type == EventType.Repaint)
            UtilityEditorStyles.Placeholder.Draw(nameRect, new GUIContent(fallback + " (sans nom)"), false, false, false, false);

        // ---- Mini-aperçus des courbes entre le nom et le compteur ----
        float thumbX = nameRect.xMax + 8f;
        int fit = Mathf.FloorToInt((right - thumbX + ThumbGap) / (ThumbW + ThumbGap));
        int shown = Mathf.Min(count, Mathf.Max(0, fit));
        float thumbY = header.y + (header.height - ThumbH) * 0.5f;
        for (int i = 0; i < shown; i++)
        {
            SerializedProperty c = considerations.GetArrayElementAtIndex(i);
            Rect r = new Rect(thumbX + i * (ThumbW + ThumbGap), thumbY, ThumbW, ThumbH);
            string key = c.FindPropertyRelative(nameof(Consideration.Key)).stringValue;
            UtilityEditorStyles.DrawCurveThumb(r, c.FindPropertyRelative(nameof(Consideration.Curve)).animationCurveValue,
                UtilityEditorStyles.KeyColor(key));
            if (!string.IsNullOrEmpty(key))
                GUI.Label(r, new GUIContent(string.Empty, key)); // tooltip = clé
        }

        // ---- Cliquer ailleurs dans l'en-tête replie/déplie (le champ Nom garde ses clics) ----
        Event e = Event.current;
        if (e.type == EventType.MouseDown && e.button == 0 && header.Contains(e.mousePosition) && !nameRect.Contains(e.mousePosition))
        {
            property.isExpanded = !property.isExpanded;
            e.Use();
        }
    }

    // ------------------------------------------------------------------ Liste

    ReorderableList GetList(SerializedProperty property)
    {
        SerializedProperty considerations = property.FindPropertyRelative(nameof(UtilityAction.Considerations));
        string key = property.propertyPath;

        if (_lists.TryGetValue(key, out ReorderableList existing) &&
            existing.serializedProperty.serializedObject == property.serializedObject)
        {
            existing.serializedProperty = considerations; // les SerializedProperty sont des itérateurs : on rebinde
            return existing;
        }

        ReorderableList list = null;
        list = new ReorderableList(property.serializedObject, considerations, true, true, true, true)
        {
            headerHeight = 20f,
            footerHeight = 18f,

            drawHeaderCallback = rect =>
            {
                SerializedProperty arr = list.serializedProperty;
                float sum = 0f;
                for (int i = 0; i < arr.arraySize; i++)
                    sum += arr.GetArrayElementAtIndex(i).FindPropertyRelative(nameof(Consideration.Weight)).floatValue;

                EditorGUI.LabelField(rect, "Considérations", EditorStyles.boldLabel);
                GUI.Label(rect, $"Σ poids : {sum:0.##}", UtilityEditorStyles.RightMiniLabel);
            },

            drawNoneElementCallback = rect =>
                GUI.Label(rect, "Aucune considération — cliquez sur +", UtilityEditorStyles.CenteredHint),

            elementHeightCallback = index =>
                EditorGUI.GetPropertyHeight(list.serializedProperty.GetArrayElementAtIndex(index)) + 4f,

            drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                SerializedProperty element = list.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2f;
                rect.height -= 4f;
                EditorGUI.PropertyField(rect, element, GUIContent.none, true);
            },

            onAddCallback = l =>
            {
                SerializedProperty arr = l.serializedProperty;
                int i = arr.arraySize;
                arr.InsertArrayElementAtIndex(i);
                SerializedProperty el = arr.GetArrayElementAtIndex(i);
                el.FindPropertyRelative(nameof(Consideration.Key)).stringValue = string.Empty;
                el.FindPropertyRelative(nameof(Consideration.Weight)).floatValue = 1f;
                el.FindPropertyRelative(nameof(Consideration.Curve)).animationCurveValue = AnimationCurve.Linear(0f, 0f, 1f, 1f);
                arr.serializedObject.ApplyModifiedProperties();
                l.index = i;
            },
        };

        _lists[key] = list;
        return list;
    }
}
