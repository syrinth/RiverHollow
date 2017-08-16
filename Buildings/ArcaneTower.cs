using Adventure.Game_Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Buildings
{
    public class ArcaneTower : Building
    {
        public ArcaneTower()
        {
            _baseWidth = 3;
            _baseHeight = 3;

            _reqGold = 10000;
            _texture = GameContentManager.GetInstance().GetTexture(@"Textures\ArcaneTower");
        }
        
    }
}
