using System.Windows.Shell;
using Forms = System.Windows.Forms;

namespace StickyNotes;

internal sealed record AppCommand(byte Code, string Id, string Title, bool SeparatorBefore = false);

internal static class AppCommands
{
    internal static IReadOnlyList<AppCommand> All => Array.AsReadOnly(new[]
    {
        new AppCommand(1, "new", L10n.Text("AppCommands.Text01")),
        new AppCommand(2, "open", L10n.Text("AppCommands.Text02")),
        new AppCommand(3, "link-section", L10n.Text("AppCommands.Text03")),
        new AppCommand(4, "link-daily", L10n.Text("AppCommands.Text04")),
        new AppCommand(5, "show-all", L10n.Text("AppCommands.Text05")),
        new AppCommand(6, "temporary-front", L10n.Text("AppCommands.Text06")),
        new AppCommand(7, "settings", L10n.Text("AppCommands.Text07")),
        new AppCommand(8, "exit", L10n.Text("AppCommands.Text08"), true)
    });

    internal static AppCommand? Parse(string[] args) => args.Length == 2 && args[0] == "--command"
        ? All.FirstOrDefault(command => command.Id == args[1]) : null;

    internal static Forms.ContextMenuStrip CreateTrayMenu(Action<AppCommand> execute)
    {
        var menu = new Forms.ContextMenuStrip();
        foreach (var command in All)
        {
            if (command.SeparatorBefore) menu.Items.Add(new Forms.ToolStripSeparator());
            menu.Items.Add(command.Title, null, (_, _) => execute(command));
        }
        return menu;
    }

    internal static JumpList CreateJumpList(string executable)
    {
        var list = new JumpList { ShowRecentCategory = false, ShowFrequentCategory = false };
        foreach (var command in All)
        {
            if (command.SeparatorBefore) list.JumpItems.Add(new JumpTask());
            list.JumpItems.Add(new JumpTask
            {
                Title = command.Title, Description = command.Title,
                ApplicationPath = executable, Arguments = "--command " + command.Id,
                IconResourcePath = executable, IconResourceIndex = 0
            });
        }
        return list;
    }
}
