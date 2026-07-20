using FilmInspiredGames.Burning.C08;
using UnityEditor;
using UnityEngine;

namespace FilmInspiredGames.Burning.C08.Editor
{
    [CustomEditor(typeof(C08ToC12SequenceController))]
    public sealed class C08ToC12SequenceControllerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            C08ToC12SequenceController sequence = (C08ToC12SequenceController)target;

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.TextField("현재 챕터", sequence.CurrentChapter);
                EditorGUILayout.TextField("현재 장면", sequence.CurrentState);
                EditorGUILayout.Toggle("C11 터치 대기", sequence.IsWaitingForC11Touch);
            }

            EditorGUILayout.Space();
            DrawDefaultInspector();

            if (Application.isPlaying)
            {
                Repaint();
            }
        }
    }
}
