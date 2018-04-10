using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PROProtocol
{
    public class MoveRelearnManager
    {
        private List<MovesManager.MoveData> MoveDatas = new List<MovesManager.MoveData>();
        public ReadOnlyCollection<MovesManager.MoveData> Moves => MoveDatas.AsReadOnly();
        public int SelecetedPokemonUid { get; set; }
        public bool isEgg { get; set; }
        public MoveRelearnManager()
        {

        }
        public void ProcessRelearnManager(string msg)
        {
           string[] array = msg.Split(new string[]
           {
                "|"
           }, StringSplitOptions.None);

            if (array.Length > 0)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    if (array[i].Length > 2)
                    {
                        MovesManager.MoveData move = MovesManager.Instance.GetMoveData(array[i]);
                        MoveDatas.Add(move);
                    }
                }
            }
        }
    }
}
