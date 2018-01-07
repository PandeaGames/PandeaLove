using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public enum Direction
{
    TO,
    FROM
}

[Serializable]
public class ScreenTransition
{
    [SerializeField]
    private string _sceneId;
    [SerializeField]
    private Direction _direction;
    [SerializeField]
    private ScreenController.Config _screenConfig;

    public string SceneId { get { return _sceneId; } }
    public Direction Direction { get { return _direction; } }
    public ScreenController.Config ScreenConfig { get { return _screenConfig; } }

    public ScreenTransition(string sceneId, ScreenController.Config screenConfig, Direction direction)
    {
        _direction = direction;
        _screenConfig = screenConfig;
        _sceneId = sceneId;
    }

    public ScreenTransition(string sceneId, ScreenController.Config screenConfig) : this(sceneId, screenConfig, Direction.TO)
    {

    }
}


public class WindowController : MonoBehaviour
{
    [SerializeField]
    private bool _overlapTransitions;

    private ScreenController _activeScreen;

    public virtual void LaunchScreen(ScreenTransition transition)
    {
        StartCoroutine(LoadSceneAsync(transition));
    }

    public virtual void LaunchScreen(string sceneId, ScreenController.Config screenConfig)
    {
        LaunchScreen(new ScreenTransition(sceneId, screenConfig));
    }

    public void RemoveScreen()
    {
        
    }

    private IEnumerator LoadSceneAsync(ScreenTransition transition)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(transition.SceneId, LoadSceneMode.Additive);
        
        //Wait until the last operation fully loads to return anything
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        Scene scene = SceneManager.GetSceneByName(transition.SceneId);
        ActivateScene(scene, transition);
    }

    private void ActivateScene(Scene scene, ScreenTransition transition)
    {
        ScreenController controller = null;

        foreach( GameObject gameObj in scene.GetRootGameObjects())
        {
            controller = gameObj.GetComponent<ScreenController>();

            if (controller != null)
                break;
        }

        if(controller == null)
        {
            Debug.LogError("ScreenController not found in scene '"+transition.SceneId+"'");
            return;
        }

        if (_activeScreen != null)
        {
            _activeScreen.Transition(transition);
            _activeScreen.OnTransitionComplete += TransitionComplete;
        }

        _activeScreen = controller;

        controller.Transition(transition);
        controller.transform.SetParent(transform, true);
        controller.Setup(this, transition.ScreenConfig);

        RectTransform rt = controller.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        StartCoroutine(UnloadScene(scene));
    }

    public void TransitionScreen(ScreenTransition transition)
    {
        
    }

    private void TransitionComplete(ScreenController controller)
    {
        controller.OnTransitionComplete -= TransitionComplete;
        Destroy(controller.gameObject);
    }

    private IEnumerator UnloadScene(Scene scene)
    {
        AsyncOperation asyncLoad = SceneManager.UnloadSceneAsync(scene);

        //Wait until the last operation fully loads to return anything
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        //done unload of scene
    }
}
