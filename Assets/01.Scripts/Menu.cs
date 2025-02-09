using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public class Menu
{
    [MenuItem("Custom/Load Lobby")]
    private static void LoadLobbyScene()
    {
        // First, save current scene if needed
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            // Load the Lobby scene
            EditorSceneManager.OpenScene("Assets/00.Scenes/Lobby.unity");

            // Enter play mode
            EditorApplication.isPlaying = true;
        }
    }
}