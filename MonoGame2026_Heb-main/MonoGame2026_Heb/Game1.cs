using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using MonoGame2026_Heb.Content;

namespace MonoGame2026_Heb;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    Texture2D _logo;
    Texture2D _pongAtlas;
    
    public static Vector2 _screenCenter;

    private Player player = null;
    private Enemy enemy = null;

    private SpriteFont _fontOswald;
    
    MousePositionText mousePositionText = new MousePositionText();

    #region ResourcesManager
    
    private ResourcesManager<Texture2D> textureManager;
    private ResourcesManager<Song> songManager;
    private ResourcesManager<SoundEffect> soundEffectManager;

    #endregion
    
    
    private SpriteManager spriteManager = null;
    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);

        textureManager = new(Content);
        songManager = new(Content);
        soundEffectManager = new(Content);
        
        spriteManager = new SpriteManager();
        
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _graphics.PreferredBackBufferWidth = 1920;
        _graphics.PreferredBackBufferHeight = 1080;

        _graphics.IsFullScreen = true;
        
        _screenCenter =  new Vector2(
            _graphics.PreferredBackBufferWidth * 0.5f,
            _graphics.PreferredBackBufferHeight * 0.5f);

    }

    protected override void Initialize()
    {
        // TODO: Add your initialization logic here

        base.Initialize();
    }

    protected override void LoadContent()
    {
        
        AudioManager.AddSong("theme", "Audio/Music/theme");
        AudioManager.AddSoundEffect("collect", "Audio/SFX/collect");
        AudioManager.AddSoundEffect("bounce", "Audio/SFX/bounce");
        
        
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        SpriteManager.AddSprite("orangeBird","Images/Bird1_1", 4,4);
        SpriteManager.AddSprite("duck","Images/Bird2 Duck_1", 4,4);
        SpriteManager.AddSprite("egret","Images/Bird3_Egret4", 4,4);
        SpriteManager.AddSprite("Pixel","Images/pixel");
 
        mousePositionText.font = Content.Load<SpriteFont>("Fonts/Oswald");
        
        Start();
    }

    void Start()
    {
        AudioManager.PlaySong("theme");
        
        enemy = SceneManager.Create<Enemy>();
        enemy.PlayAnimation();
        
        player = SceneManager.Create<Player>();
        player.PlayAnimation();
        
        SceneManager.Instance.Start();

        player.collider.RegisterOnCollision(player.OnCollision);
        player.collider.RegisterOnTrigger(player.OnTrigger);

    }

    bool ShouldExitApplication()
    {
        return GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
               Keyboard.GetState().IsKeyDown(Keys.Escape);
    }

    protected override void Update(GameTime gameTime)
    {
        if (ShouldExitApplication()) Exit();
        
        SceneManager.Instance.Update(gameTime);
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.DarkRed);

        _spriteBatch.Begin();

        SceneManager.Instance.Draw(_spriteBatch);
        
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}