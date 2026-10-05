using System.Windows.Shell;
using Forms = System.Windows.Forms;

namespace StickyNotes;

internal sealed record AppCommand(byte Code, string Id, string Title, bool SeparatorBefore = false);

internal static class AppCommands
{
    internal static IReadOnlyList<AppCommand> All { get; } = Array.AsReadOnly(new[]
    {
        new AppCommand(1, "new", "新しい付箋"),
        new AppCommand(2, "open", "Markdownを開く…"),
        new AppCommand(3, "link-section", "ノートの一部分を付箋にする…"),
        new AppCommand(4, "link-daily", "デイリーノートを表示…"),
        new AppCommand(5, "show-all", "すべて表示"),
        new AppCommand(6, "temporary-front", "一時的に付箋を最前面に表示する（10秒間）"),
        new AppCommand(7, "settings", "設定…"),
        new AppCommand(8, "exit", "終了", true)
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
