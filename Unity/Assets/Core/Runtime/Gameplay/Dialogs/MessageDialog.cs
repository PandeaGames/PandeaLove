using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MessageDialog : Dialog
{
    public class MessageDialogConfig : Config
    {
        public delegate void OptionDelegate(Option selected);

        public event OptionDelegate OnOptionSelected;

        private List<Option> _options;

        public List<Option> options { get { return _options; } }

        public MessageDialogConfig(List<Option> options)
        {
            _options = options;
        }
    }

    public struct Option
    {
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

    public override void Setup(Config config)
    {
        _config = config as MessageDialogConfig;

        base.Setup(config);
    }
}
