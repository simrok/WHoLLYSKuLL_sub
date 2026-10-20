// 트레일러 녹화용 임시 메뉴. _TempBuildFlow 폴더째 지우면 같이 사라짐.
// Tools > Temp > Record Trailer : 마을 씬부터 연출을 재생하며 mp4로 녹화, 끝나면 자동 정지
// Tools > Temp > Preview Trailer : 녹화 없이 연출만 재생
using System.IO;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Encoder;
using UnityEditor.Recorder.Input;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class TempTrailerRecorder
{
    const string Mode = "TempTrailerRecorder.Mode"; // 0 없음, 1 미리보기, 2 녹화
    static RecorderController controller;

    static TempTrailerRecorder() { EditorApplication.playModeStateChanged += OnPlayMode; }

    [MenuItem("Tools/Temp/Record Trailer")] static void Record() => Begin(2);
    [MenuItem("Tools/Temp/Preview Trailer")] static void Preview() => Begin(1);

    static void Begin(int mode)
    {
        if (EditorApplication.isPlaying) return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EditorSceneManager.OpenScene("Assets/Scenes/Village.unity");
        SessionState.SetInt(Mode, mode);
        EditorApplication.isPlaying = true;
    }

    static void OnPlayMode(PlayModeStateChange s)
    {
        if (s == PlayModeStateChange.EnteredPlayMode)
        {
            int mode = SessionState.GetInt(Mode, 0);
            if (mode == 0) return;
            SessionState.SetInt(Mode, 0);
            var go = new GameObject("_TempTrailerDirector");
            var d = go.AddComponent<TempTrailerDirector>();
            d.logo = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/_TempBuildFlow/Title/TitleLogo.png");
            d.bodyFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/_TempBuildFlow/Trailer/Fonts/Cinzel.ttf");
            d.titleFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/_TempBuildFlow/Trailer/Fonts/CinzelDecorative-Bold.ttf");
            TempTrailerDirector.OnFinished = () => EditorApplication.isPlaying = false;
            if (mode == 2) StartRecording();
        }
        else if (s == PlayModeStateChange.ExitingPlayMode)
        {
            TempTrailerDirector.OnFinished = null;
            if (controller == null) return;
            controller.StopRecording();
            controller = null;
            Debug.Log("[TempTrailerRecorder] 트레일러 저장 완료: " + Path.GetFullPath("Recordings"));
            EditorUtility.RevealInFinder(Path.GetFullPath("Recordings"));
        }
    }

    static void StartRecording()
    {
        var settings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
        var movie = ScriptableObject.CreateInstance<MovieRecorderSettings>();
        movie.name = "Trailer";
        movie.Enabled = true;
        movie.EncoderSettings = new CoreEncoderSettings
        {
            Codec = CoreEncoderSettings.OutputCodec.MP4,
            EncodingQuality = CoreEncoderSettings.VideoEncodingQuality.High
        };
        movie.ImageInputSettings = new GameViewInputSettings { OutputWidth = 1920, OutputHeight = 1080 };
        movie.AudioInputSettings.PreserveAudio = false;
        movie.OutputFile = "Recordings/WhollySkull_Trailer_" + System.DateTime.Now.ToString("HHmmss");
        settings.AddRecorderSettings(movie);
        settings.SetRecordModeToManual();
        settings.FrameRate = 30;
        settings.CapFrameRate = true;
        controller = new RecorderController(settings);
        controller.PrepareRecording();
        controller.StartRecording();
        Debug.Log("[TempTrailerRecorder] 녹화 시작");
    }
}
