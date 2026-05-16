# Samples

## Start ProGet

Before running the samples, start a local ProGet instance with your preferred container engine.

For example:

```bash
podman compose up
```

## Run a sample

> [!IMPORTANT]  
> The samples require a running ProGet instance with a valid license key configured.

After ProGet is running and initial setup is complete, run the sample with:

```bash
dotnet run ProGetBootstrap.cs
```