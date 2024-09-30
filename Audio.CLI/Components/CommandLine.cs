using System.CommandLine;
using System.CommandLine.Binding;

namespace Audio.CLI.Components;
public static class CommandLine
{
    public static void Init(string[] args)
    {
        RootCommand rootCommand = RegisterOptions();
        rootCommand.Invoke(args);
    }

    public static RootCommand RegisterOptions()
    {
        RootCommand rootCommand = [];
        rootCommand.SetHandler(Program.Run, new OptionsBinder(rootCommand));

        return rootCommand;
    }
}

public record Options
{
    public bool Tags { get; set; }
    public bool Playlist { get; set; }
    public bool Hierarchy { get; set; }
    public bool Bank { get; set; }
    public bool Audio { get; set; }
    public bool Convert { get; set; }
    public FileInfo? Externals { get; set; }
    public FileInfo? Events { get; set; }
    public string? Input { get; set; }
    public DirectoryInfo? Output { get; set; }
}

public class OptionsBinder : BinderBase<Options>
{
    private readonly Option<bool> _tags;
    private readonly Option<bool> _playlist;
    private readonly Option<bool> _hierarchy;
    private readonly Option<bool> _bank;
    private readonly Option<bool> _audio;
    private readonly Option<bool> _convert;
    private readonly Option<FileInfo> _externals;
    private readonly Option<FileInfo> _events;
    private readonly Argument<string> _input;
    private readonly Argument<DirectoryInfo> _output;

    public OptionsBinder(RootCommand rootCommand)
    {
        rootCommand.Add(_tags = new("--tags", "Enable resolving events and matching tags."));
        rootCommand.Add(_playlist = new("--playlist", "Enable dumping playlist.xspf."));
        rootCommand.Add(_hierarchy = new("--hierarchy", "Enable dumping hierarchy structures as json files."));
        rootCommand.Add(_bank = new("--bank", "Enable dumping raw bank files."));
        rootCommand.Add(_audio = new("--audio", "Enable dumping audio files."));
        rootCommand.Add(_convert = new("--convert", "Enable conversion while dumping audio files."));
        rootCommand.Add(_externals = new("--externals", "Path to txt file with list of external names."));
        rootCommand.Add(_events = new("--events", "Path to txt file with list of event names."));
        rootCommand.Add(_input = new("input", "Path to input file/folder."));
        rootCommand.Add(_output = new("output", "Path to output directory."));
    }

    protected override Options GetBoundValue(BindingContext bindingContext) => new() 
    {
        Tags = bindingContext.ParseResult.GetValueForOption(_tags),
        Playlist = bindingContext.ParseResult.GetValueForOption(_playlist),
        Hierarchy = bindingContext.ParseResult.GetValueForOption(_hierarchy),
        Bank = bindingContext.ParseResult.GetValueForOption(_bank),
        Audio = bindingContext.ParseResult.GetValueForOption(_audio),
        Convert = bindingContext.ParseResult.GetValueForOption(_convert),
        Externals = bindingContext.ParseResult.GetValueForOption(_externals),
        Events = bindingContext.ParseResult.GetValueForOption(_events),
        Input = bindingContext.ParseResult.GetValueForArgument(_input),
        Output = bindingContext.ParseResult.GetValueForArgument(_output),
    };
}