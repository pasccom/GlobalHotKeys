using System;
using System.Reflection;
using System.Collections.Generic;

namespace GlobalHotKeys
{
    class ShortcutData
    {
        /// <summary>
        ///     Enumeration of all supported modifiers.
        ///     <list type="bullet">
        ///         <item><term>R_ALT</term> <description>The right ALT key is pressed</description></item>
        ///         <item><term>L_ALT</term> <description>The left ALT key is pressed</description></item>
        ///         <item><term>X_ALT</term> <description>Any of the ALT key is pressed</description></item>
        ///         <item><term>R_CTRL</term> <description>The right CTRL key is pressed</description></item>
        ///         <item><term>L_CTRL</term> <description>The left CTRL key is pressed</description></item>
        ///         <item><term>X_CTRL</term> <description>Any of the CTRL key is pressed</description></item>
        ///         <item><term>R_SHIFT</term> <description>The right Shift key is pressed</description></item>
        ///         <item><term>L_SHIFT</term> <description>The left Shift key is pressed</description></item>
        ///         <item><term>X_SHIFT</term> <description>Any of the Shift key is pressed</description></item>
        ///         <item><term>R_META</term> <description>The right "windows" key is pressed</description></item>
        ///         <item><term>L_META</term> <description>The left "windows" key is pressed</description></item>
        ///         <item><term>X_META</term> <description>Any of the "windows" key is pressed</description></item>
        ///     </list>
        /// </summary>
        public enum Modifiers { R_ALT = 0x01, L_ALT = 0x02, X_ALT = 0x03,
                                R_CTRL = 0x04, L_CTRL = 0x08, X_CTRL = 0x0C,
                                R_SHIFT = 0x10, L_SHIFT = 0x20, X_SHIFT = 0x30,
                                R_META = 0x40, L_META = 0x80, X_META = 0xC0 }
        /// <summary>
        ///     Enumeration of the supported keys. 
        ///     <list type="bullet">
        ///         <item><term>None</term> <description>No key. The shortcut is invalid.</description></item>
        ///         <item><term>Esc</term> <description>Escape key.</description></item>
        ///         <item><term>[A-Z]</term> <description>The specified letter is pressed.</description></item>
        ///         <item><term>Left</term> <description>The left arrow key is pressed.</description></item>
        ///         <item><term>Up</term> <description>The up arrow key is pressed.</description></item>
        ///         <item><term>Right</term> <description>The right arrow key is pressed.</description></item>
        ///         <item><term>Down</term> <description>The down arrow key is pressed.</description></item>
        ///         <item><term>PgUp</term> <description>The previous page key is pressed.</description></item>
        ///         <item><term>PgDown</term> <description>The next page key is pressed.</description></item>
        ///         <item><term>F[1-12]</term> <description>The specified function key is pressed.</description></item>
        ///         <item><term>N[0-9]</term> <description>The specified numpad key is pressed.</description></item>
        ///     </list>
        /// </summary>
        public enum Keys
        {
            None = 0x0, Esc = 0x1B,
            A = 0x41, B = 0x42, C = 0x43, D = 0x44, E = 0x45, F = 0x46, G = 0x47, H = 0x48, I = 0x49, J = 0x4A, K = 0x4B, L = 0x4C, M = 0x4D, N = 0x4E, O = 0x4F, P = 0x50, Q = 0x51, R = 0x52, S = 0x53, T = 0x54, U = 0x55, V = 0x56, W = 0x57, X = 0x58, Y = 0x59, Z = 0x5A,
            Left = 0x25, Up = 0x26, Right = 0x27, Down = 0x28, PgUp = 0x21, PgDown = 0x22,
            F1 = 0x70, F2 = 0x71, F3 = 0x72, F4 = 0x73, F5 = 0x74, F6 = 0x75, F7 = 0x76, F8 = 0x77, F9 = 0x78, F10 = 0x79, F11 = 0x7A, F12 = 0x7B,
            N0 = 0x60, N1 = 0x61, N2 = 0x62, N3 = 0x63, N4 = 0x64, N5 = 0x65, N6 = 0x66, N7 = 0x67, N8 = 0x68, N9 = 0x69
        }

        /// <summary>
        ///     Modifiers of this shortcut.
        /// </summary>
        /// <para> To add or remove a modifier, use <see cref="addModifier"/> and <see cref="removeModifier"/> respectively.</para>
        /// <seealso cref="Modifier"/>
        public Modifiers Modifier { get; private set; }
        /// <summary>
        ///     Key of this shortcut.
        /// </summary>
        /// <para> To set the key use the <see cref="setKey"/> method.</para>
        /// <seealso cref="Keys"/>
        public Keys Key { get; private set; }
        public string Class { get; set; }
        public string Method { get; set; }
        public List<string> Params { get; set; }

        public int Id { get; set; }

        public static ShortcutData exitShortcut = new ShortcutData() {
            Modifier = Modifiers.X_ALT | Modifiers.X_CTRL,
            Key = Keys.C,
            Class = "Shortcuts.Handler",
            Method = "exit",
        };

        public static ShortcutData resetShortcut = new ShortcutData() {
            Modifier = Modifiers.X_ALT | Modifiers.X_CTRL,
            Key = Keys.Esc,
            Class = "Shortcuts.Handler",
            Method = "reset",
        };

        public ShortcutData()
        {
            Key = Keys.None;
            Id = 0;
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

        public static uint getKeyHashCode(uint key)
        {
            uint offset = 0;

            // Handles no key:
            if (key == (uint)Keys.None)
                return offset;
            offset++;

            // Handles Escape key: 
            if (key == (uint)Keys.Esc)
                return offset;
            offset++;

            // Handles page dowm and page up keys:
            if ((key == (uint)Keys.PgUp) || (key == (uint)Keys.PgDown))
                return offset + key - (uint)Keys.PgUp;
            offset += 2;

            // Handles directionnal arrow keys:
            if ((key >= (uint)Keys.Left) && (key <= (uint)Keys.Down))
                return offset + key - (uint)Keys.Left;
            offset += (1 + (uint)Keys.Down - (uint)Keys.Left);

            // Handles F1 to F12 keys:
            if ((key >= (uint)Keys.F1) && (key <= (uint)Keys.F12))
                return offset + key - (uint)Keys.F1;
            offset += (1 + (uint)Keys.F12 - (uint)Keys.F1);

            // Handles letters keys:
            if ((key >= (uint)Keys.A) && (key <= (uint)Keys.Z))
                return offset + key - (uint)Keys.A;
            offset += (1 + (uint)Keys.Z - (uint)Keys.A);

            // Handles numpad numbers keys:
            if ((key >= (uint)Keys.N0) && (key <= (uint)Keys.N9))
                return offset + key - (uint)Keys.N0;
            offset += (1 + (uint)Keys.N9 - (uint)Keys.N0);

            // Flag used to get key code count:
            if (key == 0xFFFFFFFF)
                return offset - 1;

            return 0;
        }

        public static uint getKeyHashCode(ShortcutData.Keys key)
        {
            return getKeyHashCode((uint)key);
        }

        public static uint getKeyCodeCount()
        {
            return getKeyHashCode(0xFFFFFFFF);
        }

        public uint getKeyHashCode()
        {
            return getKeyHashCode((uint)Key);
        }

        public bool isSpecial()
        {
            // Signals CTRL + ALT + Escape reserved shortcut (reset):
            if (((Modifier & (ShortcutData.Modifiers.X_ALT | ShortcutData.Modifiers.X_CTRL)) != 0)
             && ((Modifier & ~(ShortcutData.Modifiers.X_ALT | ShortcutData.Modifiers.X_CTRL)) == 0)
             && (Key == ShortcutData.Keys.Esc))
                return true;
            // Prevents the user to load CTRL + ALT + C reserved shortcut (exit):
            if (((Modifier & (ShortcutData.Modifiers.X_ALT | ShortcutData.Modifiers.X_CTRL)) != 0)
             && ((Modifier & ~(ShortcutData.Modifiers.X_ALT | ShortcutData.Modifiers.X_CTRL)) == 0)
             && (Key == ShortcutData.Keys.C))
                return true;

            return false;
        }

        public bool isValid()
        {
            // Prevents a shortcut with no key to load:
            if (Key == Keys.None)
                return false;

            // Prevents an empty shortcut to load:
            if ((Class == null) || (Class == String.Empty))
                return false;
            if ((Method == null) || (Method == String.Empty))
                return false;
            return true;
        }

        public string keyCombination()
        {
            Array modifiers = Enum.GetValues(typeof(Modifiers));
            string ret = String.Empty;

            for (int i = 2; i < modifiers.Length; i += 3) {
                if ((Modifier & (Modifiers) modifiers.GetValue(i)) == (Modifiers) modifiers.GetValue(i))
                    ret += ((Modifiers) modifiers.GetValue(i) + "+");
                else if ((Modifier & (Modifiers) modifiers.GetValue(i - 1)) == (Modifiers) modifiers.GetValue(i - 1))
                    ret += ((Modifiers) modifiers.GetValue(i - 1) + "+");
                else if ((Modifier & (Modifiers)modifiers.GetValue(i - 2)) == (Modifiers)modifiers.GetValue(i - 2))
                    ret += ((Modifiers)modifiers.GetValue(i - 2) + "+");
            }
            ret += Key;

            return ret;
        }

        public string action()
        {
            string ret = String.Empty;

            if ((Class != null) && (Class != String.Empty)) {
                ret += Class;
                if ((Class != null) && (Class != String.Empty))
                    ret += ("." + Method + "(" + String.Join<string>(",", Params) + ")");
            }

            return ret;
        }

        public override string ToString()
        {
            return (keyCombination() + " -> " + action());
        }

        public ShortcutData Clone()
        {
            return new ShortcutData() {
                Modifier = this.Modifier,
                Key = this.Key,
                Id = 0,
                Class = this.Class,
                Method = this.Method
            };
        }

        public override int GetHashCode()
        {
            return (int)Modifier + (int)Key + Class.GetHashCode() + Method.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            ShortcutData shortcut = obj as ShortcutData;
            if (shortcut == null)
                return false;

            return ((shortcut.Modifier == Modifier) && (shortcut.Key == Key) && (shortcut.Class == Class) && (shortcut.Method == Method));
        }
    }
}
