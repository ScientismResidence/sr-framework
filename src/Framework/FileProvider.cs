using System.Collections;
using Framework.Logger;

namespace Framework;

public class FileProvider<TKey> : IProvider<TKey> where TKey : struct, Enum
{
    private readonly Dictionary<TKey, string> _storage = new();
    private readonly ILogger _logger;
    
    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="path">
    /// Relative path from assembly execution.
    /// </param>
    public FileProvider(string path, ILogger logger)
    {
        _logger = logger;

        ProcessDirectory(Path.Combine(AppContext.BaseDirectory, path));

        _logger.Log(
            $"Scripts provider [{GetType()}] is initialized with [{Count}] scripts.", 
            null, 
            LogLevel.Information, 
            LogTag.Startup);
    }
    
    public string this[TKey key] => _storage[key];

    public IEnumerable<TKey> Keys => _storage.Keys;
    
    public IEnumerable<string> Values => _storage.Values;
    
    public int Count => _storage.Count;

    public IEnumerator<KeyValuePair<TKey, string>> GetEnumerator()
    {
        return _storage.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public bool ContainsKey(TKey key)
    {
        return _storage.ContainsKey(key);
    }

    public bool TryGetValue(TKey key, out string value)
    {
        return _storage.TryGetValue(key, out value!);
    }

    private void ProcessDirectory(string path)
    {
        string[] files = Directory.GetFiles(path);

        foreach (string file in files)
        {
            string filename = Path.GetFileNameWithoutExtension(file);
            if (Enum.TryParse(filename, out TKey result))
            {
                string script = File.ReadAllText(file);
                _storage.Add(result, script);
            }
        }

        string[] directories = Directory.GetDirectories(path);
        foreach (string directory in directories)
        {
            ProcessDirectory(directory);
        }
    }
}