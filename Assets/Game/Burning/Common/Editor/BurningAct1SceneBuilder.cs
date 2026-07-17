using System.Collections.Generic;
using System.Linq;
using FilmInspiredGames.Burning.C02;
using FilmInspiredGames.Burning.C04;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FilmInspiredGames.Burning.Editor
{
    public static class BurningAct1SceneBuilder
    {
        private const string C02ScenePath = "Assets/Game/Burning/C02/Scenes/Burning_C02_Playable.unity";
        private const string ActScenePath = "Assets/Game/Burning/Scenes/Burning_Act1_Playable.unity";
        private const string C01ArtPath = "Assets/Game/Burning/C01/Art/C01_Walk.png";
        private const string C03FirstPath = "Assets/Game/Burning/C03/Art/C03_First.png";
        private const string C03SecondPath = "Assets/Game/Burning/C03/Art/C03_Second.png";
        private const string C04BackgroundPath = "Assets/Game/Burning/C04/Art/C04_Background.png";
        private const string C04CapsuleUpPath = "Assets/Game/Burning/C04/Art/C04_CapsuleUp.png";
        private const string C04CapsuleDownPath = "Assets/Game/Burning/C04/Art/C04_CapsuleDown.png";
        private const string C04WatchPath = "Assets/Game/Burning/C04/Art/C04_Watch.png";

        [MenuItem("Tools/Burning/Build Act 1 Playable Scene")]
        public static void BuildAndOpen()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("재생을 멈춘 뒤 1막 씬을 다시 생성하세요.");
                return;
            }

            PrepareSprite(C01ArtPath);
            PrepareSprite(C03FirstPath);
            PrepareSprite(C03SecondPath);
            PrepareSprite(C04BackgroundPath);
            PrepareSprite(C04CapsuleUpPath);
            PrepareSprite(C04CapsuleDownPath);
            PrepareSprite(C04WatchPath);

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(C02ScenePath) == null)
            {
                Debug.LogError("C02 플레이 씬을 먼저 생성하세요.");
                return;
            }

            AssetDatabase.DeleteAsset(ActScenePath);

            if (!AssetDatabase.CopyAsset(C02ScenePath, ActScenePath))
            {
                Debug.LogError("1막 플레이 씬 복사 실패");
                return;
            }

            AssetDatabase.Refresh();
            Scene scene = EditorSceneManager.OpenScene(ActScenePath, OpenSceneMode.Single);
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            C02SequenceController c02Sequence = Object.FindFirstObjectByType<C02SequenceController>();

            if (canvas == null || c02Sequence == null)
            {
                Debug.LogError("1막 구성에 필요한 Canvas 또는 C02 진행 코드가 없습니다.");
                return;
            }

            GameObject systems = scene.GetRootGameObjects().FirstOrDefault(root => root.name == "Systems")
                ?? new GameObject("Systems");
            Transform sequenceRoot = systems.transform.Find("ChapterSequences");

            if (sequenceRoot == null)
            {
                sequenceRoot = new GameObject("ChapterSequences").transform;
                sequenceRoot.SetParent(systems.transform);
            }

            c02Sequence.transform.SetParent(sequenceRoot);

            RectTransform chaptersRoot = CreateRoot("Chapters", canvas.transform);
            RectTransform c01Root = CreateRoot("C01", chaptersRoot);
            RectTransform c02Root = canvas.transform.Find("C02") as RectTransform;

            if (c02Root == null)
            {
                c02Root = CreateRoot("C02", chaptersRoot);
                MoveChild(canvas.transform, "SceneViewport", c02Root);
                MoveChild(canvas.transform, "Darkness", c02Root);
                MoveChild(canvas.transform, "StackGame", c02Root);
            }
            else
            {
                c02Root.SetParent(chaptersRoot, false);
                Stretch(c02Root);
            }

            RectTransform c03Root = CreateRoot("C03", chaptersRoot);
            RectTransform c04Root = CreateRoot("C04", chaptersRoot);

            Image c01Image = CreateFullScreenImage("C01_Walk", c01Root, LoadSprite(C01ArtPath));
            Image c03First = CreateFullScreenImage("C03_First", c03Root, LoadSprite(C03FirstPath));
            Image c03Second = CreateFullScreenImage("C03_Second", c03Root, LoadSprite(C03SecondPath));
            Image c04Background = CreateFullScreenImage("C04_Background", c04Root, LoadSprite(C04BackgroundPath));
            Image c04CapsuleUp = CreateFullScreenImage("C04_CapsuleUp", c04Root, LoadSprite(C04CapsuleUpPath));
            Image c04CapsuleDown = CreateFullScreenImage("C04_CapsuleDown", c04Root, LoadSprite(C04CapsuleDownPath));
            Image c04Watch = CreateFullScreenImage("C04_Watch", c04Root, LoadSprite(C04WatchPath));
            Image transitionFadeImage = CreateFullScreenImage("TransitionFade", canvas.transform, null);
            transitionFadeImage.color = Color.black;
            c01Image.raycastTarget = true;
            c03First.raycastTarget = true;
            c03Second.raycastTarget = true;
            c04Background.raycastTarget = true;
            c04CapsuleUp.raycastTarget = false;
            c04CapsuleDown.raycastTarget = false;
            c04Watch.raycastTarget = false;
            transitionFadeImage.raycastTarget = false;

            CanvasGroup c01Group = c01Root.gameObject.AddComponent<CanvasGroup>();
            CanvasGroup c02Group = c02Root.gameObject.AddComponent<CanvasGroup>();
            CanvasGroup c03Group = c03Root.gameObject.AddComponent<CanvasGroup>();
            CanvasGroup c04Group = c04Root.gameObject.AddComponent<CanvasGroup>();
            CanvasGroup capsuleUpGroup = c04CapsuleUp.gameObject.AddComponent<CanvasGroup>();
            CanvasGroup capsuleDownGroup = c04CapsuleDown.gameObject.AddComponent<CanvasGroup>();
            CanvasGroup watchGroup = c04Watch.gameObject.AddComponent<CanvasGroup>();
            CanvasGroup transitionFade = transitionFadeImage.gameObject.AddComponent<CanvasGroup>();
            transitionFade.alpha = 0f;

            GameObject c04SequenceObject = new("C04Sequence");
            c04SequenceObject.transform.SetParent(sequenceRoot);
            C04RewardSequenceController c04Sequence = c04SequenceObject.AddComponent<C04RewardSequenceController>();
            ConfigureC04Sequence(
                c04Sequence,
                c04CapsuleUp,
                c04CapsuleDown,
                c04Watch,
                capsuleUpGroup,
                capsuleDownGroup,
                watchGroup);

            GameObject flowObject = new("Act1Flow");
            flowObject.transform.SetParent(systems.transform);
            flowObject.transform.SetSiblingIndex(0);
            BurningAct1FlowController flow = flowObject.AddComponent<BurningAct1FlowController>();
            ConfigureFlow(
                flow,
                c01Group,
                c02Group,
                c03Group,
                c04Group,
                c03First.gameObject,
                c03Second.gameObject,
                c02Sequence,
                c04Sequence,
                transitionFade);
            BurningAct1InputForwarder input = canvas.gameObject.AddComponent<BurningAct1InputForwarder>();
            ConfigureInput(input, flow);
            DisableC02AutoPlay(c02Sequence);
            DisableC04AutoPlay(c04Sequence);

            c01Root.SetSiblingIndex(0);
            c02Root.SetSiblingIndex(1);
            c03Root.SetSiblingIndex(2);
            c04Root.SetSiblingIndex(3);

            EditorSceneManager.SaveScene(scene, ActScenePath);
            AddSceneToBuildSettings();
            Selection.activeGameObject = flowObject;
            Debug.Log("버닝 1막 플레이 씬 생성 완료. Play 후 화면을 클릭하면 C01부터 진행됩니다.");
        }

        private static void ConfigureFlow(
            BurningAct1FlowController flow,
            CanvasGroup c01,
            CanvasGroup c02,
            CanvasGroup c03,
            CanvasGroup c04,
            GameObject c03First,
            GameObject c03Second,
            C02SequenceController c02Sequence,
            C04RewardSequenceController c04Sequence,
            CanvasGroup transitionFade)
        {
            SerializedObject serialized = new(flow);
            serialized.FindProperty("c01Group").objectReferenceValue = c01;
            serialized.FindProperty("c02Group").objectReferenceValue = c02;
            serialized.FindProperty("c03Group").objectReferenceValue = c03;
            serialized.FindProperty("c04Group").objectReferenceValue = c04;
            serialized.FindProperty("c03FirstFrame").objectReferenceValue = c03First;
            serialized.FindProperty("c03SecondFrame").objectReferenceValue = c03Second;
            serialized.FindProperty("c02Sequence").objectReferenceValue = c02Sequence;
            serialized.FindProperty("c04Sequence").objectReferenceValue = c04Sequence;
            serialized.FindProperty("transitionFade").objectReferenceValue = transitionFade;
            serialized.FindProperty("fadeOutDuration").floatValue = 0.65f;
            serialized.FindProperty("fadeHoldDuration").floatValue = 0.18f;
            serialized.FindProperty("fadeInDuration").floatValue = 0.8f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureInput(
            BurningAct1InputForwarder input,
            BurningAct1FlowController flow)
        {
            SerializedObject serialized = new(input);
            serialized.FindProperty("flow").objectReferenceValue = flow;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureC04Sequence(
            C04RewardSequenceController sequence,
            Image capsuleUp,
            Image capsuleDown,
            Image watch,
            CanvasGroup capsuleUpGroup,
            CanvasGroup capsuleDownGroup,
            CanvasGroup watchGroup)
        {
            SerializedObject serialized = new(sequence);
            serialized.FindProperty("capsuleUp").objectReferenceValue = capsuleUpGroup;
            serialized.FindProperty("capsuleDown").objectReferenceValue = capsuleDownGroup;
            serialized.FindProperty("capsuleUpRect").objectReferenceValue = capsuleUp.rectTransform;
            serialized.FindProperty("capsuleDownRect").objectReferenceValue = capsuleDown.rectTransform;
            serialized.FindProperty("watch").objectReferenceValue = watchGroup;
            serialized.FindProperty("watchRect").objectReferenceValue = watch.rectTransform;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void DisableC02AutoPlay(C02SequenceController sequence)
        {
            SerializedObject serialized = new(sequence);
            serialized.FindProperty("playOnStart").boolValue = false;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void DisableC04AutoPlay(C04RewardSequenceController sequence)
        {
            SerializedObject serialized = new(sequence);
            serialized.FindProperty("playOnStart").boolValue = false;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static RectTransform CreateRoot(string name, Transform parent)
        {
            GameObject root = new(name, typeof(RectTransform));
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            Stretch(rect);
            return rect;
        }

        private static Image CreateFullScreenImage(string name, Transform parent, Sprite sprite)
        {
            GameObject imageObject = new(name, typeof(RectTransform), typeof(Image));
            RectTransform rect = imageObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            Stretch(rect);
            Image image = imageObject.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = false;
            return image;
        }

        private static void MoveChild(Transform currentParent, string childName, Transform nextParent)
        {
            Transform child = currentParent.Find(childName);

            if (child != null)
            {
                child.SetParent(nextParent, false);
                Stretch((RectTransform)child);
            }
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void PrepareSprite(string path)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer == null)
            {
                Debug.LogError($"이미지 로드 실패: {path}");
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.maxTextureSize = 4096;
            importer.SaveAndReimport();
        }

        private static Sprite LoadSprite(string path)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void AddSceneToBuildSettings()
        {
            List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes
                .Where(item => item.path != ActScenePath)
                .ToList();
            scenes.Insert(0, new EditorBuildSettingsScene(ActScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
