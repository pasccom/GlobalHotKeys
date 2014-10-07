using System;
using System.Threading;

using log4net;

namespace GlobalHotKeys
{
    namespace Shortcuts
    {
        class HookHandler
        {
            static private readonly ILog log = LogManager.GetLogger(typeof(HookHandler));

            private IntPtr mHookHandle;
            private User32.HookProc mHookCallbackHandle;
            private ModifierStates[] mModifierStates;
            private ModifierStates[] mSavedModifierStates;

            private Mutex mKeyCombinationsMutex;
            private int[] mKeyCombinations;

            private enum ModifierIndexes
            {
                ALT = 0,
                CTRL,
                SHIFT,
                META,
                Count 
            }

            private enum ModifierStates
            {
                None = 0x0,
                Right = 0x1,
                Left = 0x2,
                Both = 0x3
            }

            private enum ModifierVirtualKeyCodes : uint
            {
                L_META = 0x5B,
                R_META = 0x5C,
                L_SHIFT = 0xA0,
                R_SHIFT = 0xA1,
                L_CTRL = 0xA2,
                R_CTRL = 0xA3,
                L_ALT = 0xA4,
                R_ALT = 0xA5
            }

            private enum ModifierScanCodes : uint
            {
                CTRL = 0x1D,
                L_SHIFT = 0x2A,
                R_SHIFT = 0x36,
                ALT = 0x38,
                L_META = 0x5B,
                R_META = 0x5C
            }

            public HookHandler()
            {
                mModifierStates = new ModifierStates[(uint) ModifierIndexes.Count];

                for (uint i = 0; i < (uint)ModifierIndexes.Count; i++)
                    mModifierStates[i] = ModifierStates.None;

                mKeyCombinationsMutex = new Mutex();
                mKeyCombinations = new int[ShortcutData.getKeyCodeCount() * 256];
                reset();

                mHookCallbackHandle = new User32.HookProc(keyboardLowLevelHookCallback);
                mHookHandle = User32.SetWindowsHookEx(User32.HookType.WH_KEYBOARD_LL, mHookCallbackHandle, IntPtr.Zero, 0);
            }

            ~HookHandler()
            {
                User32.UnhookWindowsHookEx(mHookHandle);
                mHookCallbackHandle = null;
            }

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

                        switch(virtualModifier)
                        {
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
                        mSavedModifierStates = (ModifierStates[]) mModifierStates.Clone();

                    if (mSavedModifierStates == null)
                        mSavedModifierStates = mModifierStates;

                    /* Tries to lock the mutex so that the other thread cannot modify the list of keycombinations.
                     * If the attempt does not succeed, then the mutex is owned by the other thread.
                     * In this case it should not wait and the shortcut is ignored.
                     */
                    if(!mKeyCombinationsMutex.WaitOne(0))
                        return User32.CallNextHookEx(IntPtr.Zero, code, wParam, lParam);

                    // TODO: This is not optimal.
                    foreach (int modifierFlag in new int[] { 0, 1, 2, 4, 8, 3, 5, 9, 6, 10, 12, 7, 11, 13, 14, 15 }) {
                        uint modifierHash = 0;
                        for (int m = 0; m < (uint) ModifierIndexes.Count; m++) {
                            if ((modifierFlag & (1 << m)) == 0)
                                modifierHash |= ((uint) (mSavedModifierStates[m] | mModifierStates[m]) << 2 * m);
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
                        messageModifierHash |= (((mSavedModifierStates[m]| mModifierStates[m])  != ModifierStates.None ? (uint)1 : (uint)0) << m);

                    // Clears saved modifiers state:
                    mSavedModifierStates = null;

                    log.Info("Got shortcut id=" + id);

                    User32.PostMessage(IntPtr.Zero, User32.WM_HOTKEY, new IntPtr(id), new IntPtr(((int)messageModifierHash << 32) + keyboardLowLevelData.vkCode));
                    return User32.SUCCESS;
            }



            public void reset()
            {
                mKeyCombinationsMutex.WaitOne();
                for (uint i = 0; i < ShortcutData.getKeyCodeCount() * 256; i++)
                    mKeyCombinations[i] = 0;
                mKeyCombinationsMutex.ReleaseMutex();
            }

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

            private uint hash(ShortcutData.Modifiers modifier, ShortcutData.Keys key)
            {
                if (ShortcutData.getKeyHashCode(key) == 0)
                    throw new InvalidShortcutException("Cannot register a shortcut with no key");

                return (uint)modifier + (ShortcutData.getKeyHashCode(key) - 1) * 256;
            }

            private void updateModifierState(ModifierIndexes index, ModifierStates side, bool released)
            {
                if (released)
                    mModifierStates[(uint) index] &= (~side);
                else
                    mModifierStates[(uint) index] |= side;
            }
        }
    }
}