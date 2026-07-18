using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MonoGame2026_Heb;

public class Player : Animation
{
    bool IsRKeyPressed = false;
    float speedRotation = 0;
    float speedMovement = 300;
    public Collider collider { get; }

    bool isColliding = false;
    Vector2 prevPosition = Vector2.Zero;

    public Player() : base("orangeBird")
    {
        collider = SceneManager.Create<Collider>();
        collider.Parent = this;
    }

    public override void Start()
    {
        base.Start();
        
        tm.position = Game1._screenCenter;
        tm.scale = new Vector2(0.3f, 0.3f);
        
        prevPosition =  tm.position;
   }

    public override void Update(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        if (Keyboard.GetState().IsKeyDown(Keys.R) && !IsRKeyPressed)
        {
            // R was pressed in this frame
            speedRotation = 500;
        }
        
        if (Keyboard.GetState().IsKeyDown(Keys.D))
        {
            effects = SpriteEffects.FlipHorizontally;
            tm.position += new Vector2(speedMovement * deltaTime, 0);
        }
        
        if (Keyboard.GetState().IsKeyDown(Keys.A))
        {
            effects = SpriteEffects.None;
            tm.position += new Vector2(-speedMovement * deltaTime, 0);
        }
        
        if (Keyboard.GetState().IsKeyDown(Keys.S))
        {
            tm.position += new Vector2(0, speedMovement * deltaTime);
        }
        
        if (Keyboard.GetState().IsKeyDown(Keys.W))
        {
            tm.position += new Vector2(0, -speedMovement * deltaTime);
        }

        IsRKeyPressed =  Keyboard.GetState().IsKeyDown(Keys.R);
        
        if (Keyboard.GetState().IsKeyDown(Keys.NumPad1))
        {
           AudioManager.IsPaused = true;
           ChangeSprite("duck");
        }
        if (Keyboard.GetState().IsKeyDown(Keys.NumPad2))
        {
            AudioManager.IsPaused = false;
        }
        
        tm.rotation = (float)gameTime.TotalGameTime.TotalSeconds * speedRotation;

        base.Update(gameTime);
        
        if (isColliding)
        {
            tm.position =  prevPosition;
            isColliding = false;
        }
        
        prevPosition =  tm.position;
        
       

    }

    public void OnCollision(Collider selfCollder, Collider otherCollder)
    {
        isColliding = true;
        Console.WriteLine("Self " + selfCollder.Parent + " is colliding with " + otherCollder.Parent);
    }
    
    public void OnTrigger(Collider selfCollder, Collider otherCollder)
    {
        
        AudioManager.PlaySoundEffect("collect");
        
        Console.WriteLine("Self " + selfCollder.Parent + " is trigger with " + otherCollder.Parent);
        
        SceneManager.Remove(otherCollder);
        SceneManager.Remove(otherCollder.Parent);
    }
}