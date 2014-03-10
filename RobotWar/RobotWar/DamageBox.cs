using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace RobotWar
{
    class DamageBox
    {

        Texture2D textura;
        public Vector2 posicao_inicial;
        float posicao_actual_y,posicao_final_y;
        SpriteFont font;
        public bool activo;
        public int fire_power_atacante;
        static public float largura = 76, altura = 20;
        public int dano;
        float transparencia;
        bool morreu,ha_dano,passar_turno;
        tipo_PowerUp tipo;

        public void Initializing(Vector2 posicao_inicial, int fire_power_atacante, int dano, bool morreu)
        {
            this.posicao_inicial = posicao_inicial;
            this.posicao_actual_y = (int)posicao_inicial.Y;
            this.fire_power_atacante = fire_power_atacante;
            this.dano = dano;
            this.posicao_final_y = (int)posicao_inicial.Y - 40;
            this.transparencia = 1f;
            this.morreu = morreu;
            this.ha_dano = true;
            this.passar_turno = true;
        }

        public void Initializing(Vector2 posicao_inicial, tipo_PowerUp tipo, bool passar_turno)
        {
            this.posicao_inicial = posicao_inicial;
            this.posicao_actual_y = (int)posicao_inicial.Y;
            this.posicao_final_y = (int)posicao_inicial.Y - 40;
            this.transparencia = 1f;
            this.tipo = tipo;
            this.ha_dano = false;
            this.passar_turno = passar_turno;
        }

        public void LoadContent(ContentManager content, GraphicsDevice graphic)
        {
            font = content.Load<SpriteFont>("fontes/arial_12");
            textura = new Texture2D(graphic, 1, 1);
            textura.SetData(new[] { Color.White });
        }

        public void Update(ContentManager Content)
        {
            if (this.activo)
            {
                if (posicao_actual_y != posicao_final_y)
                {
                    posicao_actual_y -= 0.5f;
                    transparencia -= 0.01f;
                }
                else
                {
                    if(this.passar_turno) Game1.actualizarJogadas(Content);
                    this.activo = false;
                }
            }
        }

        public void UnloadContent()
        {
            textura.Dispose();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Vector2 posicao_actual = new Vector2(posicao_inicial.X, posicao_actual_y);
            Vector2 posicao_texto;
            string texto;
            Color cor_texto = Color.White;
            if (ha_dano)
            {
                if (morreu)
                {
                    texto = "TANGO DOWN";
                    posicao_texto = new Vector2(posicao_inicial.X - 5, posicao_actual.Y + 2);
                }
                else
                {
                    texto = "Damage: -" + dano;
                    posicao_texto = new Vector2(posicao_inicial.X + 5, posicao_actual.Y + 2);
                }
            }
            else
            {
                texto = this.tipo.ToString();
                posicao_texto = new Vector2(posicao_inicial.X + 5, posicao_actual.Y + 2);
            }

            //caixa vermelha
            
            if (!morreu)  
            {
                spriteBatch.Draw(textura, posicao_actual, null,
                    Color.DarkRed * transparencia, 0f, Vector2.Zero, new Vector2(largura, altura), SpriteEffects.None, 0f);
            }
            //texto
            spriteBatch.DrawString(font, texto, posicao_texto, cor_texto);

        }
    }
}
