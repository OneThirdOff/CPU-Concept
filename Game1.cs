using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Hacking_Game
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont spriteFont;

        string text; 
        
        Keys[] keysToCheck = new Keys[] {
            Keys.A, Keys.B, Keys.C, Keys.D, Keys.E,
            Keys.F, Keys.G, Keys.H, Keys.I, Keys.J,
            Keys.K, Keys.L, Keys.M, Keys.N, Keys.O,
            Keys.P, Keys.Q, Keys.R, Keys.S, Keys.T,
            Keys.U, Keys.V, Keys.W, Keys.X, Keys.Y,
            Keys.Z, Keys.Back, Keys.Space, Keys.OemQuestion, Keys.Enter }; 

        KeyboardState currentKeyboardState;
        KeyboardState lastKeyboardState;

        private int NumberOfRegisters = 2;
        private int DataBusWidth = 8;
        private int AddressBusWidth = 16;
        private int MemorySize = 2256;
        private int ReservedMemForGraphics = 2000;

        public static int ScreenWidth = 1024;
        public static int ScreenHeight = 768;
        
        public static BIOS systemBios;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferHeight = ScreenHeight;
            graphics.PreferredBackBufferWidth = ScreenWidth;
            graphics.ApplyChanges();
            systemBios = new BIOS(DataBusWidth, AddressBusWidth, NumberOfRegisters, MemorySize, ReservedMemForGraphics);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteFont = Content.Load<SpriteFont>("Arial");
            systemBios.DoDumpRegisters();
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            graphics.PreferredBackBufferHeight = ScreenHeight;
            graphics.PreferredBackBufferWidth = ScreenWidth;
            graphics.ApplyChanges();

            currentKeyboardState = Keyboard.GetState();

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape)) 
            {
                Exit();
            }

            foreach (Keys key in keysToCheck)
            {
                if (CheckKey(key))
                {
                    AddKeyToText(key);
                    break;
                }
            }
            if (currentKeyboardState.Equals(Keys.Enter))
            {
                systemBios.Update();
            }
            base.Update(gameTime);

            lastKeyboardState = currentKeyboardState;
        }
        
        private bool CheckKey(Keys theKey)
        {
            return lastKeyboardState.IsKeyDown(theKey) && currentKeyboardState.IsKeyUp(theKey);
        }

        private void AddKeyToText(Keys key)
        {
            string newChar = "";
            if (text.Length >= 20 && key != Keys.Back)
                return;
            switch (key)
            {
                case Keys.A:
                    newChar += "a";
                    break;
                case Keys.B:
                    newChar += "b";
                    break;
                case Keys.C:
                    newChar += "c";
                    break;
                case Keys.D:
                    newChar += "d";
                    break;
                case Keys.E:
                    newChar += "e";
                    break;
                case Keys.F:
                    newChar += "f";
                    break;
                case Keys.G:
                    newChar += "g";
                    break;
                case Keys.H:
                    newChar += "h";
                    break;
                case Keys.I:
                    newChar += "i";
                    break;
                case Keys.J:
                    newChar += "j";
                    break;
                case Keys.K:
                    newChar += "k";
                    break;
                case Keys.L:
                    newChar += "l";
                    break;
                case Keys.M:
                    newChar += "m";
                    break;
                case Keys.N:
                    newChar += "n";
                    break;
                case Keys.O:
                    newChar += "o";
                    break;
                case Keys.P:
                    newChar += "p";
                    break;
                case Keys.Q:
                    newChar += "q";
                    break;
                case Keys.R:
                    newChar += "r";
                    break;
                case Keys.S:
                    newChar += "s";
                    break;
                case Keys.T:
                    newChar += "t";
                    break;
                case Keys.U:
                    newChar += "u";
                    break;
                case Keys.V:
                    newChar += "v";
                    break;
                case Keys.W:
                    newChar += "w";
                    break;
                case Keys.X:
                    newChar += "x";
                    break;
                case Keys.Y:
                    newChar += "y";
                    break;
                case Keys.Z:
                    newChar += "z";
                    break;
                case Keys.Space:
                    newChar += " ";
                    break;
                case Keys.Back:
                    if (text.Length != 0)
                    {
                        text = text.Remove(text.Length - 1);
                        return;
                    }
                    break;
            }
            if (currentKeyboardState.IsKeyDown(Keys.RightShift) ||
                currentKeyboardState.IsKeyDown(Keys.LeftShift))
            {
                newChar = newChar.ToUpper();
            }
            text += newChar;
        }



    /// <summary>
    /// This is called when the game should draw itself.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            spriteBatch.Begin();
            // TODO: Add your drawing code here
            systemBios.Draw(spriteBatch, spriteFont, ScreenWidth, ScreenHeight);

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
