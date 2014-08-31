using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlobalHotKeys
{
    class Program
    {
        static void Main(string[] args)
        {
            string path;
            if (args.Length > 1)
                path = args[0];
            else
                path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Pascom\GlobalHotKeys\globalhotkeys.conf";

            Shortcut test = new Shortcut();

            test.setKey("A");
            Console.WriteLine(test.Key);

            test.addModifier(Shortcut.Modifiers.ALT);
            Console.WriteLine(test.Modifier);

            test.addModifier(Shortcut.Modifiers.CTRL);
            Console.WriteLine(test.Modifier);

            test.removeModifier(Shortcut.Modifiers.ALT);
            Console.WriteLine(test.Modifier);

            Console.WriteLine(test);

            Console.WriteLine(path);
            ConfigProvider config = new PlainTextConfig(path);
            config.NewShortcutEvent += (Shortcut shortcut) => Console.WriteLine("New shortcut: " + shortcut);
            config.parseConfig();

            Shortcuts.Handler app = Shortcuts.Handler.getInstance(new PlainTextConfig(path));
            app.exec();
        }
    }
}
