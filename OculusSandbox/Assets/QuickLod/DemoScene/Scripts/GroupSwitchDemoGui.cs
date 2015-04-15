using System;
using System.Globalization;

using QuickLod.GroupSwitch;

using UnityEngine;

public class GroupSwitchDemoGui : MonoBehaviour
{
    [SerializeField]
    private bool showInfo = true;

    [SerializeField]
    private bool showSettings = true;

    [SerializeField]
    private bool showTriggers = true;

    [SerializeField]
    private GsGroupSwitch groupSwitch;
    
    private Rect iwr = new Rect(10, 10, 200, 20);

    private Rect swr = new Rect(Screen.width - 420, 10, 200, 20);

    private Rect twr = new Rect(Screen.width - 210, 10, 200, 20);

    private Vector2 infoScrollPosition;

    private Vector2 settingsScrollPosition;

    private Vector2 triggersScrollPosition;
    
    private void OnGUI()
    {
        GUI.skin.label.alignment = TextAnchor.MiddleLeft;
        GUI.contentColor = Color.black;

        this.iwr = GUILayout.Window(0, this.iwr, this.DrawInfoWindow, string.Empty);
        this.iwr.x = Mathf.Max(10, Mathf.Min(Screen.width - this.iwr.width - 10, this.iwr.x));
        this.iwr.y = Mathf.Max(10, Mathf.Min(Screen.height - this.iwr.height - 10, this.iwr.y));
        this.iwr.width = 200;
        this.iwr.height = 0;  

        if (this.groupSwitch != null)
        {
            this.swr = GUILayout.Window(1, this.swr, this.DrawSettingsWindow, string.Empty);
            this.swr.x = Mathf.Max(10, Mathf.Min(Screen.width - this.swr.width - 10, this.swr.x));
            this.swr.y = Mathf.Max(10, Mathf.Min(Screen.height - this.swr.height - 10, this.swr.y));
            this.swr.width = 200;
            this.swr.height = 0;

            this.twr = GUILayout.Window(2, this.twr, this.DrawTriggerWindow, string.Empty);
            this.twr.x = Mathf.Max(10, Mathf.Min(Screen.width - this.twr.width - 10, this.twr.x));
            this.twr.y = Mathf.Max(10, Mathf.Min(Screen.height - this.twr.height - 10, this.twr.y));
            this.twr.width = 200;
            this.twr.height = 0;
        }
    }

    private void DrawInfoWindow(int id)
    {
        var prevColor = GUI.color;
        GUI.color = Color.black;
        GUI.Label(new Rect(20, -1, this.iwr.width - 20, 22), "Info");
        GUI.color = prevColor;

        this.showInfo = GUI.Toggle(new Rect(3, 0, 14, 16), this.showInfo, new GUIContent());
        GUI.DragWindow(new Rect(0, 0, 1000, 18));

        if (this.showInfo)
        {
            this.infoScrollPosition = GUILayout.BeginScrollView(this.infoScrollPosition, GUILayout.Height(Mathf.Min(510, Screen.height - 50))); 
            GUILayout.Label("This scene shows an example for the GroupSwitch feature of QuickLod.");
            GUILayout.Label(
                "It contains multiple triggers, the red ones are collision triggers while the blue ones are distance triggers.");
            GUILayout.Label(
                "Collision triggers are activated when they collide with a collider that matches their filter setting.");
            GUILayout.Label("Distance triggers are triggered when a LodSource (your camera) comes inside their range.");
            GUILayout.Label(
                "When at least one trigger is triggered, the group switch will enable all managed objects (white cubes in the center).");
            GUILayout.Label(
                "When no trigger is triggered, then the group switch will deactivate all managed objects after a defined time.");
            GUILayout.Label(
                "You can also force the group switch to show or hide the managed objects by using the \"Force trigger state\" option");
            GUILayout.EndScrollView();
        }
        else
        {
            GUILayout.FlexibleSpace();
        }
    }

    private void DrawSettingsWindow(int id)
    {
        var prevColor = GUI.color;
        GUI.color = Color.black;
        GUI.Label(new Rect(20, -1, this.swr.width - 20, 22), "Settings");
        GUI.color = prevColor;

        this.showSettings = GUI.Toggle(new Rect(3, 0, 14, 16), this.showSettings, new GUIContent());
        GUI.DragWindow(new Rect(0, 0, 1000, 18));

        if (this.showSettings)
        {
            this.settingsScrollPosition = GUILayout.BeginScrollView(this.settingsScrollPosition, GUILayout.Height(Mathf.Min(this.groupSwitch.ForceTriggerState ? 95 : 73, Screen.height - 50))); 
            
            GUILayout.BeginHorizontal();
            GUILayout.Label("Delay:", GUILayout.MaxWidth(40));
            this.groupSwitch.DeactivationDelay = GUILayout.HorizontalSlider(this.groupSwitch.DeactivationDelay, 0, 5, GUILayout.Width(100));
            GUILayout.Label(Math.Round(this.groupSwitch.DeactivationDelay, 2).ToString(CultureInfo.InvariantCulture));
            GUILayout.EndHorizontal();

            this.groupSwitch.ForceTriggerState = GUILayout.Toggle(
                this.groupSwitch.ForceTriggerState, "Force trigger state");

            if (this.groupSwitch.ForceTriggerState)
            {
                this.groupSwitch.ForcedState = GUILayout.Toggle(this.groupSwitch.ForcedState, "Forced state");
            }

            GUILayout.EndScrollView();
        }
        else 
        {
            GUILayout.FlexibleSpace();
        }
    }

    private void DrawTriggerWindow(int id)
    {
        var prevColor = GUI.color;
        GUI.color = Color.black;
        GUI.Label(new Rect(20, -1, this.swr.width - 20, 22), "Triggers");
        GUI.color = prevColor;

        this.showTriggers = GUI.Toggle(new Rect(3, 0, 14, 16), this.showTriggers, new GUIContent());
        GUI.DragWindow(new Rect(0, 0, 1000, 18));

        if (this.showTriggers)
        {
            this.triggersScrollPosition = GUILayout.BeginScrollView(this.triggersScrollPosition, GUILayout.Height(Mathf.Min(211, Screen.height - 50))); 
            foreach (var trigger in this.groupSwitch.Triggers)
            {
                var triggerObject = trigger as MonoBehaviour;
                if (triggerObject == null)
                {
                    continue;
                }

                GUI.color = trigger.IsTriggered ? Color.green : Color.red;
                GUILayout.Label(triggerObject.name + " : " + trigger.Name);
            }

            GUILayout.EndScrollView();

            GUI.color = prevColor;
        }
        else
        {
            GUILayout.FlexibleSpace();
        }
    }
}
