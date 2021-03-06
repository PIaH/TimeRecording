﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Shapes;
using TimeRecording.Common;
using TimeRecording.Common.Navigation;
using TimeRecording.IO.Repository;
using TimeRecording.Model;

namespace TimeRecording.ViewModel
{
    public enum TimeBudgetUnit
    {
        ManMonths,
        ManDays,
        Hours
    }

    public static class TimeBudgetUnitExtension
    {
        public static string ToReadableString(this TimeBudgetUnit unit)
        {
            if (unit == TimeBudgetUnit.Hours)
            {
                return "H (Stunden)";
            }
            else if (unit == TimeBudgetUnit.ManDays)
            {
                return "MT (Manntage)";
            }
            else
            {
                return "MM (Mannmonate)";
            }
        }

        public static TimeBudgetUnit FromReadableString(this string humanReadableUnit)
        {
            if (humanReadableUnit.Equals("H (Stunden)"))
            {
                return TimeBudgetUnit.Hours;
            }
            else if (humanReadableUnit.Equals("MT (Manntage)"))
            {
                return TimeBudgetUnit.ManDays;
            }
            else
            {
                return TimeBudgetUnit.ManMonths;
            }
        }
    }


    public class CreateProjectViewModel : INotifyPropertyChanged
    {
        #region Member

        private IRepository CurrentRepository = RepositoryFactory.CurrentRepository;
        private ObservableCollection<Project> mProjects;

        #endregion

        #region C'tor

        public CreateProjectViewModel(ObservableCollection<Project> projects)
        {           
            mProjects = projects;

            TimeBudget = string.Empty;
            TimeBudgetUnits = new ObservableCollection<string>();
            TimeBudgetUnits.Add(TimeBudgetUnit.Hours.ToReadableString());
            TimeBudgetUnits.Add(TimeBudgetUnit.ManDays.ToReadableString());
            TimeBudgetUnits.Add(TimeBudgetUnit.ManMonths.ToReadableString());
            SelectedTimeBudgetUnit = TimeBudgetUnits.First();

            InitCommands();
        }

        #endregion

        #region Model

        private ObservableCollection<string> mTimeBudgetUnits;
        public ObservableCollection<string> TimeBudgetUnits
        {
            get
            {
                return mTimeBudgetUnits;
            }
            set
            {
                mTimeBudgetUnits = value;
            }
        }

        private string mTimeBudget;
        public string TimeBudget
        {
            get
            {
                return mTimeBudget;
            }
            set
            {
                mTimeBudget = value;
                NotifyPropertyChanged("TimeBudget");
            }
        }

        private string mSelectedTimeBudgetUnit;
        public string SelectedTimeBudgetUnit
        {
            get
            {
                return mSelectedTimeBudgetUnit;
            }
            set
            {
                mSelectedTimeBudgetUnit = value;
                NotifyPropertyChanged("SelectedTimeBudgetUnit");
            }
        }


        private string mProjectName;
        public string ProjectName
        {
            get
            {
                return mProjectName;
            }
            set
            {
                mProjectName = value;
                NotifyPropertyChanged("ProjectName");
            }
        }

        #endregion

        #region Commands

        public ICommand CreateProjectCommand { get; set; }

        private void InitCommands()
        {
            CreateProjectCommand = new RelayCommand(o => CreateProjectHandler(), o => CreateProjectCondition());
        }

        #region Create Project

        private void CreateProjectHandler()
        {
            mProjects.Add(new Project { Name = ProjectName, TimeBudget = GetTimeBudget() });
            NavigatorFactory.MyNavigator.NavigateBack();
        }

        private bool CreateProjectCondition()
        {
            return !CurrentRepository.IsProjectExisting(ProjectName) && CurrentRepository.IsProjectNameValid(ProjectName);
        }

        #endregion

        #endregion

        #region Private Helpers

        private TimeSpan? GetTimeBudget()
        {
            TimeSpan? budget = null;
            double timeNumber = 0.0;
            var correctedTimeBudget = TimeBudget.Replace('.', ',');
            if (double.TryParse(correctedTimeBudget, out timeNumber))
            {
                var selectedUnit = SelectedTimeBudgetUnit.FromReadableString();
                if (selectedUnit == TimeBudgetUnit.Hours)
                {
                    budget = TimeSpan.FromHours(timeNumber);
                }
                else if (selectedUnit == TimeBudgetUnit.ManDays)
                {
                    budget = TimeSpan.FromHours(timeNumber * 8);
                }
                else if (selectedUnit == TimeBudgetUnit.ManMonths)
                {
                    budget = TimeSpan.FromHours(timeNumber * 8 * 20);
                }
            }
            return budget;
        }

        #endregion

        #region Common

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        #endregion

    }
}
