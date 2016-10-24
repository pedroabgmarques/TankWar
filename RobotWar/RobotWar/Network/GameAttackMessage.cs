using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using RobotWar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TankWar.Network
{

    /// <summary>
    /// Class documentation
    /// </summary>
    public class GameAttackMessage : Message
    {

        [JsonProperty("IDAtacante")]
        public int IDAtacante;
        [JsonProperty("IDAtacado")]
        public int IDAtacado;
        [JsonProperty("posicao")]
        public Vector2 posicao;
        [JsonProperty("dano")]
        public int dano;
        [JsonProperty("rotacao")]
        public float rotacao;

        public GameAttackMessage(MessageType msgType, int IDAtacante, int IDAtacado, Vector2 posicao, int dano, float rotacao)
            : base(msgType)
        {
            this.IDAtacante = IDAtacante;
            this.IDAtacado = IDAtacado;
            this.posicao = posicao;
            this.dano = dano;
            this.rotacao = rotacao;
        }
       

        public override byte[] ByteMessage()
        {
            return Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(this));
        }
    }
}
