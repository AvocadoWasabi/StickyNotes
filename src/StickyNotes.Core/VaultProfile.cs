namespace StickyNotes.Core;

public sealed class VaultProfile
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = "";
    public bool UseCli { get; set; } = true;
    public string Folder { get; set; } = "";
    public string CliFolder { get; set; } = "";
    public string LocalRoot { get; set; } = "";
    public string CliTarget { get; set; } = "";
    public string NotesFolder { get; set; } = "";
    public string? ObsidianNotesFolder { get; set; }
    public string DailyFolder { get; set; } = "";
    public string DailyPattern { get; set; } = DailyNoteResolver.RegexExample;
    public DailyNoteRetention DailyRetention { get; set; }
    public VaultProfile Copy() => (VaultProfile)MemberwiseClone();
    public override string ToString() => Name;
}

public sealed partial class Settings
{
    public const string MainVaultId = "main";
    public string MainVaultName { get; set; } = "";
    public string DefaultDailyVaultId { get; set; } = MainVaultId;
    private List<VaultProfile> additionalVaults = [];
    public List<VaultProfile> AdditionalVaults { get => additionalVaults; set => additionalVaults = value ?? []; }

    // The original fields remain the main profile's backing store for compatible migration.
    public List<VaultProfile> GetVaultProfiles() => [new()
    {
        Id = MainVaultId, Name = string.IsNullOrWhiteSpace(MainVaultName) ? L10n.Text("Vault.Main") : MainVaultName,
        UseCli = ObsidianTasksEnabled,
        Folder = ObsidianTasksEnabled ? ObsidianVaultFolder : string.IsNullOrWhiteSpace(NotesRoot) ? NotesFolder : NotesRoot,
        CliFolder = ObsidianVaultFolder, LocalRoot = string.IsNullOrWhiteSpace(NotesRoot) ? NotesFolder : NotesRoot,
        CliTarget = ObsidianVaultId, NotesFolder = NotesFolder, ObsidianNotesFolder = ObsidianNotesFolder,
        DailyFolder = DailyFolder, DailyPattern = DailyPattern, DailyRetention = DailyRetention
    }, .. AdditionalVaults.Select(p => p.Copy())];

    public string VaultName(string id) => GetVaultProfiles().FirstOrDefault(v => v.Id == id)?.Name ?? L10n.Text("Vault.Missing");

    public Settings ForVault(string id)
    {
        if (id == MainVaultId) return this;
        var profile = AdditionalVaults.SingleOrDefault(p => p.Id == id)
            ?? throw new InvalidOperationException(L10n.Text("Vault.Missing"));
        var result = (Settings)MemberwiseClone();
        result.SetSource(profile);
        return result;
    }

    public void SetVaultProfiles(IEnumerable<VaultProfile> profiles)
    {
        var all = profiles.Select(p => p.Copy()).ToArray();
        if (all.Count(p => p.Id == MainVaultId) != 1 || all.Any(p => string.IsNullOrWhiteSpace(p.Id) || string.IsNullOrWhiteSpace(p.Name)) ||
            all.Select(p => p.Id).Distinct(StringComparer.Ordinal).Count() != all.Length)
            throw new InvalidOperationException(L10n.Text("Vault.Invalid"));
        var main = all.Single(p => p.Id == MainVaultId);
        if (!all.Any(p => p.Id == DefaultDailyVaultId)) throw new InvalidOperationException(L10n.Text("Vault.Missing"));
        MainVaultName = main.Name.Trim(); SetSource(main);
        AdditionalVaults = all.Where(p => p.Id != MainVaultId).ToList();
    }

    private void SetSource(VaultProfile profile)
    {
        ObsidianTasksEnabled = profile.UseCli;
        ObsidianVaultFolder = profile.UseCli ? profile.Folder : profile.CliFolder;
        ObsidianVaultId = profile.CliTarget;
        NotesRoot = profile.UseCli ? profile.LocalRoot : profile.Folder;
        NotesFolder = profile.NotesFolder;
        ObsidianNotesFolder = profile.ObsidianNotesFolder;
        DailyFolder = profile.DailyFolder;
        DailyPattern = DailyNotePatternMigration.Upgrade(profile.DailyPattern);
        DailyRetention = profile.DailyRetention;
    }

    public static bool SameSource(Settings first, Settings second) =>
        first.ObsidianTasksEnabled == second.ObsidianTasksEnabled &&
        (!first.ObsidianTasksEnabled ||
         string.Equals(first.ObsidianVaultFolder.TrimEnd('\\', '/'), second.ObsidianVaultFolder.TrimEnd('\\', '/'), StringComparison.OrdinalIgnoreCase) &&
         string.Equals(first.ObsidianVaultId, second.ObsidianVaultId, StringComparison.Ordinal));

    // Capture this at read time so changing a connection cannot redirect a stale write.
    public string SourceIdentity(bool daily = false) => System.Text.Json.JsonSerializer.Serialize(new
    {
        ObsidianTasksEnabled,
        Cli = ObsidianTasksEnabled ? ObsidianCli : "",
        Root = ObsidianTasksEnabled ? ObsidianVaultFolder.TrimEnd('\\', '/').ToUpperInvariant() : "",
        Target = ObsidianTasksEnabled ? ObsidianVaultId : "",
        DailyFolder = daily ? DailyFolder : "", DailyPattern = daily ? DailyPattern : ""
    });
}
