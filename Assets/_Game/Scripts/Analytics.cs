using Firebase.Analytics;
using FruitSwipeMatch3Kit;
using UnityEngine;

public class Analytics : MonoBehaviour
{
    private static Analytics _instance;
    public static Analytics Instance => _instance;

    private const string Win = "Win_{0}";
    private const string Lose = "Lose_{0}";
    private const string Restart = "Restart_{0}";
    private const string Quit = "Quit_{0}";
    private const string Continue = "Continue_{0}_{1}";

    private void Awake()
    {
        if (_instance != null) Destroy(gameObject);
        else _instance = this;
    }

    void Start()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available) {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                //   app = Firebase.FirebaseApp.DefaultInstance;
                FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventLogin);
                // Set a flag here to indicate whether Firebase is ready to use by your app.
            } else {
                UnityEngine.Debug.LogError(System.String.Format(
                    "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });
        DontDestroyOnLoad(gameObject);
    }

    public void CompleteLevel()
    {
        int level = PlayerPrefs.GetInt(GameplayConstants.NextLevelPrefKey);
        FirebaseAnalytics.LogEvent(string.Format(Win, level.ToString("000")));
    }

    public void LoseLevel()
    {
        int level = PlayerPrefs.GetInt(GameplayConstants.NextLevelPrefKey);
        FirebaseAnalytics.LogEvent(string.Format(Lose, level.ToString("000")));
    }

    public void RestartLevel()
    {
        int level = PlayerPrefs.GetInt(GameplayConstants.NextLevelPrefKey);
        FirebaseAnalytics.LogEvent(string.Format(Restart, level.ToString("000")));
    }

    public void QuitLevel()
    {
        int level = PlayerPrefs.GetInt(GameplayConstants.NextLevelPrefKey);
        FirebaseAnalytics.LogEvent(string.Format(Quit, level.ToString("000")));
    }

    public void ContinueLevel(int count)
    {
        int level = PlayerPrefs.GetInt(GameplayConstants.NextLevelPrefKey);
        FirebaseAnalytics.LogEvent(string.Format(Continue, count, level.ToString("000")));
    }
}
