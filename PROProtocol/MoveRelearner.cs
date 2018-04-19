using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PROProtocol
{
    public class MoveRelearner
    {
        public ReadOnlyCollection<MovesManager.MoveData> Moves => _moveNames.AsReadOnly();
        private List<MovesManager.MoveData> _moveNames = new List<MovesManager.MoveData>();

        public int SelectedPokemonUid { get; }

        public bool IsEgg { get; }

        public MoveRelearner(int pokemonUid, bool isEgg)
        {
            SelectedPokemonUid = pokemonUid;
            IsEgg = isEgg;
        }

        public void ProcessMessage(string msg)
        {
            string[] array = msg.Split(new string[]
            {
                "|"
            }, StringSplitOptions.None);

            for (int i = 0; i < array.Length; i++)
            {
                string moveName = array[i];
                if (moveName.Length > 2)
                {
                    MovesManager.MoveData move = MovesManager.Instance.GetMoveData(moveName);
                    _moveNames.Add(move);
                }
            }
        }
    }
}
