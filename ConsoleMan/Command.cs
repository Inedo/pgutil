namespace ConsoleMan;

public abstract class Command : ConsoleToken
{
    private protected Command(List<Command> commands, List<Option> options)
    {
        this.Subcommands = commands;
        this.Options = options;
    }

    internal Command? Parent { get; set; }
    internal List<Command> Subcommands { get; }
    internal List<Option> Options { get; }

    public static Command Create<TCommand>() where TCommand : IConsoleCommand
    {
        var builder = new CommandBuilder<TCommand>();
        TCommand.Configure(builder);
        return builder.Build();
    }

    public async Task<int> ExecuteAsync(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        var context = this.ParseArguments(args);
        if (context == null)
            return -1;

        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += cancel;
        try
        {
            return await context.Command.RunAsync(context, cts.Token);
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine();
            CM.WriteError("Operation canceled.");
            return -1;
        }
        finally
        {
            Console.CancelKeyPress -= cancel;
        }

        void cancel(object? source, ConsoleCancelEventArgs e)
        {
            cts.Cancel();
            e.Cancel = true;
        }
    }

    internal abstract Task<int> RunAsync(CommandContext context, CancellationToken cancellationToken);

    internal IEnumerable<ScopedOption> GetOptionsInScope(int scope = 0) => this.Options.Select(o => new ScopedOption(o, this.Name, scope)).Union(this.Parent?.GetOptionsInScope(scope + 1) ?? []);
    private CommandContext? ParseArguments(string[] args)
    {
        var parsedCommands = new List<Command> { this };
        var commandIndexes = new List<int>();

        var commandScope = this.Subcommands;
        var command = this;

        for (int i = 0; i < args.Length; i++)
        {
            var match = commandScope.FirstOrDefault(c => c.Name == args[i]);
            if (match != null)
            {
                parsedCommands.Add(match);
                commandIndexes.Add(i);
                commandScope = match.Subcommands;
                command = match;
            }
        }

        bool error = false;
        bool help = false;

        var scopedOptions = command.GetOptionsInScope().Select(o => o.Option).ToList();

        var parsedOptions = new Dictionary<Type, ParsedOption>();
        for (int i = 0; i < args.Length; i++)
        {
            if (commandIndexes.Contains(i))
                continue;

            if (args[i] is "-?" or "--help")
            {
                help = true;
                break;
            }

            foreach (var o in scopedOptions)
            {
                if (o.TryParse(args[i], out var value))
                {
                    if (!parsedOptions.TryAdd(o.Type, new ParsedOption(o, value)))
                    {
                        CM.WriteError($"option specified more than once: {o.Name}");
                        error = true;
                    }

                    goto next;
                }
            }

            CM.WriteError($"unexpected argument: {args[i]}");
            error = true;
        next:;
        }

        if (!help)
        {
            foreach (var o in scopedOptions)
            {
                if (!parsedOptions.ContainsKey(o.Type))
                {
                    if (o.DefaultValue != null)
                    {
                        parsedOptions[o.Type] = new ParsedOption(o, o.DefaultValue);
                    }
                    else if (o.Required)
                    {
                        CM.WriteError($"missing required argument: {o.Name}");
                        error = true;
                    }
                }
            }

            foreach (var o in parsedOptions.Values)
            {
                if (o.Option.ValidValues is not null && !o.Option.ValidValues.Contains(o.Value, StringComparer.OrdinalIgnoreCase))
                {
                    CM.WriteError($"{o.Option.Name}={o.Option.ValidValues} is invalid. Valid values are: {string.Join(", ", o.Option.ValidValues)}");
                    error = true;
                }
            }
        }

        var context = new CommandContext(parsedOptions, parsedCommands);
        if (!error && !help)
        {
            return context;
        }
        else
        {
            Console.WriteLine();
            context.WriteUsage();
            return null;
        }
    }
    internal sealed record ScopedOption(Option Option, string Scope, int Depth);
}

internal sealed class Command<TCommand> : Command where TCommand : IConsoleCommand
{
    internal Command(List<Command> commands, List<Option> options) : base(commands, options)
    {
    }

    public override string Name => TCommand.Name;
    public override string Description => TCommand.Description;
    public override Type Type => typeof(TCommand);

    internal override Task<int> RunAsync(CommandContext context, CancellationToken cancellationToken) => TCommand.ExecuteAsync(context, cancellationToken);
}
