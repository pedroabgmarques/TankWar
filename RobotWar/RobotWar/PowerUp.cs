using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace RobotWar
{

    public enum tipo_PowerUp
    {
        double_turn,
        mega_power,
        double_shot,
        sniper,
        vida
    }
    
    public class PowerUp
    {
        Texture2D textura, double_turn, mega_power, double_shot, sniper, vida;
        Vector2 posicao;
        public Vector2 posicao_grelha;
        public tipo_PowerUp tipo;
        public int duracao;
        SpriteFont tahoma_20_bold;

        public void Initializing(tipo_PowerUp tipo, Vector2 posicao, Vector2 posicao_grelha, ContentManager content, int duracao)
        {
            this.tipo = tipo;
            this.duracao = duracao;
            this.posicao = posicao;
            this.posicao_grelha = posicao_grelha;

            LoadContent(content);

        }

        public void LoadContent(ContentManager content)
        {

            double_shot = content.Load<Texture2D>("Texturas/sprites/tanques/powerups/double_shot");

            double_turn = content.Load<Texture2D>("Texturas/sprites/tanques/powerups/double_turn");

            mega_power = content.Load<Texture2D>("Texturas/sprites/tanques/powerups/mega_power");

            sniper = content.Load<Texture2D>("Texturas/sprites/tanques/powerups/sniper");


            vida = content.Load<Texture2D>("Texturas/sprites/tanques/powerups/vida");

            tahoma_20_bold = content.Load<SpriteFont>("fontes/font_tahoma_20_bold");
        }

        public void UnloadContent()
        {
        }

        public void Update()
        {

        }

        public void Draw(SpriteBatch spritebatch)
        {

            if (tipo == tipo_PowerUp.double_shot)
            {
                textura = double_shot;
            }
            if (tipo == tipo_PowerUp.double_turn)
            {
                textura = double_turn;
            }
            if (tipo == tipo_PowerUp.mega_power)
            {
                textura = mega_power;
            }
            if (tipo == tipo_PowerUp.sniper)
            {
                textura = sniper;
            }
            if (tipo == tipo_PowerUp.vida)
            {
                textura = vida;
            }

                spritebatch.Draw(textura, posicao, null, Color.White,
                   0f, Vector2.Zero, 0.5f, SpriteEffects.None, 0f);
                spritebatch.DrawString(tahoma_20_bold, duracao.ToString(), new Vector2(posicao.X + 3, posicao.Y + 2), Color.Yellow);
            
        }
    }
}
