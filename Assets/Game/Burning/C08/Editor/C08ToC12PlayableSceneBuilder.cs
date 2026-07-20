using FilmInspiredGames.Burning.C08;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FilmInspiredGames.Burning.C08.Editor
{
    public static class C08ToC12PlayableSceneBuilder
    {
        private const string ScenePath = "Assets/Game/Burning/C08/Scenes/Burning_C08_C12_Playable.unity";
        private const string C08ArtPath = "Assets/Game/Burning/C08/Art/";
        private const string C09ArtPath = "Assets/Game/Burning/C09/Art/";
        private const string C10ArtPath = "Assets/Game/Burning/C10/Art/";
        private const string C11ArtPath = "Assets/Game/Burning/C11/Art/";
        private const string C12ArtPath = "Assets/Game/Burning/C12/Art/";

        [MenuItem("Tools/Burning/Build C08-C12 Playable Scene")]
        public static void BuildAndOpen()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("재생을 멈춘 뒤 C08-C12 씬을 다시 생성하세요.");
                return;
            }

            PrepareSprites(C08ArtPath, C09ArtPath, C10ArtPath, C11ArtPath, C12ArtPath);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Burning_C08_C12_Playable";

            CreateCamera();
            Canvas canvas = CreateCanvas();
            RectTransform chapterRoot = CreateRoot("Chapters", canvas.transform);

            ChapterObjects chapters = CreateChapters(chapterRoot);
            Image blackOverlay = CreateSolidImage("BlackDissolve", canvas.transform, Color.black);
            CanvasGroup blackOverlayGroup = blackOverlay.gameObject.AddComponent<CanvasGroup>();

            GameObject systems = new("Systems");
            GameObject controllerObject = new("C08ToC12Sequence");
            controllerObject.transform.SetParent(systems.transform);
            C08ToC12SequenceController controller = controllerObject.AddComponent<C08ToC12SequenceController>();
            ConfigureController(controller, chapters, blackOverlayGroup);

            EditorSceneManager.SaveScene(scene, ScenePath);
            Selection.activeGameObject = controllerObject;
            Debug.Log("C08-C12 플레이 씬 생성 완료. Play 버튼을 누르면 C08부터 자동 재생됩니다.");
        }

        private static ChapterObjects CreateChapters(Transform parent)
        {
            ChapterObjects chapters = new();

            RectTransform c08Root = CreateRoot("C08", parent);
            chapters.C08Root = c08Root.gameObject.AddComponent<CanvasGroup>();
            CreateFullScreenImage("BackgroundDark", c08Root, LoadSprite(C08ArtPath, "C08_BG1.png"));
            Image c08Bright = CreateFullScreenImage("BackgroundBright", c08Root, LoadSprite(C08ArtPath, "C08_BG2.png"));
            CreateFullScreenImage("JongsuAndHaemi", c08Root, LoadSprite(C08ArtPath, "C08_Characters.png"));
            chapters.C08Bright = c08Bright.gameObject.AddComponent<CanvasGroup>();

            RectTransform c09Root = CreateRoot("C09", parent);
            chapters.C09Root = c09Root.gameObject.AddComponent<CanvasGroup>();
            Image haemiDark = CreateFullScreenImage("HaemiBeforeLight", c09Root, LoadSprite(C09ArtPath, "C09_Haemi1.png"));
            Image haemiLit = CreateFullScreenImage("HaemiAfterLight", c09Root, LoadSprite(C09ArtPath, "C09_Haemi2.png"));
            chapters.Jongsu = CreateFullScreenImage("Jongsu", c09Root, LoadSprite(C09ArtPath, "C09_Jongsu.png"));
            chapters.HaemiDark = haemiDark.gameObject.AddComponent<CanvasGroup>();
            chapters.HaemiLit = haemiLit.gameObject.AddComponent<CanvasGroup>();

            RectTransform c10Root = CreateRoot("C10", parent);
            chapters.C10Root = c10Root.gameObject.AddComponent<CanvasGroup>();
            chapters.C10Neutral = CreateFrame("Neutral", c10Root, C10ArtPath, "C10_1.png");
            chapters.C10Blink = CreateFrame("Blink", c10Root, C10ArtPath, "C10_2.png");
            chapters.C10Glance = CreateFrame("Glance", c10Root, C10ArtPath, "C10_3.png");
            chapters.C10HaemiOnly = CreateFrame("HaemiOnly", c10Root, C10ArtPath, "C10_4.png");

            RectTransform c11Root = CreateRoot("C11", parent);
            chapters.C11Root = c11Root.gameObject.AddComponent<CanvasGroup>();
            CreateFullScreenImage("Background", c11Root, LoadSprite(C11ArtPath, "C11_Background.png"));
            Image c11Watch = CreateFullScreenImage("JongsuPresentsWatch", c11Root, LoadSprite(C11ArtPath, "C11_JongsuWatch.png"));
            chapters.C11Watch = c11Watch.gameObject.AddComponent<CanvasGroup>();
            chapters.C11WatchRect = c11Watch.rectTransform;

            RectTransform c12Root = CreateRoot("C12", parent);
            chapters.C12Root = c12Root.gameObject.AddComponent<CanvasGroup>();
            chapters.C12Smile = CreateFrame("Smile", c12Root, C12ArtPath, "C12_1.png");
            chapters.C12Laugh = CreateFrame("Laugh", c12Root, C12ArtPath, "C12_2.png");

            return chapters;
        }

        private static CanvasGroup CreateFrame(string name, Transform parent, string artPath, string fileName)
        {
            Image image = CreateFullScreenImage(name, parent, LoadSprite(artPath, fileName));
            return image.gameObject.AddComponent<CanvasGroup>();
        }

        private static void ConfigureController(
            C08ToC12SequenceController controller,
            ChapterObjects chapters,
            CanvasGroup blackOverlay)
        {
            SerializedObject serialized = new(controller);
            Set(serialized, "c08Root", chapters.C08Root);
            Set(serialized, "c08BrightBackground", chapters.C08Bright);
            Set(serialized, "c09Root", chapters.C09Root);
            Set(serialized, "haemiDark", chapters.HaemiDark);
            Set(serialized, "haemiLit", chapters.HaemiLit);
            Set(serialized, "jongsuRect", chapters.Jongsu.rectTransform);
            Set(serialized, "c10Root", chapters.C10Root);
            Set(serialized, "c10Neutral", chapters.C10Neutral);
            Set(serialized, "c10Blink", chapters.C10Blink);
            Set(serialized, "c10Glance", chapters.C10Glance);
            Set(serialized, "c10HaemiOnly", chapters.C10HaemiOnly);
            Set(serialized, "c11Root", chapters.C11Root);
            Set(serialized, "c11Watch", chapters.C11Watch);
            Set(serialized, "c11WatchRect", chapters.C11WatchRect);
            Set(serialized, "c12Root", chapters.C12Root);
            Set(serialized, "c12Smile", chapters.C12Smile);
            Set(serialized, "c12Laugh", chapters.C12Laugh);
            Set(serialized, "blackOverlay", blackOverlay);
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Set(SerializedObject serialized, string property, Object value)
        {
            serialized.FindProperty(property).objectReferenceValue = value;
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

        private static Image CreateSolidImage(string name, Transform parent, Color color)
        {
            Image image = CreateFullScreenImage(name, parent, null);
            image.color = color;
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

        private static void PrepareSprites(params string[] artPaths)
        {
            foreach (string artPath in artPaths)
            {
                string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { artPath.TrimEnd('/') });

                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

                    if (importer == null)
                    {
                        continue;
                    }

                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importer.alphaIsTransparency = true;
                    importer.mipmapEnabled = false;
                    importer.maxTextureSize = 4096;
                    importer.SaveAndReimport();
                }
            }
        }

        private static Sprite LoadSprite(string artPath, string fileName)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(artPath + fileName);
        }

        private sealed class ChapterObjects
        {
            public CanvasGroup C08Root;
            public CanvasGroup C08Bright;
            public CanvasGroup C09Root;
            public CanvasGroup HaemiDark;
            public CanvasGroup HaemiLit;
            public Image Jongsu;
            public CanvasGroup C10Root;
            public CanvasGroup C10Neutral;
            public CanvasGroup C10Blink;
            public CanvasGroup C10Glance;
            public CanvasGroup C10HaemiOnly;
            public CanvasGroup C11Root;
            public CanvasGroup C11Watch;
            public RectTransform C11WatchRect;
            public CanvasGroup C12Root;
            public CanvasGroup C12Smile;
            public CanvasGroup C12Laugh;
        }
    }
}
