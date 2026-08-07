using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame2026_Heb;

public class SceneManager : IUpdatable, IDrawable
{
    // ============ Variables & References ==================================================================================================================
    
    private static List<IUpdatable> _updatables = new();
    private static List<IDrawable> _drawables = new();
    private static List<Collider> _colliders = new();
    private static readonly List<object> pendingRemoval = new();
    private static SceneManager instance = null;
    private static bool hasStarted;
    
    // =======================================================================================================================================================

    public static T Create<T>()  where T : new() // Instantiates a new game object and adds it to the active scene
    {
        T obj = new T();
        if (obj is IUpdatable updatable)
        {
            _updatables.Add(updatable);
            if (hasStarted) { updatable.Start(); }
        }
        if (obj is IDrawable drawable) { _drawables.Add(drawable); }
        if (obj is Collider collider) { _colliders.Add(collider); }
        return obj;
    }

    public static void Remove<T>(T obj) // Removes a game object from the active scene
    {
        pendingRemoval.Add(obj);
    }
    
    public static SceneManager Instance
    {
        get
        {
            if (instance == null) { instance = new SceneManager(); }
            return instance;
        }
    }
    
    public static void Clear() // Removes all active game objects, clearing the current scene
    {
        _updatables.Clear();
        _drawables.Clear();
        _colliders.Clear();
        pendingRemoval.Clear();
    }

    public void Start() // Initializes all objects currently in the scene
    {
        hasStarted = true;
        _updatables.ForEach(updatable => updatable.Start());
    }

    public void Update(GameTime gameTime) // Calls the Update() method on all active objects that implement IUpdatable
    {
        // Safely remove any objects that were queued for deletion
        foreach (var obj in pendingRemoval)
        {
            if (obj is IUpdatable updatable) _updatables.Remove(updatable);
            if (obj is IDrawable drawable) _drawables.Remove(drawable);
            if (obj is Collider collider) _colliders.Remove(collider);
        }
        pendingRemoval.Clear();
        for (int i = 0; i < _updatables.Count; i++) { _updatables[i].Update(gameTime); }
        HandleCollisions();
    }

    public void HandleCollisions() // Handels the colliders of all the objects in the game
    {
        for (int i = 0; i < _colliders.Count; i++)
        {
            Collider firstCollider = _colliders[i];
            if (!firstCollider.IsEnabled || firstCollider.Parent == null) { continue; }
            for (int j = i + 1; j < _colliders.Count; j++)
            {
                Collider secondCollider = _colliders[j];
                if (!secondCollider.IsEnabled || secondCollider.Parent == null) { continue; }
                if (!firstCollider.IsIntersecting(secondCollider)) continue;
                firstCollider.Notify(secondCollider);
                secondCollider.Notify(firstCollider);
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch) // Calls the Draw() method on all active objects that implement IDrawable
    {
        foreach (IDrawable drawable in _drawables.OrderBy(drawable => drawable is Sprite sprite ? sprite.sortingOrder : 30000))
        {
            drawable.Draw(spriteBatch);
        }
    }
}