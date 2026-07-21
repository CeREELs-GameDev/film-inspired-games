using FilmInspiredGames.Burning.C15;
using UnityEditor;

namespace FilmInspiredGames.Burning.C15.Editor
{
    [CustomEditor(typeof(C15SequenceController))]
    public sealed class C15SequenceControllerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            C15SequenceController sequence = (C15SequenceController)target;
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("현재 챕터", sequence.CurrentChapter);
            EditorGUILayout.LabelField("진행 상태", sequence.CurrentState);

            if (EditorApplication.isPlaying)
            {
                Repaint();
            }
        }
    }
}
