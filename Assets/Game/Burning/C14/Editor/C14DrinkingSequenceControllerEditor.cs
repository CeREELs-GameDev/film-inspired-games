using FilmInspiredGames.Burning.C14;
using UnityEditor;

namespace FilmInspiredGames.Burning.C14.Editor
{
    [CustomEditor(typeof(C14DrinkingSequenceController))]
    public sealed class C14DrinkingSequenceControllerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            C14DrinkingSequenceController sequence = (C14DrinkingSequenceController)target;
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.TextField("현재 챕터", sequence.CurrentChapter);
                EditorGUILayout.TextField("현재 파트", sequence.CurrentPart);
                EditorGUILayout.TextField("현재 장면", sequence.CurrentState);
                EditorGUILayout.Slider("잔에 찬 술", sequence.GlassFill, 0f, 1f);
                EditorGUILayout.Slider("병에 남은 술", sequence.BottleFill, 0f, 1f);
            }

            EditorGUILayout.Space();
            DrawDefaultInspector();
            if (EditorApplication.isPlaying)
            {
                Repaint();
            }
        }
    }
}
