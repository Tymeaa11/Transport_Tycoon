using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VolcanicTransport.Model.Utils;
using VolcanicTransport.Model.World;

namespace VolcanicTransport_WPF.ViewModel
{
    public class MinimapViewModel : ViewModelBase
    {
        private SquareMatrixIterator<Chunk> Chunks;

        public MinimapViewModel(SquareMatrixIterator<Chunk> chunks)
        {
            Chunks = chunks;
        }




    }
}
