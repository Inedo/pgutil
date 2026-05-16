#!/usr/bin/dotnet run

#:sdk Microsoft.NET.Sdk

#:package Inedo.ProGet@2.1.3

#:property JsonSerializerIsReflectionEnabledByDefault=true

using Inedo.ProGet;

Console.WriteLine($"Starting ProGet bootstrap");

var client = new ProGetClient("http://localhost:8080", "Admin", "admin");

var groups = client.ListUserGroups();
foreach (var group in DesiredState.Groups)
{
    if (await groups.FirstOrDefaultAsync(g => g.Name.Equals(group.Name)) != null)
    {
        await client.UpdateUserGroupAsync(group);
    }
    else
    {
        await client.CreateUserGroupAsync(group);
    }
}

var users = client.ListUsersAsync();
foreach (var user in DesiredState.Users)
{
    if (await users.FirstOrDefaultAsync(u => u.Name.Equals(user.Name)) != null)
    {
        await client.UpdateUserAsync(user);
    }
    else
    {
        await client.CreateUserAsync(user);
    }
}

var connectors = client.ListConnectorsAsync();
foreach (var connector in DesiredState.Connectors)
{
    if (await connectors.FirstOrDefaultAsync(c => c.Name.Equals(connector.Name)) != null)
    {
        await client.UpdateConnectorAsync(connector.Name, connector);
    }
    else
    {
        await client.CreateConnectorAsync(connector);
    }
}

var feeds = client.ListFeedsAsync();
foreach (var feed in DesiredState.Feeds)
{
    if (await feeds.FirstOrDefaultAsync(f => f.Name!.Equals(feed.Name)) != null)
    {
        await client.UpdateFeedAsync(feed.Name!, feed);
    }
    else
    {
        await client.CreateFeedAsync(feed.Name!, feed.FeedType!);
        await client.UpdateFeedAsync(feed.Name!, feed);
    }
}

var settings = client.ListSettingsAsync();
foreach (var setting in DesiredState.Settings)
{
    await client.SetSettingAsync(setting.Name, setting.Value);
}

Console.WriteLine("ProGet bootstrap completed successfully.");

public static class DesiredState
{
    public static IReadOnlyList<SecurityUser> Users { get; } =
    [
        new SecurityUser {
            Name = "svc-ci",
            DisplayName = "CI Service Account",
            Email = "svc-ci@example.com",
            Password = "svc-ci",
            Groups = ["proget-admins"],
        },
    ];

    public static IReadOnlyList<SecurityGroup> Groups { get; } =
    [
        new SecurityGroup {
            Name = "proget-admins",
        },
    ];

    public static IReadOnlyList<ProGetConnector> Connectors { get; } =
    [
        new ProGetConnector {
            Name = "nuget-org",
            FeedType = "nuget",
            Url = "https://api.nuget.org/v3/index.json",
            Timeout = 30,
            MetadataCacheEnabled = true,
        },
    ];

    public static IReadOnlyList<ProGetFeed> Feeds { get; } =
    [
        new ProGetFeed {
            Name = "nuget-internal",
            FeedType = "NuGet",
            Description = "Internal NuGet packages with nuget.org connector",
            Active = true,
            UseApiV3 = true,
            Connectors = ["nuget-org"],
            RetentionRulesEnabled = true,
            VulnerabilitiesEnabled = true,
            PackageStatisticsEnabled = true,
        },
    ];

    public static IReadOnlyList<SettingsInfo> Settings { get; } =
    [
        new SettingsInfo {
            Name = "Web.BaseUrl",
            Value = "http://localhost:8080",
            Description = "Full root URL for this installation of ProGet. This should start with http:// or https://",
            ValueType = SettingsInfoValueType.Text,
        },
    ];
}