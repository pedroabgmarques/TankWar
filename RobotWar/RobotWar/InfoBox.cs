using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace RobotWar
{
    class InfoBox
    {

        Texture2D rectangulo_branco;
        List<Tank> lista_tanques;
        public Tank tanque_activo;
        SpriteFont font, arial_12, tahoma_20, tahoma_20_bold;
        public Vector2 posicao_rato;
        public bool activo = false;

        public void Initializing(List<Tank> lista_tanques) 
        {
            this.lista_tanques = lista_tanques;
        }

        public void LoadContent(ContentManager content, GraphicsDevice graphic, Tank tanque_seleccionado)
        {
            font = content.Load<SpriteFont>("fontes/font_tahoma_25_bold");
            arial_12 = content.Load<SpriteFont>("fontes/arial_12");
            tahoma_20 = content.Load<SpriteFont>("fontes/font_tahoma_20");
            tahoma_20_bold = content.Load<SpriteFont>("fontes/font_tahoma_20_bold");
            rectangulo_branco = content.Load<Texture2D>("Texturas/componentes/textura_infobox2");
            /*
             * if (tanque_seleccionado != null)
            {
                if (tanque_seleccionado.sniper == false && tanque_seleccionado.double_shot == false && tanque_seleccionado.mega_power == false)
                {
                    rectangulo_branco = content.Load<Texture2D>("Texturas/componentes/textura_infobox2_2");
                }
                else
                {
                    rectangulo_branco = content.Load<Texture2D>("Texturas/componentes/textura_infobox2");
                }
            }
            else
            {
                rectangulo_branco = content.Load<Texture2D>("Texturas/componentes/textura_infobox2");
            }
            */
        }

        public void UnloadContent()
        {
            rectangulo_branco.Dispose();
        }

        public void Update(ContentManager content, Tank tanque_seleccionado)
        {
            
        }

        public void Draw(SpriteBatch spritebatch, GraphicsDevice graphics, Tank tanque_seleccionado)
        {
            float x, y;
            x = posicao_rato.X;
            y = posicao_rato.Y;

            if ((x + 170f) > graphics.Viewport.TitleSafeArea.Width)
            {
                x -= 170f;
            }

            if ((y + 99f) > graphics.Viewport.TitleSafeArea.Height)
            {
                y -= 99f;
            }
            
            if (activo)
            {
                
                spritebatch.Draw(rectangulo_branco, new Vector2(x,y), null,
                    Color.White*0.5f, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

                string ID;
                if (tanque_activo.ID + 1 < 10)
                {
                    ID = "0" + (tanque_activo.ID + 1);
                }
                else
                {
                    ID = (tanque_activo.ID + 1).ToString();
                }

                spritebatch.DrawString(font, tanque_activo.equipa.ToString()+ID, new Vector2(x+10,y +10), Color.Black);
                spritebatch.DrawString(tahoma_20_bold, "Power : " + tanque_activo.power.ToString(), new Vector2(x + 10, y + 40), Color.Red);
                spritebatch.DrawString(tahoma_20_bold, "Health : " + tanque_activo.vida.ToString(), new Vector2(x + 10, y + 60), Color.LawnGreen);
                if (tanque_activo.sniper == false && tanque_activo.double_shot == false && tanque_activo.mega_power == false)
                {
                    spritebatch.DrawString(tahoma_20_bold, "Powerups : 0", new Vector2(x + 10, y + 80), Color.Blue);
                }
                else
                {
                    spritebatch.DrawString(tahoma_20_bold, "Powerups :", new Vector2(x + 10, y + 80), Color.Blue);
                    int plus=20;
                    if (tanque_activo.sniper == true)
                    {
                        spritebatch.DrawString(tahoma_20_bold, "Sniper", new Vector2(x + 10, y + 80 +plus), Color.Blue);
                        plus += 20;
                    }
                    if (tanque_activo.double_shot == true)
                    {
                        spritebatch.DrawString(tahoma_20_bold, "Double Shot", new Vector2(x + 10, y + 80 + plus), Color.Blue);
                        plus += 20;
                    }
                    if (tanque_activo.mega_power == true)
                    {
                        spritebatch.DrawString(tahoma_20_bold, "Mega Power", new Vector2(x + 10, y + 80 + plus), Color.Blue);
                        plus += 20;
                    }
                }

                /*spritebatch.DrawString(font, "x : " + x.ToString(), new Vector2(x + 10, y + 90), Color.Black);
                spritebatch.DrawString(font, "y : " + y.ToString(), new Vector2(x + 10, y + 120), Color.Black);
                
                if (tanque_seleccionado != null)
                {
                    spritebatch.DrawString(arial_12, "Angulo : " + tanque_seleccionado.angulo_actual.ToString(), new Vector2(x + 10, y + 150), Color.Black);
                    spritebatch.DrawString(arial_12, "Angulo destino: " + tanque_seleccionado.angulo_destino.ToString(), new Vector2(x + 10, y + 180), Color.Black);
                    spritebatch.DrawString(arial_12, "Angulo variacao: " + ((tanque_seleccionado.angulo_destino - tanque_seleccionado.angulo_actual) / 4).ToString(), new Vector2(x + 10, y + 130), Color.Black);
                }
                */
            }

            #region DEBUG
            /*
            spritebatch.Draw(rectangulo_branco, new Vector2(x, y), null,
                Color.White * 0.75f, 0f, Vector2.Zero, new Vector2(170, 200f), SpriteEffects.None, 0f);
            spritebatch.DrawString(arial_12, "x : " + x.ToString(), new Vector2(x + 10, y + 70), Color.DarkGreen);
            spritebatch.DrawString(arial_12, "y : " + y.ToString(), new Vector2(x + 10, y + 85), Color.DarkGreen);

            if (tanque_seleccionado != null)
            {
                spritebatch.DrawString(arial_12, "Angulo (radiano) : " + tanque_seleccionado.anguloDoisVectores(tanque_seleccionado.posicao,posicao_rato).ToString(), new Vector2(x + 10, y + 40), Color.DarkGreen);
                spritebatch.DrawString(arial_12, "Angulo (graus) : " + MathHelper.ToDegrees(tanque_seleccionado.anguloDoisVectores(tanque_seleccionado.posicao, posicao_rato)).ToString(), new Vector2(x + 10, y + 55), Color.DarkGreen);
            }
            */
            #endregion

        }

    }
}
