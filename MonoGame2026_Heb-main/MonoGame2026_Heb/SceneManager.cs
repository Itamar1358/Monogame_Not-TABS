using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame2026_Heb;

public class SceneManager : IUpdatable, IDrawable
{
    private static List<IUpdatable> _updatables = new();
    private static List<IDrawable> _drawables = new();
    private static List<Collider> _colliders = new();
    private static readonly List<object> pendingRemoval = new();

    private static SceneManager instance = null;
    private static bool hasStarted;
    
    

    public static T Create<T>()  where T : new()
    {
        T obj = new T();
        
        if (obj is IUpdatable updatable)
        {
            _updatables.Add(updatable);

            if (hasStarted)
            {
                updatable.Start();
            }
        }
        if (obj is IDrawable drawable)
        {
            _drawables.Add(drawable);
        }
        
        if (obj is Collider collider)
        {
            _colliders.Add(collider);
        }
        
        return obj;
    }

    public static void Remove<T>(T obj)
    {
        if (obj is IUpdatable updatable)
        {
            _updatables.Remove(updatable);
        }
        if (obj is IDrawable drawable)
        {
            _drawables.Remove(drawable);
        }
        if (obj is Collider collider)
        {
            _colliders.Remove(collider);
        }
    }

    public static SceneManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new SceneManager();
            }

            return instance;
        }
    }

    public void Start()
    {
        hasStarted = true;
        
        _updatables.ForEach(updatable => updatable.Start());
    }

    public void Update(GameTime gameTime)
    {
        // Iterate backwards so that if an item is added or removed during Update, it doesn't break the loop
        for (int i = _updatables.Count - 1; i >= 0; i--)
        {
            _updatables[i].Update(gameTime);
        }
        
        HandleCollisions();
    }

    public void HandleCollisions()
    {
        for (int i = 0; i < _colliders.Count; i++)
        {
            Collider firstCollider = _colliders[i];

            if (!firstCollider.IsEnabled ||
                firstCollider.Parent == null)
            {
                continue;
            }

            for (int j = i + 1; j < _colliders.Count; j++)
            {
                Collider secondCollider = _colliders[j];

                if (!secondCollider.IsEnabled ||
                    secondCollider.Parent == null)
                {
                    continue;
                }

                if (!firstCollider.IsIntersecting(secondCollider))
                    continue;

                firstCollider.Notify(secondCollider);
                secondCollider.Notify(firstCollider);
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        _drawables.ForEach(drawable => drawable.Draw(spriteBatch));
    }
}