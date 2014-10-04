using System;

namespace GlobalHotKeys
{
    namespace Shortcuts
    {
        class HookHandler
        {
            private int[] mKeyCombinations;

            public HookHandler()
            {
                mKeyCombinations = new int[ShortcutData.getKeyCodeCount()*256];
                reset();
            }

            public void reset()
            {
                for (uint i = 0; i < ShortcutData.getKeyCodeCount() * 256; i++)
                    mKeyCombinations[i] = 0;
            }

            public void loadShortcut(ShortcutData.Modifiers modifier, ShortcutData.Keys key, int id)
            {
                uint index = hash(modifier, key);

                if (mKeyCombinations[index] != 0)
                    throw new InvalidShortcutException("A shortcut with the same modifier (" + modifier + ") and the same key (" + key + ") already exists");

                mKeyCombinations[index] = id;
            }

            public void unloadShortcut(ShortcutData.Modifiers modifier, ShortcutData.Keys key)
            {
                uint index = hash(modifier, key);

                if (mKeyCombinations[index] == 0)
                    throw new InvalidShortcutException("The shortcut with modifier (" + modifier + ") and  key (" + key + ") does not exist");

                mKeyCombinations[index] = 0;
            }

            private uint hash(ShortcutData.Modifiers modifier, ShortcutData.Keys key)
            {
                if (ShortcutData.getKeyHashCode(key) == 0)
                    throw new InvalidShortcutException("Cannot register a shortcut with no key");

                return (uint)modifier + (ShortcutData.getKeyHashCode(key) - 1)*256;
            }
        }
    }
}