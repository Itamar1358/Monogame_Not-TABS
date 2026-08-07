using System;
using Microsoft.Xna.Framework;

namespace MonoGame2026_Heb;

public class Animation : Sprite
{
    // ============ Variables & References ==================================================================================================================
    
    private double totalTime = 0;
    private int samples = 6;
    private int x = 0;
    private int y = 0;
    bool isLooping = true;
    bool isAnimating = false;
    
    // ======================================================================================================================================================
    
    public Animation(string spriteName) : base(spriteName) // (Constructor): Initializes the base sprite for the animation
    { }

    public void PlayAnimation(bool isLooping = true, int samples = 6) // Starts playing the spritesheet animation at a specific framerate
    {
        this.isLooping = isLooping;
        this.samples = Math.Max(1, samples);
        Reset();
        isAnimating = true;
    }

    protected void StopAnimation() // Halts the animation and resets to the first frame
    {
        Reset();
    }

    protected void PauseAnimation() // Pauses the animation at the current frame
    {
        isAnimating = false;
    }
    protected void ResumeAnimation() // Resumes the paused animation
    {
        isAnimating = true;
    }

    private void Reset() // Resets the animation timer and frame index
    {
        isAnimating = false;
        x = y = 0;
        totalTime = 0;
        sourceRect = spritesheet[x, y];
    }

    public override void Update(GameTime gameTime) // Progresses the animation frames based on elapsed time
    { 
        if (isAnimating && CanMoveFrame(gameTime)) { MoveFrame(); }
        base.Update(gameTime);
    }

    bool CanMoveFrame(GameTime gameTime) //  Checks if enough time has passed to advance to the next frame
    {
        double deltaTime = gameTime.ElapsedGameTime.TotalSeconds;
        totalTime += deltaTime;
        if (totalTime >= 1.0f / samples) return true;
        return false;
    }

    void MoveFrame() // Increments the frame index and loops if necessary
    {
        totalTime = 0;
        x++;
        if (x == spritesheet.columns)
        {
            x = 0;
            y++;
            if (y == spritesheet.rows)
            {
                if (isLooping)
                {
                    x = 0;
                    y = 0;
                }
                else
                {
                    x = spritesheet.columns - 1;
                    y = spritesheet.rows - 1;
                }
            }
        }
        sourceRect = spritesheet[x, y];
    }
}