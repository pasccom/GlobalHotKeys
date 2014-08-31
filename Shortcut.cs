using System;
using System.Reflection;
using System.Collections.Generic;

namespace GlobalHotKeys
{
    class Shortcut
    {
        public enum Modifiers { ALT = 0x1, CTRL = 0x2, SHIFT = 0x4, META = 0x8 }
        public enum Keys
        {
            None = 0x0, Esc = 0x1B,
            A = 0x41, B = 0x42, C = 0x43, D = 0x44, E = 0x45, F = 0x46, G = 0x47, H = 0x48, I = 0x49, J = 0x4A, K = 0x4B, L = 0x4C, M = 0x4D, N = 0x4E, O = 0x4F, P = 0x50, Q = 0x51, R = 0x52, S = 0x53, T = 0x54, U = 0x55, V = 0x56, W = 0x57, X = 0x58, Y = 0x59, Z = 0x5A,
            Left = 0x25, Up = 0x26, Right = 0x27, Down = 0x28, PgUp = 0x21, PgDown = 0x22,
            F1 = 0x70, F2 = 0x71, F3 = 0x72, F4 = 0x73, F5 = 0x74, F6 = 0x75, F7 = 0x76, F8 = 0x77, F9 = 0x78, F10 = 0x79, F11 = 0x7A, F12 = 0x7B,
            N0 = 0x60, N1 = 0x61, N2 = 0x62, N3 = 0x63, N4 = 0x64, N5 = 0x65, N6 = 0x66, N7 = 0x67, N8 = 0x68, N9 = 0x69
        }

        public Modifiers Modifier { get; private set; }
        public Keys Key { get; private set; }
        public string Class { get; set; }
        public string Method { get; set; }
        public List<string> Params { get; set; }

        public bool Loaded { get; set; }

        public static Shortcut exitShortcut = new Shortcut() {
            Modifier = Modifiers.ALT | Modifiers.CTRL,
            Key = Keys.C,
            Class = "Shortcuts.Handler",
            Method = "exit",
        };

        public static Shortcut resetShortcut = new Shortcut() {
            Modifier = Modifiers.ALT | Modifiers.CTRL,
            Key = Keys.Esc,
            Class = "Shortcuts.Handler",
            Method = "reset",
        };

        public Shortcut()
        {
            Loaded = false;
            Params = new List<string>();
        }

        public void addModifier(Modifiers modifier)
        {
            Modifier |= modifier;
        }

        public void removeModifier(Modifiers modifier)
        {
            Modifier &= (~modifier);
        }

        public void setKey(string key)
        {
            Type keyEnumType = GetType().GetNestedType("Keys");
            if ((keyEnumType == null) || !keyEnumType.IsEnum)
                throw new ReflectionTypeLoadException(null, null, "Couldn't find the enum Keys");

            string[] names = keyEnumType.GetEnumNames();
            Array values = keyEnumType.GetEnumValues();

            for (int i = 0; i < names.Length; i++) {
                if (names[i] == key) {
                    Key = (Keys)values.GetValue(i);
                    return;
                }
            }

            throw new ArgumentException("Expected a key name. The key names are the name of the enum Keys");
        }

        public override string ToString()
        {
            Shortcut.Modifiers[] modifiers = { Shortcut.Modifiers.ALT, Shortcut.Modifiers.CTRL, Shortcut.Modifiers.SHIFT, Shortcut.Modifiers.META };
            string ret = String.Empty;

            for (int i = 0; i < modifiers.Length; i++)
                if ((uint)(Modifier & modifiers[i]) != 0)
                    ret += (modifiers[i] + "+");
            ret += (Key + " -> ");
            if ((Class != null) && (Class != String.Empty)) {
                ret += Class;
                if ((Class != null) && (Class != String.Empty))
                    ret += ("." + Method + "(" + String.Join<string>(",", Params) + ")");
            }

            return ret;
        }

    }
}
