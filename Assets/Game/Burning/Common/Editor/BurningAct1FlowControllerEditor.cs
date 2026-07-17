using FilmInspiredGames.Burning;
using UnityEditor;
using UnityEngine;

namespace FilmInspiredGames.Burning.Editor
{
    [CustomEditor(typeof(BurningAct1FlowController))]
    public sealed class BurningAct1FlowControllerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            BurningAct1FlowController flow = (BurningAct1FlowController)target;

            EditorGUILayout.LabelField("현재 진행", EditorStyles.boldLabel);

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.TextField("현재 챕터", Application.isPlaying ? flow.CurrentChapter : "재생 대기");
                EditorGUILayout.TextField("현재 상태", Application.isPlaying ? flow.CurrentState : "-");
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
