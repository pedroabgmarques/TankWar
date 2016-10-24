using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace RobotWar
{
    class Animacao
    {

        // Textura (spritestrip)
        Texture2D spriteStrip;

        // escala da animação
        float escala;

        //rotacao da animacao
        float rotacao;

        // tempo desde o ultimo frame
        int tempoPassado;

        // tempo que fica cada frame da animação
        float tempoFrame;

        // numero total de frames da animação
        int nFrames;

        // index do frame actual
        int frameActual;

        // cor do frame
        Color cor;

        // area do strip que estamos a mostrar a cada momento
        Rectangle sourceRect = new Rectangle();

        // area do jogo onde vai aparecer o frame
        Rectangle destinationRect = new Rectangle();

        // largura de um frame
        public int FrameWidth;

        // altura de um frame
        public int FrameHeight;

        // estado da animação
        public bool activo;

        // determina se fica em loop ou apenas uma vez
        public bool Looping;

        // largura do spritestrip
        public Vector2 Position;

        public void Initialize(Texture2D texture, Vector2 position,
int frameWidth, int frameHeight, int frameCount,
float frametime, Color color, float scale, bool looping, float rotacao)
        {
            // Keep a local copy of the values passed in
            this.cor = color;
            this.FrameWidth = frameWidth;
            this.FrameHeight = frameHeight;
            this.nFrames = frameCount;
            this.tempoFrame = frametime;
            this.escala = scale;
            this.rotacao = rotacao;

            Looping = looping;
            Position = position;
            spriteStrip = texture;

            // Set the time to zero
            tempoPassado = 0;
            frameActual = 0;

            // Set the Animation to active by default
            activo = true;
        }

        public void Update(GameTime gameTime)
        {
            // Do not update the game if we are not active
            if (activo == false)
                return;

            // Update the elapsed time
            tempoPassado += (int)gameTime.ElapsedGameTime.TotalMilliseconds;

            // If the elapsed time is larger than the frame time
            // we need to switch frames
            if (tempoPassado > tempoFrame)
            {
                // Move to the next frame
                frameActual++;

                // If the currentFrame is equal to frameCount reset currentFrame to zero
                if (frameActual == nFrames)
                {
                    frameActual = 0;
                    // If we are not looping deactivate the animation
                    if (Looping == false)
                        activo = false;
                }

                // Reset the elapsed time to zero
                tempoPassado = 0;
            }

            // Grab the correct frame in the image strip by multiplying the currentFrame index by the frame width
            sourceRect = new Rectangle(frameActual * FrameWidth, 0, FrameWidth, FrameHeight);

            
            destinationRect = new Rectangle((int)Position.X - (int)(FrameWidth * escala) / 2,
            (int)Position.Y - (int)(FrameHeight * escala) / 2,
            (int)(FrameWidth * escala),
            (int)(FrameHeight * escala));
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Only draw the animation when we are active
            if (activo)
            {
                spriteBatch.Draw(spriteStrip, destinationRect, sourceRect, cor,rotacao,Vector2.Zero,SpriteEffects.None,rotacao);
            }
        }
    }
}
