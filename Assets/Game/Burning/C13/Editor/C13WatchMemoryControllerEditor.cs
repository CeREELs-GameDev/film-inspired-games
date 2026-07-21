using FilmInspiredGames.Burning.C13;
using UnityEditor;
using UnityEngine;

namespace FilmInspiredGames.Burning.C13.Editor
{
    [CustomEditor(typeof(C13WatchMemoryController))]
    public sealed class C13WatchMemoryControllerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            C13WatchMemoryController sequence = (C13WatchMemoryController)target;

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.TextField("현재 챕터", sequence.CurrentChapter);
                EditorGUILayout.TextField("현재 장면", sequence.CurrentState);
                EditorGUILayout.Slider("회전 진행", sequence.Progress, 0f, 1f);
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
