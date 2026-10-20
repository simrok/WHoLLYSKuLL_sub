// 발표용 임시 녹화 도구. _TempBuildFlow 폴더째 지우면 같이 사라짐.
// 메뉴: Tools > Temp > Record Gameplay → 타이틀 씬부터 Play + mp4 녹화, Play 정지하면 저장.
using System.IO;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Encoder;
using UnityEditor.Recorder.Input;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class TempPlayRecorder
{
    const string Flag = "TempPlayRecorder.Armed";
    static RecorderController controller;

    static TempPlayRecorder()
    {
        EditorApplication.playModeStateChanged += OnPlayMode;
    }

    [MenuItem("Tools/Temp/Record Gameplay")]
    static void Record()
    {
        if (EditorApplication.isPlaying) return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        EditorSceneManager.OpenScene("Assets/_TempBuildFlow/_TempTitle.unity");
        SessionState.SetBool(Flag, true);
        EditorApplication.isPlaying = true;
    }

    static void OnPlayMode(PlayModeStateChange s)
    {
        if (s == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Flag, false))
        {
            SessionState.SetBool(Flag, false);
            Start();
        }
        else if (s == PlayModeStateChange.ExitingPlayMode && controller != null)
        {
            controller.StopRecording();
            controller = null;
            Debug.Log("[TempPlayRecorder] 녹화 저장 완료: " + Path.GetFullPath("Recordings"));
            EditorUtility.RevealInFinder(Path.GetFullPath("Recordings"));
        }
    }

    static void Start()
    {
        var settings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
        var movie = ScriptableObject.CreateInstance<MovieRecorderSettings>();
        movie.name = "Gameplay";
        movie.Enabled = true;
        movie.EncoderSettings = new CoreEncoderSettings
        {
            Codec = CoreEncoderSettings.OutputCodec.MP4,
            EncodingQuality = CoreEncoderSettings.VideoEncodingQuality.High
        };
        movie.ImageInputSettings = new GameViewInputSettings { OutputWidth = 1920, OutputHeight = 1080 };
        movie.AudioInputSettings.PreserveAudio = true;
        movie.OutputFile = "Recordings/WhollySkull_" + System.DateTime.Now.ToString("HHmmss");

        settings.AddRecorderSettings(movie);
        settings.SetRecordModeToManual();
        settings.FrameRate = 30;

        controller = new RecorderController(settings);
        controller.PrepareRecording();
        controller.StartRecording();
        Debug.Log("[TempPlayRecorder] 녹화 시작");
    }
}
