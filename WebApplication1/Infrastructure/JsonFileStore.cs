using System.Text.Json;

namespace WebApplication1.Infrastructure;

public sealed class JsonFileStore<T>
{
    private readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public JsonFileStore(IHostEnvironment environment, ILogger<JsonFileStore<T>> logger)
    {
        FilePath = Path.Combine(environment.ContentRootPath, "App_Data", FileNameFor(typeof(T)));
        _logger = logger;
    }

    private readonly ILogger<JsonFileStore<T>> _logger;

    public string FilePath { get; }

    public IReadOnlyList<T> Load()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
                File.WriteAllText(FilePath, "[]");
                return [];
            }

            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<List<T>>(json, _options) ?? [];
        }
        catch (JsonException exception)
        {
            _logger.LogError(exception, "Malformed JSON in {FilePath}. The original file was preserved.", FilePath);
            throw new InvalidDataException($"The data file '{Path.GetFileName(FilePath)}' contains malformed JSON. The original file was preserved.", exception);
        }
        catch (IOException exception)
        {
            _logger.LogError(exception, "Unable to read {FilePath}.", FilePath);
            throw new InvalidDataException($"The data file '{Path.GetFileName(FilePath)}' could not be read.", exception);
        }
    }

    public void Save(IEnumerable<T> records)
    {
        var directory = Path.GetDirectoryName(FilePath)!;
        Directory.CreateDirectory(directory);

        var temporaryPath = $"{FilePath}.{Guid.NewGuid():N}.tmp";
        var backupPath = $"{FilePath}.bak";
        var json = JsonSerializer.Serialize(records, _options);

        try
        {
            File.WriteAllText(temporaryPath, json);
            if (File.Exists(FilePath))
            {
                File.Copy(FilePath, backupPath, overwrite: true);
                File.Replace(temporaryPath, FilePath, backupPath, ignoreMetadataErrors: true);
            }
            else
            {
                File.Move(temporaryPath, FilePath);
            }
        }
        catch (IOException exception)
        {
            _logger.LogError(exception, "Unable to save {FilePath}. Existing data was preserved where possible.", FilePath);
            throw new InvalidDataException($"The data file '{Path.GetFileName(FilePath)}' could not be saved. Existing data was preserved where possible.", exception);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    private static string FileNameFor(Type type) => type == typeof(ItemRecord) ? "Items.json"
        : type == typeof(ClaimRecord) ? "Claims.json"
        : type == typeof(UserRecord) ? "Users.json"
        : $"{type.Name}.json";
}