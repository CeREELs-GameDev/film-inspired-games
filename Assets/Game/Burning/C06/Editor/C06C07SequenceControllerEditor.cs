using FilmInspiredGames.Burning.C06;
using UnityEditor;
using UnityEngine;

namespace FilmInspiredGames.Burning.C06.Editor
{
    [CustomEditor(typeof(C06C07SequenceController))]
    public sealed class C06C07SequenceControllerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            C06C07SequenceController sequence = (C06C07SequenceController)target;

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.TextField("현재 챕터", sequence.CurrentChapter);
                EditorGUILayout.TextField("현재 장면", sequence.CurrentState);
                EditorGUILayout.Toggle("퍼즐 클릭 대기", sequence.IsWaitingForPuzzleBreak);
                EditorGUILayout.Toggle("퍼즐 진행 중", sequence.IsPuzzlePlayable);
                EditorGUILayout.IntField("맞춘 퍼즐", sequence.PlacedPuzzleCount);
                EditorGUILayout.Toggle("C08 이동 대기", sequence.IsWaitingForNextScene);
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
