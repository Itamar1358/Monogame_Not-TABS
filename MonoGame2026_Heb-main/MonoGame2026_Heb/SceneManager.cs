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

    private static SceneManager instance = null;

    public static T Create<T>()  where T : new()
    {
        T obj = new T();
        
        if (obj is IUpdatable updatable)
        {
            _updatables.Add(updatable);
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
        _updatables.ForEach(updatable => updatable.Start());
    }

    public void Update(GameTime gameTime)
    {
        _updatables.ForEach(updatable => updatable.Update(gameTime));
        
        HandleCollisions();
    }

    public void HandleCollisions()
    {
        for (int i = 0; i < _colliders.Count; i++)
        {
            Collider currentCollider = _colliders[i];

            for (int j = 0; j < _colliders.Count; j++)
            {
                Collider otherCollider = _colliders[j];
                
                if (i != j && currentCollider.IsInterset(otherCollider))
                    currentCollider.Notify(otherCollider);
            }
        }
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        _drawables.ForEach(drawable => drawable.Draw(spriteBatch));
    }
}