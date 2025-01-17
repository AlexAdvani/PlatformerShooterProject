﻿using UnityEngine;

using Rewired;

public class SettingsManager : SingletonBehaviour<SettingsManager>
{
	// Config Filename
	public string sConfigFileName;
    // Input Filename
    public string sInputFileName;

    // Gamepad Input XML Settings
    public static string sPadInputXml = "";
    // Gamepad Input Settings Assigned flag
    public static bool bPadInputAssigned = true;

    // Has An Option Changed flag
    public static bool bOptionChanged = false;

	// Initialization
	public override void Awake()
	{
		base.Awake();

        LoadSettings();
		InitializeAudioSettings();
		bOptionChanged = false;
	}

	// Initializes audio settings
	private void InitializeAudioSettings()
	{
		AudioManager.Instance.SetVolume("MasterVolume", GlobalSettings.fMasterVolume);
		AudioManager.Instance.SetVolume("SoundVolume", GlobalSettings.fSoundVolume);
		AudioManager.Instance.SetVolume("MusicVolume", GlobalSettings.fMusicVolume);
		AudioManager.Instance.SetMute(GlobalSettings.bMute);
	}

    #region Loading Methods

    // Try Reading Int Value
    private int TryReading(ES2Reader reader, string varName, int defaultValue)
    {
        bool varExists = reader.TagExists(varName);

        if (varExists)
        {
            return reader.Read<int>(varName);
        }
        else
        {
            return defaultValue;
        }
    }

    // Try Reading Float Value
    private float TryReading(ES2Reader reader, string varName, float defaultValue)
    {
        bool varExists = reader.TagExists(varName);

        if (varExists)
        {
            return reader.Read <float>(varName);
        }
        else
        {
            return defaultValue;
        }
    }

    // Try Reading String Value
    private string TryReading(ES2Reader reader, string varName, string defaultValue)
    {
        bool varExists = reader.TagExists(varName);

        if (varExists)
        {
            return reader.Read<string>(varName);
        }
        else
        {
            return defaultValue;
        }
    }

    // Try Reading Bool Value
    private bool TryReading(ES2Reader reader, string varName, bool defaultValue)
    {
        bool varExists = reader.TagExists(varName);

        if (varExists)
        {
            return reader.Read<bool>(varName);
        }
        else
        {
            return defaultValue;
        }
    }

    #endregion

    #region Loading

    // Loads Settings if a config file exists
    private void LoadSettings()
    {
        // If file exists, then load settings
        if (ES2.Exists(Application.persistentDataPath + "/" + sConfigFileName))
        {
            ES2Reader configReader = ES2Reader.Create(Application.persistentDataPath + "/" + sConfigFileName);

            LoadConfig(configReader);

            configReader.Dispose();
        }
        else // Otherwise, detect display settings
        {
            DetectDisplaySettings();
            QualitySettings.vSyncCount = 1;
        }

        if (ES2.Exists(Application.persistentDataPath + "/" + sInputFileName))
        {
            ES2Reader inputReader = ES2Reader.Create(Application.persistentDataPath + "/" + sInputFileName);

            LoadInputMaps(inputReader);
            LoadGamepadConfigMaps(inputReader);

            inputReader.Dispose();
        }
    }

    // Detects the user's maximum supported resolution and sets that resolution
    private void DetectDisplaySettings()
    {
        Display display = Display.displays[0];
        int width = display.renderingWidth;
        int height = display.renderingHeight;

        if (width >= 3840)
        {
            GlobalSettings.iResolution = 14;
        }
        else if (width >= 3440)
        {
            GlobalSettings.iResolution = 13;
        }
        else if (width >= 2560)
        {
            if (height >= 1440)
            {
                GlobalSettings.iResolution = 12;
            }
            else
            {
                GlobalSettings.iResolution = 11;
            }
        }
        else if (width >= 1920)
        {
            if (height >= 1200)
            {
                GlobalSettings.iResolution = 10;
            }
            else
            {
                GlobalSettings.iResolution = 9;
            }
        }
        else if (width >= 1680)
        {
            GlobalSettings.iResolution = 8;
        }
        else if (width >= 1600)
        {
            GlobalSettings.iResolution = 7;
        }
        else if (width >= 1440)
        {
            GlobalSettings.iResolution = 6;
        }
        else if (width >= 1366)
        {
            GlobalSettings.iResolution = 5;
        }
        else if (width >= 1280)
        {
            if (height >= 800)
            {
                GlobalSettings.iResolution = 4;
            }
            else if (height >= 768)
            {
                GlobalSettings.iResolution = 3;
            }
            else
            {
                GlobalSettings.iResolution = 2;
            }
        }
        else if (width >= 1024)
        {
            GlobalSettings.iResolution = 1;
        }
        else
        {
            GlobalSettings.iResolution = 0;
        }

        Screen.SetResolution(width, height, true);
    }

    // Loads Options Config
    private void LoadConfig(ES2Reader reader)
    {
        GlobalSettings.iCrosshairVisibility = TryReading(reader, "Crosshair", 0);
        GlobalSettings.iLaserVisible = TryReading(reader, "LaserSight", 0);
        GlobalSettings.iSlideInputMethod = TryReading(reader, "SlideInputCustom", 0);
        GlobalSettings.iResolution = TryReading(reader, "ResolutionID", -1);
        GlobalSettings.iDisplayMode = TryReading(reader, "DisplayMode", 0);
        GlobalSettings.iMonitor = TryReading(reader, "Monitor", 0);
        GlobalSettings.bVSync = TryReading(reader, "VSync", true);
        GlobalSettings.fMasterVolume = TryReading(reader, "MasterVolume", 1f);
        GlobalSettings.fSoundVolume = TryReading(reader, "SoundVolume", 1f);
        GlobalSettings.fMusicVolume = TryReading(reader, "MusicVolume", 1f);
        GlobalSettings.bMute = TryReading(reader, "Mute", false);
        GlobalSettings.bVibration = TryReading(reader, "Vibration", true);

        // If resolution setting is missing, detect display settings
        if (GlobalSettings.iResolution == -1)
        {
            DetectDisplaySettings();
        }
    }

    // Loads Input Maps
    private void LoadInputMaps(ES2Reader reader)
    {
        Player playerInput = ReInput.players.GetPlayer(0);

        string keyMap = reader.Read<string>(ControllerType.Keyboard.ToString());
        string mouseMap = reader.Read<string>(ControllerType.Mouse.ToString());

        string padMap = "";

        if (reader.TagExists(ControllerType.Joystick.ToString()))
        {
            padMap = reader.Read<string>(ControllerType.Joystick.ToString());
        }

        playerInput.controllers.maps.AddMapFromXml(ControllerType.Keyboard, 0, keyMap);
        playerInput.controllers.maps.AddMapFromXml(ControllerType.Mouse, 0, mouseMap);

        Joystick[] gamepads = ReInput.controllers.GetJoysticks();

        if (gamepads != null)
        {
            if (gamepads.Length > 0)
            {
                playerInput.controllers.maps.AddMapFromXml(ControllerType.Joystick, 0, padMap);
                bPadInputAssigned = true;
            }
        }
        else
        {
            sPadInputXml = padMap;
            bPadInputAssigned = false;
        }
    }

    // Loads Gamepad Configuration Maps
    private void LoadGamepadConfigMaps(ES2Reader reader)
    {
        if (ReInput.controllers.joystickCount > 0)
        {
            foreach (Joystick gamepad in ReInput.controllers.Joysticks)
            {
                string configMap = "";

                if (reader.TagExists(gamepad.name))
                {
                    configMap = reader.Read<string>(gamepad.name);
                }

                if (configMap != "")
                {
                    gamepad.ImportCalibrationMapFromXmlString(configMap);
                }
            }
        }
    }

    #endregion

    #region Saving

    // Saves Config Settings
    public void SaveSettings()
	{
		if (!bOptionChanged)
		{
			return;
		}

		ES2Writer writer = ES2Writer.Create(Application.persistentDataPath + "/" + sConfigFileName);

		SaveConfig(writer);
		
		writer.Save();
		writer.Dispose();

        ES2Writer inputWriter = ES2Writer.Create(Application.persistentDataPath + "/" + sInputFileName);

        SaveInputMaps(inputWriter);
        SaveGamepadConfigMaps(inputWriter);

        inputWriter.Save();
        inputWriter.Dispose();

        SaveNotificationUI.OpenSaveNotification();

        bOptionChanged = false;
    }

	// Saves Options Config
	private void SaveConfig(ES2Writer writer)
	{
        writer.Write<int>(GlobalSettings.iCrosshairVisibility, "Crosshair");
        writer.Write<int>(GlobalSettings.iLaserVisible, "LaserSight");
		writer.Write<int>(GlobalSettings.iResolution, "ResolutionID");
		writer.Write<int>(GlobalSettings.iDisplayMode, "DisplayMode");
		writer.Write<int>(GlobalSettings.iMonitor, "Monitor");
		writer.Write<bool>(GlobalSettings.bVSync, "VSync");
		writer.Write<float>(GlobalSettings.fMasterVolume, "MasterVolume");
		writer.Write<float>(GlobalSettings.fSoundVolume, "SoundVolume");
		writer.Write<float>(GlobalSettings.fMusicVolume, "MusicVolume");
		writer.Write<bool>(GlobalSettings.bMute, "Mute");
		writer.Write<bool>(GlobalSettings.bVibration, "Vibration");
	}

	// Saves Input Maps
	private void SaveInputMaps(ES2Writer writer)
	{
		PlayerSaveData inputData = ReInput.players.GetPlayer(0).GetSaveData(true);

		foreach (ControllerMapSaveData mapData in inputData.AllControllerMapSaveData)
		{
			writer.Write(mapData.map.ToXmlString(), mapData.controllerType.ToString());
		}
	}

	// Saves Gamepad Input Configuration
	private void SaveGamepadConfigMaps(ES2Writer writer)
	{
		if (ReInput.controllers.joystickCount > 0)
		{
			foreach (Joystick gamepad in ReInput.controllers.Joysticks)
			{
				JoystickCalibrationMapSaveData configData = gamepad.GetCalibrationMapSaveData();
				writer.Write<string>(configData.map.ToXmlString(), gamepad.name);
			}
		}
	}

    #endregion
}
