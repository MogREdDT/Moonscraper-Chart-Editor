﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MSE.Input;

public class RebindOverlayInterface : MonoBehaviour
{
    InputRebinder rebinder;
    public MSE.Event rebindCompleteEvent = new MSE.Event();
    public MSE.Event<InputAction> inputConflictEvent = new MSE.Event<InputAction>();

    [SerializeField]
    Text conflictNotificationText;
    const string conflictFormatStr = "Cannot remap to {0} as it is already in use by {1}";

    private void OnEnable()
    {
        conflictNotificationText.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (MSChartEditorInput.GetInputDown(MSChartEditorInputActions.CloseMenu))
        {
            Close(false);
        }
        else
        {

            InputAction conflict;
            IInputMap attemptedInput;
            if (rebinder.TryMap(out conflict, out attemptedInput))
            {
                Close(true);
            }
            else if (conflict != null)
            {
                inputConflictEvent.Fire(conflict);
                conflictNotificationText.text = string.Format(conflictFormatStr, attemptedInput.GetInputStr(), conflict.properties.displayName);
                conflictNotificationText.enabled = true;
            }
        }
    }

    public void Open(InputAction actionToRebind, IInputMap mapToRebind, IEnumerable<InputAction> allActions, IInputDevice device)
    {
        if (ChartEditor.Instance)
            ChartEditor.Instance.uiServices.SetPopupBlockingEnabled(true);

        rebinder = new InputRebinder(actionToRebind, mapToRebind, allActions, device);
        gameObject.SetActive(true);
    }

    void Close(bool rebindSuccess)
    {
        if (!rebindSuccess && rebinder != null)
        {
            rebinder.RevertMapBeingRebound();
        }

        rebinder = null;
        gameObject.SetActive(false);
        rebindCompleteEvent.Fire();

        if (ChartEditor.Instance)
            ChartEditor.Instance.uiServices.SetPopupBlockingEnabled(false);
    }

    void OnDisable()
    {
        if (rebinder != null)
        {
            Close(false);
        }
    }

    public void ClearInput()
    {
        Close(true);
    }
}