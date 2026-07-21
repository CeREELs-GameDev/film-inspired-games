using FilmInspiredGames.Burning.C16;
using UnityEditor;

namespace FilmInspiredGames.Burning.C16.Editor
{
    [CustomEditor(typeof(C16ToC18SequenceController))]
    public sealed class C16ToC18SequenceControllerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            C16ToC18SequenceController sequence = (C16ToC18SequenceController)target;
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
