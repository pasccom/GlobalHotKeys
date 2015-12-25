using System;
using System.Reflection;
using System.Collections.Generic;

namespace GlobalHotKeys
{
    /// <summary>
    ///     Describes a shortcut.
    /// </summary>
    /// <para>
    ///     This class describes a shortcut and defines the functions to create and work with shortcuts.
    ///     It thus contains the key combination which triggers the shortcut (<see cref="Modifier"/> and <see cref="Key"/>)
    ///     as well as the needed information to invoke the method (<see cref="Class"/>, <see cref="Method"/> and <see cref="Params"/>).
    ///     It also defines the list of supported <see cref="Keys"/> and supported <see cref="Modifiers"/> 
    ///     as well as some special default shortcuts (<see cref="exitShortcut"/> and <see cref="resetShortcut"/>).
    /// </para>
    class ShortcutData
    {
        /// <summary>
        ///     Enumeration of all supported modifiers.
        /// </summary>
        /// <para>List of the supported modifiers</para>
        /// <list type="table">
        ///     <item><term>R_ALT</term> <description>The right ALT key is pressed</description></item>
        ///     <item><term>L_ALT</term> <description>The left ALT key is pressed</description></item>
        ///     <item><term>X_ALT</term> <description>Any of the ALT key is pressed</description></item>
        ///     <item><term>R_CTRL</term> <description>The right CTRL key is pressed</description></item>
        ///     <item><term>L_CTRL</term> <description>The left CTRL key is pressed</description></item>
        ///     <item><term>X_CTRL</term> <description>Any of the CTRL key is pressed</description></item>
        ///     <item><term>R_SHIFT</term> <description>The right Shift key is pressed</description></item>
        ///     <item><term>L_SHIFT</term> <description>The left Shift key is pressed</description></item>
        ///     <item><term>X_SHIFT</term> <description>Any of the Shift key is pressed</description></item>
        ///     <item><term>R_META</term> <description>The right "windows" key is pressed</description></item>
        ///     <item><term>L_META</term> <description>The left "windows" key is pressed</description></item>
        ///     <item><term>X_META</term> <description>Any of the "windows" key is pressed</description></item>
        /// </list>
        public enum Modifiers
        {
            R_ALT = 0x01,
            L_ALT = 0x02, X_ALT = 0x03,
            R_CTRL = 0x04, L_CTRL = 0x08, X_CTRL = 0x0C,
            R_SHIFT = 0x10, L_SHIFT = 0x20, X_SHIFT = 0x30,
            R_META = 0x40, L_META = 0x80, X_META = 0xC0
        }

        /// <summary>
        ///     Enumeration of the supported keys. 
        /// </summary>
        /// <para>List of the supported keys</para>
        /// <list type="table">
        ///     <item><term>None</term> <description>No key. The shortcut is invalid.</description></item>
        ///     <item><term>Esc</term> <description>Escape key.</description></item>
        ///     <item><term>[A-Z]</term> <description>The specified letter is pressed.</description></item>
        ///     <item><term>Left</term> <description>The left arrow key is pressed.</description></item>
        ///     <item><term>Up</term> <description>The up arrow key is pressed.</description></item>
        ///     <item><term>Right</term> <description>The right arrow key is pressed.</description></item>
        ///     <item><term>Down</term> <description>The down arrow key is pressed.</description></item>
        ///     <item><term>PgUp</term> <description>The previous page key is pressed.</description></item>
        ///     <item><term>PgDown</term> <description>The next page key is pressed.</description></item>
        ///     <item><term>F[1-12]</term> <description>The specified function key is pressed.</description></item>
        ///     <item><term>N[0-9]</term> <description>The specified numpad key is pressed.</description></item>
        /// </list>
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
        /// <seealso cref="Modifiers" />
        public Modifiers Modifier { get; private set; }
        /// <summary>
        ///     Key of this shortcut.
        /// </summary>
        /// <para> To set the key use the <see cref="setKey"/> method.</para>
        /// <seealso cref="Keys" />
        public Keys Key { get; private set; }
        /// <summary>
        ///     Class of the shortcut.
        /// </summary>
        /// <para> This is the fully-qualified name of the class where the method will be invoked.</para>
        /// <seealso cref="Method" />
        public string Class { get; set; }
        /// <summary>
        ///     Method invoked by the shortcut.
        /// </summary>
        /// <para>The method of the specified class which will be invoked by the shortcut.</para>
        /// <seealso cref="Class" />
        public string Method { get; set; }
        /// <summary>
        ///     The parameters for the method.
        /// </summary>
        /// <para>These parameters will be passed to the method invoked by the shortcut.</para>
        public List<string> Params { get; set; }

        /// <summary>
        ///     Identifier for the shortcut.
        /// </summary>
        /// <remarks>Should be assigned by used if needed.</remarks>
        public int Id { get; set; }

        /// <summary>
        ///     A special virtual shortcut to terminate the application.
        /// </summary>
        /// <para>This is a virtual shortcut. It has no real associated method. 
        /// It is used internally to terminate the application. It cannot be modified.</para>
        public static ShortcutData exitShortcut = new ShortcutData() {
            Modifier = Modifiers.X_ALT | Modifiers.X_CTRL | Modifiers.X_SHIFT,
            Key = Keys.C,
            Class = "Shortcuts.Handler",
            Method = "exit",
        };

        /// <summary>
        ///     A special virtual shortcut to reset the shortcuts of the application.
        /// </summary>
        /// <para>This is a virtual shortcut. It has no real associated method. 
        /// It is used internally to reset the shortcuts of the application to the default ones. 
        /// It cannot be modified.</para>
        public static ShortcutData resetShortcut = new ShortcutData() {
            Modifier = Modifiers.X_ALT | Modifiers.X_CTRL | Modifiers.X_SHIFT,
            Key = Keys.Esc,
            Class = "Shortcuts.Handler",
            Method = "reset",
        };

        /// <summary>
        ///     The default constructor. It will construct an invalid shortcut.
        /// </summary>
        /// <para>This constructor constructs an invalid shortcut with no associated key.</para>
        public ShortcutData()
        {
            Key = Keys.None;
            Id = 0;
            Params = new List<string>();
        }

        /// <summary>
        ///     Adds a modifier to the shortcut.
        /// </summary>
        /// <para>This method is used as a setter for the property <see cref="Modifier" />. 
        /// It adds the given modifier flag to the <see cref="Modifier" /> property flag combination.</para>
        /// <param name="modifier">The modifier to add to this shortcut modfiers.</param>
        /// <seealso cref="Modifiers"/>
        public void addModifier(Modifiers modifier)
        {
            Modifier |= modifier;
        }

        /// <summary>
        ///     Removes a modifier from the shortcut.
        /// </summary>
        /// <para>This method is used as a setter for the property <see cref="Modifier" />. 
        /// It removes the given modifier from the <see cref="Modifier" /> property flag combination.</para>
        /// <param name="modifier">The modifier to remove from this shortcut modifiers.</param>
        /// <seealso cref="Modifiers"/>
        public void removeModifier(Modifiers modifier)
        {
            Modifier &= (~modifier);
        }

        /// <summary>
        ///     Sets the shortcut key.
        /// </summary>
        /// <para>This method acts as a setter for the <see cref="Key"/> property of the shortcut.
        /// It parses the given string to see if it matches a enum constant name.</para>
        /// <param name="key">
        ///     A string representing a key. 
        ///     It should be the name of one of the <see cref="Keys"/> enum.
        /// </param>
        /// <exception cref="ArgumentException">
        ///     If the given string is not a valid key name.
        /// </exception>
        public void setKey(string key)
        {
            Array values = Enum.GetValues(typeof(Keys));
            string[] names = Enum.GetNames(typeof(Keys));

            for (int i = 0; i < names.Length; i++) {
                if (names[i] == key) {
                    Key = (Keys)values.GetValue(i);
                    return;
                }
            }

            throw new ArgumentException("Expected a key name. The key names are the name of the enum Keys");
        }

        /// <summary>
        ///     Converts a virtual key code into an internal key hash code.
        /// </summary>
        /// <param name="key"> The virtual key code to convert into an internal key hash code.</param>
        /// <returns> The internal key hash code corresponding to the virtual key code given.</returns>
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

        /// <summary>
        ///     Converts key enum constant into an internal key hash code.
        /// </summary>
        /// <param name="key"> The <see cref="Keys"/> enum constant to convert into an internal key hash code.</param>
        /// <returns> The internal key hash code corresponding to the <see cref="Keys"/> enum constant given.</returns>
        public static uint getKeyHashCode(ShortcutData.Keys key)
        {
            return getKeyHashCode((uint)key);
        }

        /// <summary>
        ///     Returns the maximum value of an internal key hash code.
        /// </summary>
        /// <returns>The maximum value of a internal kay hash code.</returns>
        public static uint getKeyCodeCount()
        {
            return getKeyHashCode(0xFFFFFFFF);
        }

        /// <summary>
        ///     Returns the internal key hash code of this shortcut.
        /// </summary>
        /// <returns> The internal key hash code of this shortcut. It is 0 for invalid shortcuts.</returns>
        public uint getKeyHashCode()
        {
            return getKeyHashCode((uint)Key);
        }

        /// <summary>
        ///     Whether a shirtcut is special.
        /// </summary>
        /// <returns> true if this is a special shortcut, false otherwise</returns>
        /// <seealso cref="exitShortcut" />
        /// <seealso cref="resetShortcut" />
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

        /// <summary>
        ///     Whether a shortcut is valid.
        /// </summary>
        /// <para> A shortcut is valid if all these conditions old:<list type="bullet">
        ///     <item>One key is associated with the shortcut (<see cref="Key"/> property)</item>
        ///     <item>The class name is not null nor empty (<see cref="Class"/> property)</item>
        ///     <item>The method name is not null nor empty (<see cref="Method"/> property)</item>
        /// </list></para>
        /// <returns></returns>
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

        /// <summary>
        ///     The key combination of the shortcut represented as a string.
        /// </summary>
        /// <para>The returned string is a representation of the shotcut key combination 
        /// under the form <code>[(L|R|X)_ALT+[(L|R|X)_CTRL+[(L|R|X)_SHIFT+[(L|R|X)_META+]]]]Key</code></para>
        /// <returns>The representation of the shortcut key combination.</returns>
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

        /// <summary>
        ///     The action of the shortcut represented as a string.
        /// </summary>
        /// <para>The returned string is a representation of the shortcut action
        /// under the form: <code>Class.Method([Param1[,Param2[,...]]])</code></para>
        /// <returns>The representation of the shortcut action.</returns>
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

        /// <summary>
        ///     A reprensentation of the shortcut as a string.
        /// </summary>
        /// This representation is of the form:
        /// <code>[(L|R|X)_ALT+[(L|R|X)_CTRL+[(L|R|X)_SHIFT+[(L|R|X)_META]]]]Key -> Class.Method([Param1[,Param2[,...]]])</code>
        /// <returns> The string representation of the shortcut.</returns>
        public override string ToString()
        {
            return (keyCombination() + " -> " + action());
        }

        /// <summary>
        ///     Clones the shortcut.
        /// </summary>
        /// <para>Needless to say, the id will not be copied. But all other properties are.</para>
        /// <returns> A copy of the shortcut with all properties except id copied.</returns>
        public ShortcutData Clone()
        {
            return new ShortcutData() {
                Modifier = this.Modifier,
                Key = this.Key,
                Id = 0,
                Class = this.Class,
                Method = this.Method,
                Params = new List<string>(this.Params)
            };
        }

        /// <summary>
        ///     Returns a sensitive hash code for the object.
        /// </summary>
        /// <para> The computation of the hash for a shortcut uses <see cref="Modifier" />, <see cref="Keys" />, <see cref="Class" /> and <see cref="Method"/>.</para>
        /// <remarks>The hash codes of 2 shortcuts are the same if and only if these 2 shortcuts are equal.</remarks>
        /// <returns> A hash code for this shortcut. </returns>
        /// <seealso cref="Equals"/>
        public override int GetHashCode()
        {
            return (int)Modifier + (int)Key + Class.GetHashCode() + Method.GetHashCode();
        }

        /// <summary>
        ///     Whether the given object is equal to the shortcut.
        /// </summary>
        /// <remarks> 2 shortcuts are equal if and only if the hash codes of 2 shortcuts are the same.</remarks>
        /// <param name="obj"> Any oject </param>
        /// <returns> true if the object is a shortcut and it has the same 
        /// <see cref="Modifier"/>, <see cref="Keys"/>,
        /// <see cref="Class"/>, <see cref="Method"/> properties.</returns>
        /// <seealso cref="GetHashCode"/>
        public override bool Equals(object obj)
        {
            ShortcutData shortcut = obj as ShortcutData;
            if (shortcut == null)
                return false;

            return ((shortcut.Modifier == Modifier) && (shortcut.Key == Key) && (shortcut.Class == Class) && (shortcut.Method == Method));
        }
    }
}
