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
    private class LoadedScreenScene : MonoBehaviour
    {
        private ScreenController _controller;
        private Scene _scene;

        public ScreenController Controller { set { _controller = value; } }
        public Scene Scene { set { _scene = value; } }

        public LoadedScreenScene(ScreenController controller, Scene scene)
        {
            _controller = controller;
            _scene = scene;
        }

        public void TransitionScreen(ScreenTransition transition)
        {
            _controller.Transition(transition);
            _controller.OnTransitionComplete += TransitionComplete;
        }

        private void TransitionComplete(ScreenController controller)
        {
            controller.OnTransitionComplete -= TransitionComplete;
            StartCoroutine(UnloadScene());
        }

        private IEnumerator UnloadScene()
        {
            AsyncOperation asyncLoad = SceneManager.UnloadSceneAsync(_scene);

            //Wait until the last operation fully loads to return anything
            while (!asyncLoad.isDone)
            {
                yield return null;
            }

            CleanupAfterUnload();
        }

        private void CleanupAfterUnload()
        {
            Destroy(_controller.gameObject);

            _scene = default(Scene);
            _controller = null;

            Destroy(this);
        }
    }

    [SerializeField]
    private bool _overlapTransitions;

    private LoadedScreenScene _activeScene;

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

        if (_activeScene != null)
            _activeScene.TransitionScreen(transition);

        _activeScene = gameObject.AddComponent<LoadedScreenScene>();
        _activeScene.Controller = controller;
        _activeScene.Scene = scene;

        controller.Transition(transition);
        controller.transform.SetParent(transform);
    }
}
