using Microsoft.Xna.Framework;

namespace MonoGame2026_Heb;

public class Animation : Sprite
{
    private double totalTime = 0;
    private int samples = 60;
    private int x = 0;
    private int y = 0;
    bool isLooping = true;
    bool isAnimating = false;
    
    public Animation(string spriteName) : base(spriteName)
    {
    }

    public void PlayAnimation(bool isLooping = true, int samples = 60)
    {
        Reset();
        isAnimating = true;
    }

    void StopAnimation()
    {
        Reset();
    }

    void PauseAnimation()
    {
        isAnimating = false;
    }
    void ResumeAnimation()
    {
        isAnimating = true;
    }

    void Reset()
    {
        isAnimating = false;
        x = y = 0;
        totalTime = 0;
    }

    public override void Update(GameTime gameTime)
    { 
       if (!isAnimating) return;
        
       if (CanMoveFrame(gameTime))
           MoveFrame();
       
       base.Update(gameTime);
    }

    bool CanMoveFrame(GameTime gameTime)
    {
        double deltaTime = gameTime.ElapsedGameTime.TotalSeconds;
        totalTime += deltaTime;
        
        if (totalTime >= 1.0f / samples)
            return true;

        return false;
    }

    void MoveFrame()
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