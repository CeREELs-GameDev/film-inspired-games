using System.Collections.Generic;
using System.Linq;
using FilmInspiredGames.Burning.C06;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FilmInspiredGames.Burning.C06.Editor
{
    public static class C06C07PlayableSceneBuilder
    {
        private const string ScenePath = "Assets/Game/Burning/C06/Scenes/Burning_C06_C07_Playable.unity";
        private const string NextScenePath = "Assets/Game/Burning/C08/Scenes/Burning_C08_C12_Playable.unity";
        private const string C06ArtPath = "Assets/Game/Burning/C06/Art/";
        private const string C07ArtPath = "Assets/Game/Burning/C07/Art/";
        private const float C06SourceWidth = 1632f;
        private const float C06SourceHeight = 2912f;
        private const float C07SourceWidth = 816f;
        private const float C07SourceHeight = 1456f;

        private static readonly SourceLayout[] PuzzleLayouts =
        {
            new(0, 1787, 384, 516), new(0, 2286, 372, 316),
            new(219, 2502, 664, 412), new(746, 2546, 460, 368),
            new(438, 2234, 420, 344), new(731, 2154, 384, 508),
            new(363, 1897, 412, 400), new(641, 1879, 488, 392),
            new(251, 1524, 504, 508), new(622, 1564, 400, 452),
            new(386, 1190, 588, 484), new(561, 864, 428, 448),
            new(387, 920, 292, 452), new(863, 855, 396, 576),
            new(921, 584, 328, 396), new(620, 528, 436, 452),
            new(406, 527, 336, 504), new(502, 372, 184, 280),
            new(635, 340, 304, 312), new(832, 361, 364, 356)
        };

        [MenuItem("Tools/Burning/Build C06-C07 Playable Scene")]
        public static void BuildAndOpen()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("재생을 멈춘 뒤 C06-C07 씬을 다시 생성하세요.");
                return;
            }

            PrepareSprites(C06ArtPath, C07ArtPath);
            EnsureFolder("Assets/Game/Burning/C06/Scenes");

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Burning_C06_C07_Playable";
            CreateCamera();
            Canvas canvas = CreateCanvas();
            RectTransform chapters = CreateRoot("Chapters", canvas.transform);

            RectTransform c06Root = CreateRoot("C06", chapters);
            CanvasGroup c06RootGroup = c06Root.gameObject.AddComponent<CanvasGroup>();
            CanvasGroup idle = CreateFrame("Idle", c06Root, C06ArtPath + "C06_Idle.png");
            CanvasGroup speaking = CreateFrame("Speaking", c06Root, C06ArtPath + "C06_Speaking.png");
            CanvasGroup speechBubble = CreateFrame("SpeechBubble", c06Root, C06ArtPath + "C06_SpeechBubble.png");
            CanvasGroup puzzleBase = CreateFrame("PuzzleBase", c06Root, C06ArtPath + "C06_PuzzleBase.png");
            RectTransform puzzleRoot = CreateRoot("PuzzlePieces", c06Root);
            CanvasGroup puzzleRootGroup = puzzleRoot.gameObject.AddComponent<CanvasGroup>();
            RectTransform[] pieces = CreatePuzzlePieces(puzzleRoot);

            RectTransform c07Root = CreateRoot("C07", chapters);
            CanvasGroup c07RootGroup = c07Root.gameObject.AddComponent<CanvasGroup>();
            CanvasGroup unknown = CreateFrame("Unknown", c07Root, C07ArtPath + "C07_Unknown.png");
            CanvasGroup realizes = CreateFrame("Realizes", c07Root, C07ArtPath + "C07_Realizes.png");
            Image exclamationImage = CreateSourceAlignedImage(
                "Exclamation",
                c07Root,
                LoadSprite(C07ArtPath + "C07_Exclamation.png"),
                new SourceLayout(590, 224, 72, 204),
                C07SourceWidth,
                C07SourceHeight);
            CanvasGroup exclamation = exclamationImage.gameObject.AddComponent<CanvasGroup>();

            Image black = CreateFullScreenImage("BlackDissolve", canvas.transform, null);
            black.color = Color.black;
            CanvasGroup blackGroup = black.gameObject.AddComponent<CanvasGroup>();

            GameObject systems = new("Systems");
            GameObject sequenceObject = new("C06C07Sequence");
            sequenceObject.transform.SetParent(systems.transform);
            C06C07SequenceController controller = sequenceObject.AddComponent<C06C07SequenceController>();
            ConfigureController(
                controller, c06RootGroup, idle, speaking, speechBubble, puzzleBase, puzzleRootGroup,
                pieces, c07RootGroup, unknown, realizes, exclamation, exclamationImage.rectTransform, blackGroup);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AddSceneToBuildSettings();
            Selection.activeGameObject = sequenceObject;
            Debug.Log("C06-C07 플레이 씬 생성 완료. 흩어진 퍼즐 20개를 모두 맞추면 C07로 이동합니다.");
        }

        private static RectTransform[] CreatePuzzlePieces(Transform parent)
        {
            RectTransform[] pieces = new RectTransform[PuzzleLayouts.Length];
            for (int i = 0; i < PuzzleLayouts.Length; i++)
            {
                string fileName = $"C06_Puzzle{i + 1:00}.png";
                Image image = CreateSourceAlignedImage(
                    $"Puzzle_{i + 1:00}", parent,
                    LoadSprite(C06ArtPath + "Puzzles/" + fileName),
                    PuzzleLayouts[i], C06SourceWidth, C06SourceHeight);
                pieces[i] = image.rectTransform;
            }

            return pieces;
        }

        private static void ConfigureController(
            C06C07SequenceController controller,
            CanvasGroup c06Root,
            CanvasGroup idle,
            CanvasGroup speaking,
            CanvasGroup speechBubble,
            CanvasGroup puzzleBase,
            CanvasGroup puzzleRoot,
            RectTransform[] pieces,
            CanvasGroup c07Root,
            CanvasGroup unknown,
            CanvasGroup realizes,
            CanvasGroup exclamation,
            RectTransform exclamationRect,
            CanvasGroup black)
        {
            SerializedObject serialized = new(controller);
            Set(serialized, "c06Root", c06Root);
            Set(serialized, "c06Idle", idle);
            Set(serialized, "c06Speaking", speaking);
            Set(serialized, "c06SpeechBubble", speechBubble);
            Set(serialized, "c06PuzzleBase", puzzleBase);
            Set(serialized, "c06PuzzleRoot", puzzleRoot);
            SetArray(serialized.FindProperty("puzzlePieces"), pieces);
            Set(serialized, "c07Root", c07Root);
            Set(serialized, "c07Unknown", unknown);
            Set(serialized, "c07Realizes", realizes);
            Set(serialized, "c07Exclamation", exclamation);
            Set(serialized, "c07ExclamationRect", exclamationRect);
            Set(serialized, "blackOverlay", black);
            serialized.FindProperty("nextSceneName").stringValue = "Burning_C08_C12_Playable";
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetArray(SerializedProperty property, RectTransform[] values)
        {
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
        }

        private static void Set(SerializedObject serialized, string property, Object value)
        {
            serialized.FindProperty(property).objectReferenceValue = value;
        }

        private static CanvasGroup CreateFrame(string name, Transform parent, string path)
        {
            return CreateFullScreenImage(name, parent, LoadSprite(path)).gameObject.AddComponent<CanvasGroup>();
        }

        private static Image CreateSourceAlignedImage(
            string name,
            Transform parent,
            Sprite sprite,
            SourceLayout layout,
            float sourceWidth,
            float sourceHeight)
        {
            GameObject imageObject = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform rect = imageObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);

            float scaleX = 540f / sourceWidth;
            float scaleY = 960f / sourceHeight;
            rect.sizeDelta = new Vector2(layout.Width * scaleX, layout.Height * scaleY);
            rect.anchoredPosition = new Vector2(
                (layout.X + layout.Width * 0.5f) * scaleX,
                -(layout.Y + layout.Height * 0.5f) * scaleY);

            Image image = imageObject.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = false;
            image.raycastTarget = false;
            return image;
        }

        private static Image CreateFullScreenImage(string name, Transform parent, Sprite sprite)
        {
            GameObject imageObject = new(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            RectTransform rect = imageObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            Stretch(rect);
            Image image = imageObject.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = false;
            image.raycastTarget = false;
            return image;
        }

        private static RectTransform CreateRoot(string name, Transform parent)
        {
            GameObject root = new(name, typeof(RectTransform));
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            Stretch(rect);
            return rect;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void CreateCamera()
        {
            GameObject cameraObject = new("Main Camera", typeof(Camera));
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.orthographic = true;
            camera.transform.position = new Vector3(0f, 0f, -10f);
        }

        private static Canvas CreateCanvas()
        {
            GameObject canvasObject = new("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(540f, 960f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }

        private static void PrepareSprites(params string[] artPaths)
        {
            foreach (string artPath in artPaths)
            {
                string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { artPath.TrimEnd('/') });
                foreach (string guid in guids)
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
                    importer.maxTextureSize = path.Contains("/Puzzles/") ? 1024 : 4096;
                    importer.SaveAndReimport();
                }
            }
        }

        private static void EnsureFolder(string path)
        {
            string current = "Assets";
            foreach (string part in path.Substring("Assets/".Length).Split('/'))
            {
                string next = current + "/" + part;
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, part);
                }
                current = next;
            }
        }

        private static Sprite LoadSprite(string path)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void AddSceneToBuildSettings()
        {
            List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes
                .Where(item => item.path != ScenePath && item.path != NextScenePath)
                .ToList();
            scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            scenes.Add(new EditorBuildSettingsScene(NextScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
        }

        private readonly struct SourceLayout
        {
            public SourceLayout(float x, float y, float width, float height)
            {
                X = x;
                Y = y;
                Width = width;
                Height = height;
            }

            public float X { get; }
            public float Y { get; }
            public float Width { get; }
            public float Height { get; }
        }
    }
}
