using System;
using System.Reflection;
using System.Collections.Generic;
using BepInEx.Configuration;
using RisingSlash.FP2Mods.MillasToybox;
using UnityEngine;

namespace RisingSlash.FP2Mods.MillasToybox
{
    public class FP2TrainerCustomHotkeys : MonoBehaviour
    {
        public static Dictionary<ConfigEntry<string>, KeyMapping> DictHotkeyPrefToKeyMappings;
        public void Start()
        {
            if (DictHotkeyPrefToKeyMappings == null)
            {
                DictHotkeyPrefToKeyMappings = new Dictionary<ConfigEntry<string>, KeyMapping>();
            }
        }

        public static void Add(ConfigEntry<string> mpe)
        {
            if (DictHotkeyPrefToKeyMappings == null)
            {
                DictHotkeyPrefToKeyMappings = new Dictionary<ConfigEntry<string>, KeyMapping>();
            }
            
            DictHotkeyPrefToKeyMappings.Add(mpe, new KeyMapping(mpe.Value, KeyboardInputFromString(mpe.Value), null, null) );
        }
        
        public static bool GetButtonDown(ConfigEntry<string> mpe)
        {
            if (mpe == null)
            {
                MillasToybox.Log(String.Format("mpe appears to be null: {0}", mpe));
                return false;
            }
            else if (mpe.Value == null)
            {
                MillasToybox.Log(String.Format("mpe's is set, but value appears to be null: {0} -> {1}", mpe.Definition.Key, mpe.Value));
                return false;
            }

            return InputControl.GetButtonDown(DictHotkeyPrefToKeyMappings[mpe], true);
        }
        
        public static bool GetButton(ConfigEntry<string> mpe)
        {
            if (mpe == null)
            {
                MillasToybox.Log(String.Format("mpe appears to be null: {0}", mpe));
                return false;
            }
            else if (mpe.Value == null)
            {
                MillasToybox.Log(String.Format("mpe's is set, but value appears to be null: {0} -> {1}", mpe.Definition.Key, mpe.Value));
                return false;
            }

            return InputControl.GetButton(DictHotkeyPrefToKeyMappings[mpe], true);
        }

        public static KeyboardInput KeyboardInputFromString(string value)
        {
            if (value == null)
            {
                return null;
            }


            var modifiers = ModifiersFromString(value);
            try
            {
                String baseInput = value;
                if (modifiers != KeyModifier.NoModifier)
                {
                    baseInput = value.Substring(value.LastIndexOf("+") + 1);
                    MillasToybox.Log(String.Format("Input base interpreted as {0} -> {1}\n", value, baseInput));
                }

                 
                return new KeyboardInput(KeyCodeFromString(baseInput), modifiers);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void OnLevelWasLoaded(int level)
        {
            try
            {
                
            }
            catch (Exception e)
            {
                MillasToybox.Log(e.Message + e.StackTrace);
            }
        }

        public static KeyCode KeyCodeFromString(string strKeyCode)
        {
            return (KeyCode) Enum.Parse(typeof(KeyCode), strKeyCode);
        }

        public static KeyModifier ModifiersFromString(string value)
        {
            KeyModifier keyMod = KeyModifier.NoModifier;

            if (value == null)
            {
                return keyMod;
            }

            int maxModifiers = 7;
            var strCtrlP = "Ctrl+";
            var strAltP = "Alt+";
            var strShiftP = "Shift+";
            
            for (int i = 0; i < maxModifiers; i++)
            {
                if (value.Contains(strCtrlP))
                {
                    value = value.Replace(strCtrlP, "");
                    keyMod |= KeyModifier.Ctrl;
                    continue;
                }
                if (value.Contains(strAltP))
                {
                    value = value.Replace(strAltP, "");
                    keyMod |= KeyModifier.Alt;
                    continue;
                }
                if (value.Contains(strShiftP))
                {
                    value = value.Replace(strShiftP, "");
                    keyMod |= KeyModifier.Shift;
                    continue;
                }
                break;
            }

            return keyMod;
        }

        public static string GetBindingString()
        {
            string strControlListing = "Current Hotkey Bindings:\n";
            
            // Optimization option: Possible optimization by caching a List version of these pairs instead of using the dictionary.
            foreach (var configBinding in DictHotkeyPrefToKeyMappings.Keys)
            {
                strControlListing += String.Format("{0} -> {1}\n", configBinding.Value, configBinding.Definition.Key);
            }

            return strControlListing;
        }
        
        public static string GetBindingString(int start, int end)
        {
            string strControlListing = "Current Hotkey Bindings:\n";
            int lineCount = 1;
            // Optimization option: Possible optimization by caching a List version of these pairs instead of using the dictionary.
            foreach (var configBinding in DictHotkeyPrefToKeyMappings.Keys)
            {
                if (lineCount >= start && lineCount <= end)
                {
                    strControlListing += String.Format("{1} -> {0}\n", configBinding.Value, configBinding.Definition.Key);
                }

                lineCount++;
            }

            return strControlListing;
        }
    }
}