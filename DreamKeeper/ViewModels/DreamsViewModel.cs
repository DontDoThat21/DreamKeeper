using DreamKeeper.Models;
using DreamKeeper.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DreamKeeper.ViewModels
{
    public class DreamsViewModel
    {
        private readonly DreamService _dreamService;
        public ObservableCollection<Dream> Dreams { get; set; }

        public DreamsViewModel(DreamService dreamService)
        {
            _dreamService = dreamService;
            Dreams = _dreamService.GetDreams();
            DeleteDreamCommand = new Command<Dream>(async (dream) => await OnDeleteDream(dream));
            //PopulateDreams();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task OnDeleteDream(Dream dream)
        {
            if (dream != null)
            {
                bool isConfirmed = await App.Current.MainPage.DisplayAlert(
                    "Confirm Delete",
                    "Are you sure you want to delete this dream?",
                    "Yes",
                    "No");

                if (isConfirmed)
                {
                    _dreamService.DeleteDream(dream.Id);
                    Dreams.Remove(dream);
                }
            }
        }

        public ICommand DeleteDreamCommand { get; }

    }
}
