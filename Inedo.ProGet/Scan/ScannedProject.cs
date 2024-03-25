namespace Inedo.DependencyScan;

/// <summary>
/// Contains information about a project file.
/// </summary>
public sealed class ScannedProject
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScannedProject"/> class.
    /// </summary>
    /// <param name="name">Name of the project.</param>
    /// <param name="dependencies">Dependencies used by the project.</param>
    public ScannedProject(string name, IEnumerable<DependencyPackage> dependencies)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(dependencies);
        this.Name = name;
        this.Dependencies = [.. dependencies];
    }
    private ScannedProject(string name, IReadOnlyCollection<DependencyPackage> dependencies)
    {
        this.Name = name;
        this.Dependencies = dependencies;
    }

    /// <summary>
    /// Gets the name of the project.
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// Gets the dependencies used by the project.
    /// </summary>
    public IReadOnlyCollection<DependencyPackage> Dependencies { get; }

    public static async Task<ScannedProject> CreateAsync(string name, IAsyncEnumerable<DependencyPackage> dependencies)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(dependencies);

        var items = new HashSet<DependencyPackage>();
        await foreach (var d in dependencies)
            items.Add(d);

        return new ScannedProject(name, items);
    }
}
