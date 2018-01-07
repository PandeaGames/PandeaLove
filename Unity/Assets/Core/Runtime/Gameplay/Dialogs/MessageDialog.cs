using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class MessageDialog : Dialog
{
    public class MessageDialogResponse : Response
    {
        private Option _choice;
        public Option Choice { get { return _choice; } }

        public MessageDialogResponse(Option choice)
        {
            _choice = choice;
        }
    }

    [CreateAssetMenu(fileName = "MessageDialogConfig", menuName = "Config/Dialogs/MessageDialogConfig", order = 1)]
    [Serializable]
    public class MessageDialogConfig : Config
    {
        public delegate void OptionDelegate(Option selected);

        public event OptionDelegate OnOptionSelected;

        [SerializeField]
        private List<Option> _options;

        public List<Option> options { get { return _options; } }

        public MessageDialogConfig(List<Option> options)
        {
            _options = options;
        }
    }

    [Serializable]
    public struct Option
    {
        [SerializeField]
        private string _title;

        public string Title { get { return _title; } }

        public Option(string title)
        {
            _title = title;
        }
    }

    [SerializeField]
    private GameObject _button;
    [SerializeField]
    private GameObject _buttonContainer;

    private MessageDialogConfig _config;

    public override void Setup(Config config, DialogResponseDelegate responseDelegate = null)
    {
        _config = config as MessageDialogConfig;
        base.Setup(config, responseDelegate);
    }
}
