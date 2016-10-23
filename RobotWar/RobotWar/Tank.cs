using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace RobotWar
{
    public class Tank
    {
        public Texture2D textura, alliance_destruido, coalition_destuido;
        public Vector2 posicao;
        public Vector2 posicao_destino;
        public int power, power_anterior;
        public int vida;
        public Team equipa;
        public Vector2 posicao_grelha;
        static public float escala=0.5f;
        public int ID;
        public bool seleccionado;
        public bool em_movimento;
        public bool activo;
        public SpriteEffects efeito;
        List<Terreno> lista_pode_mover;
        Tank tanque_seleccionado;
        public int turnos_espera_disparar;
        SpriteFont font;
        public bool pode_mover;
        public Color cor;

        //powerups
        public bool sniper, double_shot, mega_power, double_turn;

        //texturas da vida
        Texture2D textura_vida, vida_200, vida_180, vida_160, vida_140, vida_120, vida_100, vida_80, vida_60, vida_40, vida_20;

        //texturas do power
        Texture2D textura_power, power_100, power_90, power_80, power_70, power_60, power_50, power_up;

        //propriedades para a rotação
        public float angulo_original, angulo_destino, angulo_actual;

        #region Angulo (graus) entre dois pontos
        public float anguloDoisVectores(Vector2 destino, float rotacao)
        {

            Vector2 origem = new Vector2(posicao.X + textura.Width / 4, posicao.Y + textura.Height / 4);
            Vector2 aux;
            aux = new Vector2(destino.X - origem.X, destino.Y - origem.Y);
            double graus = Math.Atan2(aux.X,aux.Y);
            
            return (float)-graus+rotacao;

        }
        #endregion

        public void Initialiazing(Texture2D textura, Vector2 posicao, SpriteEffects efeito, Team equipa,Random gerador_numeros, int ID)
        {
            this.textura = textura;
            this.posicao = posicao;
            this.power = gerador_numeros.Next(50)+50;
            this.vida = 200;
            this.activo = true;

            this.angulo_original = MathHelper.Pi;
            
            this.equipa = equipa;
            
            this.ID = ID;
            this.seleccionado = false;
            this.angulo_actual = angulo_original;
            this.angulo_destino = angulo_actual;
            this.efeito = efeito;
            this.posicao_destino = this.posicao;
            this.em_movimento = false;
            this.turnos_espera_disparar = 0;
            this.pode_mover = true;
            this.cor = Color.White;

            //powers
            this.sniper = false;
            this.double_shot = false;
            this.double_turn = false;
            this.mega_power = false;

            this.power_anterior = power;
            this.cor = Color.White;
            
        }

        public void LoadContent(ContentManager content)
        {
            vida_200 = content.Load<Texture2D>("texturas/sprites/tanques/health/200");
            vida_180 = content.Load<Texture2D>("texturas/sprites/tanques/health/180");
            vida_160 = content.Load<Texture2D>("texturas/sprites/tanques/health/160");
            vida_140 = content.Load<Texture2D>("texturas/sprites/tanques/health/140");
            vida_120 = content.Load<Texture2D>("texturas/sprites/tanques/health/120");
            vida_100 = content.Load<Texture2D>("texturas/sprites/tanques/health/100");
            vida_80 = content.Load<Texture2D>("texturas/sprites/tanques/health/80");
            vida_60 = content.Load<Texture2D>("texturas/sprites/tanques/health/60");
            vida_40 = content.Load<Texture2D>("texturas/sprites/tanques/health/40");
            vida_20 = content.Load<Texture2D>("texturas/sprites/tanques/health/20");

            power_100 = content.Load<Texture2D>("texturas/sprites/tanques/power/power_100");
            power_90 = content.Load<Texture2D>("texturas/sprites/tanques/power/power_90");
            power_80 = content.Load<Texture2D>("texturas/sprites/tanques/power/power_80");
            power_70 = content.Load<Texture2D>("texturas/sprites/tanques/power/power_70");
            power_60 = content.Load<Texture2D>("texturas/sprites/tanques/power/power_60");
            power_up = content.Load<Texture2D>("texturas/sprites/tanques/power/power_up");

            coalition_destuido = content.Load<Texture2D>("texturas/sprites/tanques/tank2_destruido");
            alliance_destruido = content.Load<Texture2D>("texturas/sprites/tanques/tank_destruido");

            font = content.Load<SpriteFont>("fontes/gameFont");
        }

        public void UnloadContent()
        {
            vida_200.Dispose();
            vida_180.Dispose();
            vida_160.Dispose();
            vida_140.Dispose();
            vida_120.Dispose();
            vida_100.Dispose();
            vida_80.Dispose();
            vida_60.Dispose();
            vida_40.Dispose();
            vida_20.Dispose();

            power_100.Dispose();
            power_90.Dispose();
            power_80.Dispose();
            power_70.Dispose();
            power_60.Dispose();
            power_up.Dispose();

            coalition_destuido.Dispose();
            alliance_destruido.Dispose();

            
        }

        #region Rodar Alliance
        private void rodarAlliance()
        {
            angulo_actual += (angulo_destino - angulo_actual) / 4;
        }
        #endregion

        #region Rodar Coalition
        private void rodarCoalition()
        {
            //efectuar a rotação
            if (angulo_actual != angulo_destino)
            {
                if (Math.Sign(angulo_actual) != Math.Sign(angulo_destino))
                {

                    if (Math.Sign(angulo_actual) == 1)
                    {
                        if (angulo_actual > MathHelper.PiOver2)
                        {
                            angulo_actual += (MathHelper.Pi - angulo_actual) / 2;
                            if (Math.Round(angulo_actual, 3) == Math.Round(MathHelper.Pi, 3))
                            {
                                angulo_actual = -MathHelper.Pi;
                                angulo_actual += (angulo_destino - angulo_actual) / 4;
                            }
                        }
                        else
                        {
                            angulo_actual += (0 - angulo_actual) / 2;
                            if (Math.Round(angulo_actual, 3) == 0)
                            {
                                angulo_actual = 0;
                                angulo_actual += (angulo_destino - angulo_actual) / 4;
                            }
                        }
                    }

                    if (Math.Sign(angulo_actual) == -1)
                    {
                        if (angulo_actual > -MathHelper.PiOver2)
                        {
                            angulo_actual += (0 - angulo_actual) / 2;
                            if (Math.Round(angulo_actual, 3) == 0)
                            {
                                angulo_actual = 0;
                                angulo_actual += (angulo_destino - angulo_actual) / 4;
                            }
                        }
                        else
                        {
                            angulo_actual += (-MathHelper.Pi - angulo_actual) / 2;
                            if (Math.Round(angulo_actual, 3) == Math.Round(-MathHelper.Pi, 3))
                            {
                                angulo_actual = MathHelper.Pi;
                                angulo_actual += (angulo_destino - angulo_actual) / 4;
                            }
                        }
                    }
                }
                else
                {
                    angulo_actual += (angulo_destino - angulo_actual) / 4;
                }
            }
        }
        #endregion

        #region Mover tanque
        private void Mover(List<Tank> lista_tanques, Team equipa_activa, ContentManager Content)
        {
            if (Math.Round(posicao.X) != Math.Round(posicao_destino.X) || Math.Round(posicao.Y) != Math.Round(posicao_destino.Y))
            {
                em_movimento = true;
                lista_pode_mover.Clear();
                if (Math.Round(posicao.X) != Math.Round(posicao_destino.X))
                {
                    if (Math.Round(posicao.X) < Math.Round(posicao_destino.X))
                    {
                        posicao.X += 1;
                    }
                    if (Math.Round(posicao.X) > Math.Round(posicao_destino.X))
                    {
                        posicao.X -= 1;
                    }
                }
                if (Math.Round(posicao.Y) != Math.Round(posicao_destino.Y))
                {
                    if (Math.Round(posicao.Y) < Math.Round(posicao_destino.Y))
                    {
                        posicao.Y += 1;
                    }
                    if (Math.Round(posicao.Y) > Math.Round(posicao_destino.Y))
                    {
                        posicao.Y -= 1;
                    }
                }
                    
                
            }
            else
            {
                if (em_movimento)
                {
                    Game1.tanque_seleccionado = null;
                    this.seleccionado = false;
                    Game1.apontador = Game1.seta_rato;
                    em_movimento = false;

                    if (!double_turn)
                    {
                        this.cor = Color.Red;
                        pode_mover = false;
                    }

                    //Tanque chegou ao destino
                    if (Game1.apanharPowerUp(this, Content))
                    {
                        Game1.apanharPowerUp(this, Content);
                    }
                    else
                    {
                        Game1.actualizarJogadas(this);
                    }

                    if (double_turn)
                    {
                        this.cor = Color.White;
                        this.pode_mover = true;
                    }
                    double_turn = false;
                    
                }
            }
        }
        #endregion

        public void Update(GameTime gameTime, List<Tank> lista_tanques, Team equipa_activa, ContentManager Content)
        {
            if (this.pode_mover) cor = Color.White;
            if (this.equipa == Team.Coalition) rodarCoalition();
            if (this.equipa == Team.Alliance) rodarAlliance();
            Mover(lista_tanques, equipa_activa, Content);
        }

        public void Draw(SpriteBatch spritebatch, Vector2 posicao_rato)
        {
            
            //spritebatch.Draw(textura,posicao,null,Color.White,
                //anguloDoisVectores(posicao,posicao_rato),new Vector2(textura.Width/2,textura.Height/2),escala,efeito,0f);

            float x = textura.Width/2;
            float y = textura.Height/2;
            Vector2 centro_rotacao = new Vector2(x, y);

            spritebatch.Draw(textura, new Vector2(posicao.X+(textura.Width/4),posicao.Y+(textura.Height/4)), null, cor,
               angulo_actual, centro_rotacao, escala, efeito, 0f);

            //desenhar grafico da vida
            if (this.vida > 180 && this.vida <= 200) textura_vida = vida_200;
            if (this.vida > 160 && this.vida <= 180) textura_vida = vida_180;
            if (this.vida > 140 && this.vida <= 160) textura_vida = vida_160;
            if (this.vida > 120 && this.vida <= 140) textura_vida = vida_140;
            if (this.vida > 100 && this.vida <= 120) textura_vida = vida_120;
            if (this.vida > 80 && this.vida <= 100) textura_vida = vida_80;
            if (this.vida > 60 && this.vida <= 80) textura_vida = vida_60;
            if (this.vida > 40 && this.vida <= 60) textura_vida = vida_40;
            if (this.vida >= 20 && this.vida <= 40) textura_vida = vida_20;
            if (this.vida < 20) textura_vida = null;

            SpriteEffects efeito_vida;
            float x_vida = vida_200.Width / 2;
            float y_vida = vida_200.Height / 2;
            Vector2 centro_rotacao_vida = new Vector2(x_vida, y_vida);
            if (this.equipa == Team.Alliance)
            {
                efeito_vida = SpriteEffects.FlipVertically;
            }
            else
            {
                efeito_vida = SpriteEffects.None;
            }

            if (this.textura_vida != null)
            {
                spritebatch.Draw(textura_vida, new Vector2(posicao.X + (textura.Width / 4), posicao.Y + (textura.Height / 4)), null, Color.White,
                   angulo_actual, centro_rotacao_vida, escala, SpriteEffects.FlipVertically, 0f);
            }

            //desenhar gráfico do power
            if (this.power > 100) textura_power = power_up;
            if (this.power > 90 && this.power <= 100) textura_power = power_100;
            if (this.power > 80 && this.power <= 90) textura_power = power_90;
            if (this.power > 70 && this.power <= 80) textura_power = power_80;
            if (this.power > 60 && this.power <= 70) textura_power = power_70;
            if (this.power >= 50 && this.power <= 60) textura_power = power_60;

            if (activo)
            {
                spritebatch.Draw(textura_power, new Vector2(posicao.X + (textura.Width / 4), posicao.Y + (textura.Height / 4)), null, Color.White,
                       angulo_actual, centro_rotacao_vida, escala, SpriteEffects.FlipVertically, 0f);
            }

            //tempo de espera até poder disparar
            if (turnos_espera_disparar > 0 && activo)
            {
                spritebatch.DrawString(font, turnos_espera_disparar.ToString(), new Vector2((posicao.X + (textura.Width / 4))-8, posicao.Y + (textura.Height / 4)-8), Color.Yellow);
            }
        }

        public void alterarPosicaoDestino(Vector2 posicao_destino, List<Terreno> lista_pode_mover, Tank tanque_seleccionado)
        {
            this.posicao_destino = posicao_destino;
            this.lista_pode_mover = lista_pode_mover;
            this.tanque_seleccionado = tanque_seleccionado;
        }


    }
}
