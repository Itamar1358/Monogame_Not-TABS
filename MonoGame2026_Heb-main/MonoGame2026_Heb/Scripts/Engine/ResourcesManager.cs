using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace MonoGame2026_Heb;

public class ResourcesManager <T> where T : class
{
    // ============ Variables & References ==================================================================================================================
    
    static Dictionary<string, T> loadedResources = new Dictionary<string, T>();
    private static ContentManager _content;
    
    // ======================================================================================================================================================
    
    public ResourcesManager(ContentManager Content) { _content = Content; }

    public static T LoadResource(string name, string filename) // Loads an asset via MonoGame's ContentManager and stores it in a dictionar
    {
        if (!loadedResources.ContainsKey(name)) { loadedResources[name] = _content.Load<T>(filename); }
        return loadedResources[name];
    }

    public static T GetResource(string name) // Retrieves a previously loaded asset from the dictionary
    {
        if (loadedResources.ContainsKey(name)) return loadedResources[name];
        return null;
    }
}