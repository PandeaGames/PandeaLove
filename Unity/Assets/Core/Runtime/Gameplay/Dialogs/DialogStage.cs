using UnityEngine;
using System.Collections;

public class DialogStage : MonoBehaviour
{
    [SerializeField]
    private Transform _stage;

    public void ShowDialog(GameObject prefab, Dialog.Config config, Dialog.DialogResponseDelegate response = null)
    {
        GameObject prefabInstance = Instantiate(prefab, _stage);

        Dialog dialog = prefabInstance.GetComponent<Dialog>();

        if (!dialog)
        {
            Debug.LogError("Dialog component not found on prefab. Cannot display: " + prefab);
            return;
        }

        dialog.OnCancel += OnDialogCancel;
        dialog.OnClose += OnDialogClose;

        dialog.Setup(config, response);
    }

    private void BlurDialog(Dialog dialog)
    {
        dialog.Blur();
    }

    private void OnDialogClose(Dialog dialog)
    {
        BlurDialog(dialog); 
    }

    private void OnDialogCancel(Dialog dialog)
    {

    }
}
