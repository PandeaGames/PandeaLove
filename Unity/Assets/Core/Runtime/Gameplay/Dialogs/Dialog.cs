﻿using UnityEngine;
using System.Collections;
using System;

public abstract class Dialog : MonoBehaviour
{
    public class Response
    {
        
    }

    [Serializable]
    public class Config : ScriptableObject
    {
        public Config()
        {
        }
    }

    public delegate void DialogResponseDelegate(Response response);
    public delegate void DialogDelegate(Dialog dialog);

    public event DialogDelegate OnFocus;
    public event DialogDelegate OnBlur;
    public event DialogDelegate OnShow;
    public event DialogDelegate OnHide;
    public event DialogDelegate OnClose;
    public event DialogDelegate OnCancel;

    [SerializeField]
    public GameObject _closeButton;

    private DialogResponseDelegate _responseDelegate;
    private Config _config;

    public virtual void Setup(Config config, DialogResponseDelegate responseDelegate = null) {
        _config = config;
        _responseDelegate = responseDelegate;
    }

    public void OnDestroy()
    {
        if (OnFocus != null)
        {
            foreach (Delegate d in OnFocus.GetInvocationList())
                OnFocus -= (DialogDelegate)d;
            OnFocus = null;
        }

        if (OnBlur != null)
        {
            foreach (Delegate d in OnBlur.GetInvocationList())
                OnBlur -= (DialogDelegate)d;
            OnBlur = null;
        }

        if (OnShow != null)
        {
            foreach (Delegate d in OnShow.GetInvocationList())
                OnShow -= (DialogDelegate)d;
            OnShow = null;
        }

        if (OnHide != null)
        {
            foreach (Delegate d in OnHide.GetInvocationList())
                OnHide -= (DialogDelegate)d;
            OnHide = null;
        }

        if (OnClose != null)
        {
            foreach (Delegate d in OnClose.GetInvocationList())
                OnClose -= (DialogDelegate)d;
            OnClose = null;
        }

        if (OnCancel != null)
        {
            foreach (Delegate d in OnCancel.GetInvocationList())
                OnCancel -= (DialogDelegate)d;
            OnCancel = null;
        }
    }

    public void Focus()
    {
        if (OnFocus != null)
            OnFocus(this);
    }

    public void Blur()
    {
        if (OnBlur != null)
            OnBlur(this);
    }

    public void Show()
    {
        if (OnShow != null)
            OnShow(this);
    }

    public void Hide()
    {
        if (OnHide != null)
            OnHide(this);
    }

    public void Close()
    {
        if (OnClose != null)
            OnClose(this);

        if (_responseDelegate != null)
            _responseDelegate(GenerateResponse());
    }

    public void Cancel()
    {
        if (OnCancel != null)
            OnCancel(this);
    }

    protected virtual Response GenerateResponse()
    {
        return new Response();
    }
}