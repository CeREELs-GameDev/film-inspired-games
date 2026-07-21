using System.Collections.Generic;
using System.Linq;
using FilmInspiredGames.Burning.C15;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FilmInspiredGames.Burning.C15.Editor
{
    public static class C15PlayableSceneBuilder
    {
        private const string ScenePath = "Assets/Game/Burning/C15/Scenes/Burning_C15_Playable.unity";
        private const string ArtPath = "Assets/Game/Burning/C15/Art/";
        private const string ControllerPath = "Assets/Game/Burning/C15/Scripts/C15SequenceController.cs";

        [MenuItem("Tools/Burning/Build C15 Playable Scene")]
        public static void BuildAndOpen()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("재생을 멈춘 뒤 C15 씬을 다시 생성하세요.");
                return;
            }

            PrepareSprites();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Burning_C15_Playable";
            CreateCamera();
            Canvas canvas = CreateCanvas();

            CreateImage("Black", canvas.transform, LoadSprite("C15_Black.png"), false);
            CanvasGroup[] cuts =
            {
                CreateImage("Cut01", canvas.transform, LoadSprite("C15_Cut01.png"), true),
                CreateImage("Cut02", canvas.transform, LoadSprite("C15_Cut02.png"), true),
                CreateImage("Cut03", canvas.transform, LoadSprite("C15_Cut03.png"), true)
            };

            C15SequenceController controller = canvas.gameObject.AddComponent<C15SequenceController>();
            SerializedObject serialized = new(controller);
            MonoScript controllerScript = AssetDatabase.LoadAssetAtPath<MonoScript>(ControllerPath);
            if (controllerScript == null)
            {
                throw new System.InvalidOperationException("C15 컨트롤러 스크립트를 찾을 수 없음");
            }
            serialized.FindProperty("m_Script").objectReferenceValue = controllerScript;
            SerializedProperty cutArray = serialized.FindProperty("cuts");
            cutArray.arraySize = cuts.Length;
            for (int index = 0; index < cuts.Length; index++)
            {
                cutArray.GetArrayElementAtIndex(index).objectReferenceValue = cuts[index];
            }
            serialized.FindProperty("nextSceneName").stringValue = "Burning_C16_C18_Playable";
            serialized.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuildSettings();
            Selection.activeGameObject = canvas.gameObject;
            Debug.Log("C15 플레이 씬 생성 완료: 검정과 세 컷 순차 재생");
        }

        [MenuItem("Tools/Burning/Validate C15 Playable Scene")]
        public static void ValidateScene()
        {
            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Transform[] transforms = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .ToArray();
            int missingScripts = transforms
                .Sum(item => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(item.gameObject));
            string[] requiredObjects = { "Black", "Cut01", "Cut02", "Cut03" };
            bool hasAllImages = requiredObjects.All(name => transforms.Any(item => item.name == name));

            if (missingScripts > 0 || !hasAllImages)
            {
                throw new System.InvalidOperationException(
                    $"C15 씬 검사 실패: missingScripts={missingScripts}, images={hasAllImages}");
            }

            Debug.Log("C15 씬 검사 완료: 파싱 정상, 이미지 연결 정상, 누락 스크립트 0개");
        }

        private static CanvasGroup CreateImage(string name, Transform parent, Sprite sprite, bool addGroup)
        {
            GameObject imageObject = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform rect = imageObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image image = imageObject.GetComponent<Image>();
            image.sprite = sprite;
            image.color = Color.white;
            image.preserveAspect = false;
            image.raycastTarget = false;

            if (!addGroup)
            {
                return null;
            }

            CanvasGroup group = imageObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;
            return group;
        }

        private static Canvas CreateCanvas()
        {
            GameObject canvasObject = new(
                "Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(540f, 960f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        private static void CreateCamera()
        {
            GameObject cameraObject = new("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.orthographic = true;
        }

        private static void PrepareSprites()
        {
            foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { ArtPath.TrimEnd('/') }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (AssetImporter.GetAtPath(path) is not TextureImporter importer)
                {
                    continue;
                }

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.maxTextureSize = 2048;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
        }

        private static Sprite LoadSprite(string fileName)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(ArtPath + fileName);
        }

        private static void AddSceneToBuildSettings()
        {
            List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes
                .Where(item => item.path != ScenePath)
                .ToList();
            scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
        }
    }
}
