using DreamKeeper.Models;
using DreamKeeper.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DreamKeeper.ViewModels
{
    public class DreamsViewModel
    {
        private readonly DreamService _dreamService;
        public List<Dream> Dreams { get; set; }

        public DreamsViewModel(DreamService dreamService)
        {
            _dreamService = dreamService;
            Dreams = _dreamService.GetDreams();
            //PopulateDreams();
        }

        //private void PopulateDreams()
        //{
        //
        //    // Call GetDreams() method to fetch dreams
        //    var dreamList = _dreamService.GetDreams();
        //
        //    // Populate dreams collection with fetched dreams
        //    foreach (var dream in dreamList)
        //    {
        //        Dreams.Add(dream);
        //    }
        //}

    }
}
