using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class DialogService : Service
{
    [SerializeField]
    private DialogConfig _config;

    [SerializeField]
    private DialogStage _dialogStage;

    private Dictionary<Type, GameObject> _dialogLookup;

    public override void StartService(ServiceManager serviceManager)
    {
        _dialogLookup = new Dictionary<Type, GameObject>();

        foreach (GameObject dialogPrefab in _config)
        {
            Dialog dialogComponent = dialogPrefab.GetComponent<Dialog>();
            MessageDialog messageDialogComponent = dialogPrefab.GetComponent<MessageDialog>();

            if (dialogComponent == null)
            {
                Debug.LogError("Missing Dialog component found during service setup");
                continue;
            }

            //if we find a dialog of type MessageDialog, insert it into the dictionary as our generic MessageDialog.
            if(messageDialogComponent)
                _dialogLookup.Add(typeof(MessageDialog), dialogPrefab);

            _dialogLookup.Add(dialogComponent.GetType(), dialogPrefab);
        }

        base.StartService(serviceManager);
    }

    public override void EndService(ServiceManager serviceManager)
    {
        _dialogLookup.Clear();
        _dialogLookup = null;

        base.EndService(serviceManager);
    }

    public void DisplayDialog<T>(Dialog.Config config, Dialog.DialogResponseDelegate responseDelegate = null) where T:Dialog
    {
        GameObject prefab = GetDialogPrefab<T>();

        if (prefab == null)
        {
            Debug.LogError("Cannot display dialog: " + typeof(T).Name);
            return;
        }

        Debug.LogError("Display Dialog type " + typeof(T).Name+" with config "+config);
        _dialogStage.ShowDialog(prefab, config, responseDelegate);
    }

    public GameObject GetDialogPrefab<T>() where T : Dialog
    {
        GameObject prefab;
        _dialogLookup.TryGetValue(typeof(T), out prefab);

        if (prefab == null)
        {
            Debug.LogError("Dialog type not found in service. Please add it to your configuration.");
            return null;
        }

        return prefab;
    }

    public T GetDialogComponent<T>() where T : Dialog
    {
        GameObject prefab = GetDialogPrefab<T>();

        if (prefab == null)
            return default(T);

        return prefab.GetComponent<T>();
    }
}