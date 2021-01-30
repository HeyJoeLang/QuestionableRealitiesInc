using UnityEngine;
using System.Collections;
using UnityEditor;
using CinemaDirector;

[CutsceneItemControlAttribute(typeof(CinemaAudio))]
public class CinemaAudioControl : ActionFixedItemControl
{
    private string audioItemName = string.Empty;
    private Texture2D texture = null;

    public CinemaAudioControl()
    {
        base.AlterFixedAction += CinemaAudioControl_AlterFixedAction;
    }

    public override void Initialize(TimelineItemWrapper wrapper, TimelineTrackWrapper track)
    {
        base.Initialize(wrapper, track);
        actionIcon = EditorGUIUtility.Load("Cinema Suite/Cinema Director/Director_AudioIcon.png") as Texture;
    }

    void CinemaAudioControl_AlterFixedAction(object sender, ActionFixedItemEventArgs e)
    {
        CinemaAudio audioItem = e.actionItem as CinemaAudio;
        if (audioItem == null) return;

        if (e.duration <= 0)
        {
            deleteItem(audioItem);
        }
        else
        {
            Undo.RecordObject(e.actionItem, string.Format("Change {0}", audioItem.name));
            audioItem.Firetime = e.firetime;
            audioItem.Duration = e.duration;
            audioItem.InTime = e.inTime;
            audioItem.OutTime = e.outTime;
        }
    }

    protected override void showContextMenu(Behaviour behaviour)
    {
        GenericMenu createMenu = new GenericMenu();
        if (TrackControl.isExpanded)
        {
            createMenu.AddDisabledItem(new GUIContent("Rename"));
        }
        else
        {
            createMenu.AddItem(new GUIContent("Rename"), false, renameItem, behaviour);
        }
        createMenu.AddItem(new GUIContent("Copy"), false, copyItem, behaviour);
        createMenu.AddItem(new GUIContent("Delete"), false, deleteItem, behaviour);
        createMenu.ShowAsContext();
    }

    public override void Draw(DirectorControlState state)
    {
        CinemaAudio audioItem = Wrapper.Behaviour as CinemaAudio;
        if (audioItem == null) return;
        AudioSource audioSource = audioItem.GetComponent<AudioSource>();

        if (!TrackControl.isExpanded)
        {
            if (Selection.Contains(audioItem.gameObject))
            {
                GUI.Box(controlPosition, string.Empty, TimelineTrackControl.styles.AudioTrackItemSelectedStyle);
            }
            else
            {
                GUI.Box(controlPosition, string.Empty, TimelineTrackControl.styles.AudioTrackItemStyle);
            }
            Color temp = GUI.color;
            GUI.color = new Color(1f, 0.65f, 0f);
            Rect icon = controlPosition;
            icon.x += 4;
            icon.width = 16;
            icon.height = 16;
            GUI.Box(icon, actionIcon, GUIStyle.none);
            GUI.color = temp;

            Rect labelPosition = controlPosition;
            labelPosition.x = icon.xMax;
            labelPosition.width -= (icon.width + 4);


            DrawRenameLabel(audioItem.name, labelPosition);
        }
        else
        {
            if (Selection.Contains(audioItem.gameObject))
            {
                GUI.Box(controlPosition, string.Empty, TimelineTrackControl.styles.TrackItemSelectedStyle);
            }
            else
            {
                GUI.Box(controlPosition, string.Empty, TimelineTrackControl.styles.TrackItemStyle);
            }
            if (audioSource != null && audioSource.clip != null)
            {
                GUILayout.BeginArea(controlPosition);
                if (texture == null || audioItemName != audioSource.clip.name)
                {
                    audioItemName = audioSource.clip.name;

                    
                    if (!EditorPrefs.HasKey("DirectorControl.UseHQWaveformTextures"))
                    {
                        EditorPrefs.SetBool("DirectorControl.UseHQWaveformTextures", true);
                    }

                    if (EditorPrefs.GetBool("DirectorControl.UseHQWaveformTextures"))
                    {
                        texture = GenerateAudioWaveformTexture(audioSource.clip);
                    }
                    else
                    {
                        texture = AssetPreview.GetAssetPreview(audioSource.clip);
                    }
                }

                float inTimeOffset = (audioItem.InTime) * state.Scale.x;
                float outTimeOffset = (audioItem.ItemLength - audioItem.OutTime) * state.Scale.x;
                Rect texturePosition = new Rect(-inTimeOffset + 2, 0, controlPosition.width - 4 + inTimeOffset + outTimeOffset, controlPosition.height);

                if (texture != null)
                {
                    GUI.DrawTexture(texturePosition, texture, ScaleMode.StretchToFill);
                }
                GUILayout.EndArea();
            }
        }
    }

    private Texture2D GenerateAudioWaveformTexture(AudioClip audio)
    {
        Color waveformColor = new Color(1, 0.5f, 0);
        Color bgColor = new Color(0, 0, 0, 0.5f);
        int width = Mathf.Clamp((int)(128 * Mathf.Ceil(audio.length)), 0, 16384); // Textures limited to 16384 pixels
        int height = 64;

        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);

        // Texture Background
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                tex.SetPixel(x, y, bgColor);
            }
        }

        float[] samples = new float[audio.samples * audio.channels];
        audio.GetData(samples, 0);

        float[] waveform;
        float[] channelSamples;

        for (int channel = 0; channel < audio.channels; channel++)
        {
            waveform = new float[width];
            channelSamples = new float[audio.samples];
            int channelSampleIndex = 0;

            for (int i = channel; i < samples.Length; i += audio.channels)
            {
                channelSamples[channelSampleIndex++] = samples[i];
            }

            int packSize = (channelSamples.Length / width) + 1;
            int s = 0;
            for (int i = 0; i < channelSamples.Length; i += packSize)
            {
                waveform[s] = Mathf.Abs(channelSamples[i]);
                s++;
            }

            // Draw waveform 
            float heightModifier = audio.channels == 1 ? 0.5f :
                                        channel == 0 ? .75f : 0.25f;

            for (int x = 0; x < waveform.Length; x++)
            {
                for (int y = 0; y <= waveform[x] * ((float)height * .35f); y++)
                {
                    tex.SetPixel(x, (int)(height * heightModifier) + y, waveformColor);
                    tex.SetPixel(x, (int)(height * heightModifier) - y, waveformColor);
                }
            }
        }

        tex.Apply();

        return tex;
    }
}
