using DreamKeeper.Data;
using DreamKeeper.Models;
using DreamKeeper.Services;
using DreamKeeper.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace DreamKeeper
{
    public partial class MainPage : ContentPage
    {
        //int count = 0;
        //ObservableCollection<DreamsViewModel> DreamsList { get; set; }

        private readonly DreamsViewModel _viewModel;
        private readonly DbContextOptions<AppDbContext> _dbContextOptions;
        public DreamService _dreamService { get; }

        public MainPage()
        {

            //DreamsList = new ObservableCollection<DreamsViewModel>()
            //{    
            //    new DreamsViewModel 
            //    { dreams = new ObservableCollection<Dream>() {
            //            new Dream()
            //            {
            //                Id = 1, DreamName = "Test data.", DreamDescription = "Test desc.", DreamDate = DateTime.Now, DreamRecording = null
            //            },
            //            new Dream()
            //            {
            //                Id = 2, DreamName = "Test data 2.", DreamDescription = "Test desc 2.", DreamDate = DateTime.Now, DreamRecording = null
            //            }
            //        }
            //    }
            //};

            InitializeComponent();

            _dbContextOptions = new DbContextOptions<AppDbContext>();
            _dreamService = new DreamService(_dbContextOptions); // Instantiate DreamService here
            _viewModel = new DreamsViewModel(_dreamService);
            BindingContext = _viewModel;
        }

        //private void OnCounterClicked(object sender, EventArgs e)
        //{
            //count++;
        //}
    }

}
