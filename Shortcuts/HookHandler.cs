/* Copyright 2015-2016 Pascal COMBES <pascom@orange.fr>
 * 
 * This file is part of GlobalHotKeys.
 * 
 * GlobalHotKeys is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * GlobalHotKeys is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with GlobalHotKeys. If not, see <http://www.gnu.org/licenses/>
 */

﻿using System;
using System.Threading;

using log4net;

namespace GlobalHotKeys
{
    namespace Shortcuts
    {
        /// <summary>
        ///     Handles the events coming from the low-level keyboard hook.
        /// </summary>
        /// <para>
        /// </para>
        class HookHandler
        {
            /// <summary>
            ///     Logger for GlobalHokKeys.
            /// </summary>
            /// <remarks>See Apache Log4net documentation for the logging interface.</remarks>
            static private readonly ILog log = LogManager.GetLogger(typeof(HookHandler));

            /// <summary>
            ///     Handle to the hook.
            /// </summary>
            /// <seealso cref="User32.SetWindowsHookEx"/>
            /// <seealso cref="User32.UnhookWindowsHookEx"/>
            private IntPtr mHookHandle;
            /// <summary>
            ///     Pointer to the callback to the hook.
            /// </summary>
            /// <para>
            ///     This is required so that the garbage collector do not remove the callback pointer from the memory.
            /// </para>
            private User32.HookProc mHookCallbackHandle;
            /// <summary>
            ///     States of the modifiers
            /// </summary>
            private ModifierStates[] mModifierStates;
            /// <summary>
            ///     Saved states of the modifiers
            /// </summary>
            /// <remarks>
            ///     The modifiers are saved when the key is pressed, 
            ///     so that if the modifiers are released before the key is released, the right shortcut is invoked.
            ///     But the shortcut is called when the key is released.
            /// </remarks>
            private ModifierStates[] mSavedModifierStates;
            /// <summary>
            ///     Semaphore indicating if a modifer is still pressed.
            /// </summary>
            /// <seealso cref="waitModifiersReleased"/>
            private Semaphore mModifierSemaphore;
            /// <summary>
            ///     Mutex to protect the has table of registered shortcuts.
            /// </summary>
            /// <seealso cref="mKeyCombinations"/>
            private Mutex mKeyCombinationsMutex;
            /// <summary>
            ///     The hash table of registered shortcuts.
            /// </summary>
            /// <remarks>
            ///     It is protected by the mutex <see cref="mKeyCombinationsMutex"/>.
            /// </remarks>
            private int[] mKeyCombinations;

            /// <summary>
            ///     Local enum for the modifiers.
            /// </summary>
            private enum ModifierIndexes
            {
                /// <summary>ALT modifier</summary>
                ALT = 0,
                /// <summary>CTRL modifier</summary>
                CTRL,
                /// <summary>SHIFT modifier</summary>
                SHIFT,
                /// <summary>META (windows) modifier</summary>
                META,
                /// <summary>Modifier value count (must be last one)</summary>
                Count
            }

            /// <summary>
            ///     Local enum for modifier states
            /// </summary>
            private enum ModifierStates
            {
                /// <summary>No modifier pressed</summary>
                None = 0x0,
                /// <summary>Right modifier pressed</summary>
                Right = 0x1,
                /// <summary>Left modifier pressed</summary>
                Left = 0x2,
                /// <summary>Both modifier pressed</summary>
                Both = 0x3
            }

            /// <summary>
            ///     Virtual key codes for the modifier
            /// </summary>
            /// <seealso cref="ModifierScanCodes"/>
            private enum ModifierVirtualKeyCodes : uint
            {
                /// <summary>Left META modifier key code</summary>
                L_META = 0x5B,
                /// <summary>Right META modifier key code</summary>
                R_META = 0x5C,
                /// <summary>Left SHIFT modifier key code</summary>
                L_SHIFT = 0xA0,
                /// <summary>Right SHIFT modifier key code</summary>
                R_SHIFT = 0xA1,
                /// <summary>Left CTRL modifier key code</summary>
                L_CTRL = 0xA2,
                /// <summary>Right CTRL modifier key code</summary>
                R_CTRL = 0xA3,
                /// <summary>Left ALT modifier key code</summary>
                L_ALT = 0xA4,
                /// <summary>Right ALT modifier key code</summary>
                R_ALT = 0xA5
            }

            /// <summary>
            ///     Scan codes of the modifier
            /// </summary>
            /// <seealso cref="ModifierVirtualKeyCodes"/>
            private enum ModifierScanCodes : uint
            {
                /// <summary>CTRL modifier scan code (identical for left and right CTRL key)</summary>
                CTRL = 0x1D,
                /// <summary>Left SHIFT modifier scan code</summary>
                L_SHIFT = 0x2A,
                /// <summary>Right SHIFT modifier scan code</summary>
                R_SHIFT = 0x36,
                /// <summary>ALT modifier scan code (identical for left and right ALT key)</summary>
                ALT = 0x38,
                /// <summary>Left META modifier scan code</summary>
                L_META = 0x5B,
                /// <summary>Right META modifier scan code</summary>
                R_META = 0x5C
            }

            /// <summary>
            ///     Constructor
            /// </summary>
            /// <para>
            ///     Initialises the members and registers the hook.
            /// </para>
            public HookHandler()
            {
                mModifierSemaphore = new Semaphore(1, 1);
                mModifierStates = new ModifierStates[(uint)ModifierIndexes.Count];

                for (uint i = 0; i < (uint)ModifierIndexes.Count; i++)
                    mModifierStates[i] = ModifierStates.None;

                mKeyCombinationsMutex = new Mutex();
                mKeyCombinations = new int[ShortcutData.getKeyCodeCount() * 256];
                reset();

                mHookCallbackHandle = new User32.HookProc(keyboardLowLevelHookCallback);
                mHookHandle = User32.SetWindowsHookEx(User32.HookType.WH_KEYBOARD_LL, mHookCallbackHandle, IntPtr.Zero, 0);

                if (mHookHandle == IntPtr.Zero) {
                    log.ErrorFormat("SetWindowsHookEx error. Code: {0}", User32.GetLastError());
                    throw new ApplicationException("Installing hook failed. See previous message for the code");
                }
            }
            /// <summary>
            ///     Destructor
            /// </summary>
            /// <para>
            ///     Unregisters the hook and allows the garbage collector to delete the callback pointer.
            /// </para>
            ~HookHandler()
            {
                if (!User32.UnhookWindowsHookEx(mHookHandle))
                    log.ErrorFormat("UnhookWindowsHookEx error. Code: {0}", User32.GetLastError());
                mHookCallbackHandle = null;
            }

            /// <summary>
            ///     Wait until the modifiers are released.
            /// </summary>
            /// <para>
            ///     Wait until the modifiers have been released or timeout has elapsed (it it is positive). 
            /// </para>
            /// <param name="timeout">Time to wait before returning. Negative to wait forever. 0 to test but not wait.</param>
            /// <returns><c>true</c> if the modifiers have been released</returns>
            public bool waitModifiersReleased(Int32 timeout)
            {
                bool ret = mModifierSemaphore.WaitOne(timeout);
                log.Debug("Modifiers mutex released.");
                if (ret)
                    mModifierSemaphore.Release();
                return ret;
            }

            /// <summary>
            ///     The hook callback.
            /// </summary>
            /// <para>
            ///     If code is negative nothing is done and the next hook is called with <see cref="CallNextHookEx"/>.
            ///     Then handles the modifiers state management. Then it locks the key combination mutex and looks for a matching shortcut.
            ///     Finally it posts a <c>WM_HOTKEY</c> message to the shortcut <see cref="Handler"/> which calls the method in another thread.
            /// </para>
            /// <param name="code">A code the hook procedure uses to determine how to process the message</param>
            /// <param name="wParam">Additional message-specific information (see MSDN for a specific message)</param>
            /// <param name="lParam">Additional message-specific information (see MSDN for a specific message)</param>
            /// <returns>Non-zero to prevent further processing</returns>
            public IntPtr keyboardLowLevelHookCallback(int code, IntPtr wParam, IntPtr lParam)
            {
                if (code < 0)
                    return User32.CallNextHookEx(IntPtr.Zero, code, wParam, lParam);

                bool up;
                User32.WindowsMessage wm = (User32.WindowsMessage)wParam;
                User32.KeyboardLowLevelHookStruct keyboardLowLevelData = User32.getKeyboardLowLevelHookStruct(lParam);

                string debugMessage = String.Empty;
                switch (wm) {
                case User32.WindowsMessage.KEYDOWN:
                    debugMessage = "Key down event:";
                    up = false;
                    break;
                case User32.WindowsMessage.KEYUP:
                    debugMessage = "Key up event:";
                    up = true;
                    break;
                case User32.WindowsMessage.SYSKEYDOWN:
                    debugMessage = "System key down event:";
                    up = false;
                    break;
                case User32.WindowsMessage.SYSKEYUP:
                    debugMessage = "System key up event:";
                    up = true;
                    break;
                default:
                    log.WarnFormat("Unhandled message: {0}", wm);
                    return User32.CallNextHookEx(IntPtr.Zero, code, wParam, lParam);
                }

                log.DebugFormat(debugMessage + "vk: {0}, sc: {1}", keyboardLowLevelData.vkCode, keyboardLowLevelData.scanCode);

                try {
                    // Modifiers handling:
                    if (Enum.IsDefined(typeof(ModifierVirtualKeyCodes), keyboardLowLevelData.vkCode)) {
                        // This is a modifier with an non standard scan code. Ignored.
                        if (!Enum.IsDefined(typeof(ModifierScanCodes), keyboardLowLevelData.scanCode)) {
                            log.InfoFormat("The scan code ({0}) is not a modifier scan code", keyboardLowLevelData.scanCode);
                            return User32.CallNextHookEx(IntPtr.Zero, code, wParam, lParam);
                        }

                        ModifierVirtualKeyCodes virtualModifier = (ModifierVirtualKeyCodes)keyboardLowLevelData.vkCode;
                        ModifierScanCodes scanModifier = (ModifierScanCodes)keyboardLowLevelData.scanCode;

                        // Modifier virtual code and scan code don't matche. Ignored.
                        if (!virtualModifier.ToString().EndsWith(scanModifier.ToString())) {
                            log.WarnFormat("Scan code and virtual key code don't match ({0} != {1})", virtualModifier, scanModifier);
                            return User32.CallNextHookEx(IntPtr.Zero, code, wParam, lParam);
                        }

                        switch (virtualModifier) {
                        case ModifierVirtualKeyCodes.R_ALT:
                            updateModifierState(ModifierIndexes.ALT, ModifierStates.Right, up);
                            break;
                        case ModifierVirtualKeyCodes.L_ALT:
                            updateModifierState(ModifierIndexes.ALT, ModifierStates.Left, up);
                            break;
                        case ModifierVirtualKeyCodes.R_CTRL:
                            updateModifierState(ModifierIndexes.CTRL, ModifierStates.Right, up);
                            break;
                        case ModifierVirtualKeyCodes.L_CTRL:
                            updateModifierState(ModifierIndexes.CTRL, ModifierStates.Left, up);
                            break;
                        case ModifierVirtualKeyCodes.R_SHIFT:
                            updateModifierState(ModifierIndexes.SHIFT, ModifierStates.Right, up);
                            break;
                        case ModifierVirtualKeyCodes.L_SHIFT:
                            updateModifierState(ModifierIndexes.SHIFT, ModifierStates.Left, up);
                            break;
                        case ModifierVirtualKeyCodes.R_META:
                            updateModifierState(ModifierIndexes.META, ModifierStates.Right, up);
                            break;
                        case ModifierVirtualKeyCodes.L_META:
                            updateModifierState(ModifierIndexes.META, ModifierStates.Left, up);
                            break;
                        default:
                            break;
                        }

                        log.DebugFormat("Modifiers state: ALT {0}, CTRL {1}, SHIFT {2}, META {3}", mModifierStates[(uint)ModifierIndexes.ALT], mModifierStates[(uint)ModifierIndexes.CTRL], mModifierStates[(uint)ModifierIndexes.SHIFT], mModifierStates[(uint)ModifierIndexes.META]);

                        foreach (ModifierStates state in mModifierStates) {
                            if (state != ModifierStates.None) {
                                log.DebugFormat("Semaphore returned: {0}", mModifierSemaphore.WaitOne(0));
                                return User32.CallNextHookEx(IntPtr.Zero, code, wParam, lParam);
                            }
                        }

                        // Prevents the semaphore from being full... in case a modifier is released 2 times!?
                        mModifierSemaphore.WaitOne(0);
                        mModifierSemaphore.Release();
                        log.Debug("Unlocked modifier semaphore");
                        return User32.CallNextHookEx(IntPtr.Zero, code, wParam, lParam);
                    }

                    int id = 0;
                    uint keyHash = ShortcutData.getKeyHashCode(keyboardLowLevelData.vkCode);
                    // The key is not known. Ignoring shortcut.
                    if (keyHash == 0)
                        return User32.CallNextHookEx(IntPtr.Zero, code, wParam, lParam);

                    /* Shortcuts are executed when releasing the key but the modifiers are saved when pressing the key
                     * So that even when the user releases the key in a bad order, the command is executed 
                     */
                    if (!up)
                        mSavedModifierStates = (ModifierStates[])mModifierStates.Clone();

                    if (mSavedModifierStates == null)
                        mSavedModifierStates = mModifierStates;

                    /* Tries to lock the mutex so that the other thread cannot modify the list of keycombinations.
                     * If the attempt does not succeed, then the mutex is owned by the other thread.
                     * In this case it should not wait and the shortcut is ignored.
                     */
                    if (!mKeyCombinationsMutex.WaitOne(10))
                        return User32.CallNextHookEx(IntPtr.Zero, code, wParam, lParam);

                    // TODO: This is not optimal.
                    foreach (int modifierFlag in new int[] { 0, 1, 2, 4, 8, 3, 5, 9, 6, 10, 12, 7, 11, 13, 14, 15 }) {
                        uint modifierHash = 0;
                        for (int m = 0; m < (uint)ModifierIndexes.Count; m++) {
                            if ((modifierFlag & (1 << m)) == 0)
                                modifierHash |= ((uint)(mSavedModifierStates[m] | mModifierStates[m]) << 2 * m);
                            else
                                modifierHash |= (((mSavedModifierStates[m] | mModifierStates[m]) != ModifierStates.None ? (uint)3 : (uint)0) << 2 * m);
                        }

                        id = mKeyCombinations[modifierHash + 256 * (keyHash - 1)];
                        if (id != 0)
                            break;
                    }

                    mKeyCombinationsMutex.ReleaseMutex();

                    // No shortcut associated with this key combination:
                    if (id == 0)
                        return User32.CallNextHookEx(IntPtr.Zero, code, wParam, lParam);
                    // Key was pressed. Shortcut are executed when key is released:
                    if (!up)
                        return User32.SUCCESS;

                    uint messageModifierHash = 0;
                    for (int m = 0; m < (uint)ModifierIndexes.Count; m++)
                        messageModifierHash |= (((mSavedModifierStates[m] | mModifierStates[m]) != ModifierStates.None ? (uint)1 : (uint)0) << m);

                    // Clears saved modifiers state:
                    mSavedModifierStates = null;

                    log.Info("Got shortcut id=" + id);

                    if (!User32.PostMessage(IntPtr.Zero, User32.WM_HOTKEY, new IntPtr(id), new IntPtr(((int)messageModifierHash << 32) + keyboardLowLevelData.vkCode))) {
                        log.ErrorFormat("PostMessage error. Code: {0}", User32.GetLastError());
                        log.Info("Ignoring shortcut");
                        return User32.CallNextHookEx(IntPtr.Zero, code, wParam, lParam);
                    }
                    return User32.SUCCESS;
                } catch (Exception e) {
                    log.Fatal("Unhandled exception in low-level keyboard hook", e);
                    return User32.CallNextHookEx(IntPtr.Zero, code, wParam, lParam);
                }
            }
            /// <summary>
            ///     Restes the registered shortcut list.
            /// </summary>
            public void reset()
            {
                mKeyCombinationsMutex.WaitOne();
                for (uint i = 0; i < ShortcutData.getKeyCodeCount() * 256; i++)
                    mKeyCombinations[i] = 0;
                mKeyCombinationsMutex.ReleaseMutex();
            }
            /// <summary>
            ///     Load a shortcut in the hash table of registed shortcuts
            /// </summary>
            /// <param name="modifier">The modifier of the shortcut</param>
            /// <param name="key">The key code of the shortcut</param>
            /// <param name="id">The id of the shortcut</param>
            /// <seealso cref="unloadShortcut"/>
            public void loadShortcut(ShortcutData.Modifiers modifier, ShortcutData.Keys key, int id)
            {
                uint index = hash(modifier, key);

                mKeyCombinationsMutex.WaitOne();
                if (mKeyCombinations[index] != 0) {
                    mKeyCombinationsMutex.ReleaseMutex();
                    throw new InvalidShortcutException("A shortcut with the same modifier (" + modifier + ") and the same key (" + key + ") already exists");
                }
                mKeyCombinations[index] = id;
                mKeyCombinationsMutex.ReleaseMutex();
            }
            /// <summary>
            ///     Unload a shortcut from the hash table of registed shortcuts
            /// </summary>
            /// <param name="modifier">The modifier of the shortcut</param>
            /// <param name="key">The key code of the shortcut</param>
            /// <seealso cref="loadShortcut"/>
            public void unloadShortcut(ShortcutData.Modifiers modifier, ShortcutData.Keys key)
            {
                uint index = hash(modifier, key);

                mKeyCombinationsMutex.WaitOne();
                if (mKeyCombinations[index] == 0) {
                    mKeyCombinationsMutex.ReleaseMutex();
                    throw new InvalidShortcutException("The shortcut with modifier (" + modifier + ") and  key (" + key + ") does not exist");
                }

                mKeyCombinations[index] = 0;
                mKeyCombinationsMutex.ReleaseMutex();
            }
            /// <summary>
            ///     Computes the hash code for a shortcut.
            /// </summary>
            /// <param name="modifier">The modifier of the shortcut</param>
            /// <param name="key">The key code of the shortcut</param>
            /// <returns>The hash code of the shorctut</returns>
            private uint hash(ShortcutData.Modifiers modifier, ShortcutData.Keys key)
            {
                if (ShortcutData.getKeyHashCode(key) == 0)
                    throw new InvalidShortcutException("Cannot register a shortcut with no key");

                return (uint)modifier + (ShortcutData.getKeyHashCode(key) - 1) * 256;
            }
            /// <summary>
            ///     Update the array of modifier state.
            /// </summary>
            /// <param name="index">The index of the modifier</param>
            /// <param name="side">The state of the modifier</param>
            /// <param name="released">Whether the modifier was released or pressed</param>
            private void updateModifierState(ModifierIndexes index, ModifierStates side, bool released)
            {
                if (released)
                    mModifierStates[(uint)index] &= (~side);
                else
                    mModifierStates[(uint)index] |= side;
            }
        }
    }
}