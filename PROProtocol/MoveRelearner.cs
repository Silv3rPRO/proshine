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
        public bool IsEggMoves { get; }

        public MoveRelearner(int pokemonUid, bool isEggMoves)
        {
            SelectedPokemonUid = pokemonUid;
            IsEggMoves = isEggMoves;
        }

        public void ProcessMessage(string data)
        {
            string[] moveNames = data.Split(new string[] { "|" }, StringSplitOptions.None);

            for (int i = 0; i < moveNames.Length; i++)
            {
                if (moveNames[i].Length > 2)
                {
                    MovesManager.MoveData move = MovesManager.Instance.GetMoveData(moveNames[i]);
                    _moveNames.Add(move);
                }
            }
        }
    }
}
