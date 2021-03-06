﻿using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using GalaSoft.MvvmLight;

namespace MVVM.Popups.ViewModel
{
    public class PopupWindowViewModel : ViewModelBase
    {
        private string m_title;

        public IPopupViewModel ViewModel { get; }

        public string Title
        {
            get => m_title;
            set
            {
                m_title = value;
                RaisePropertyChanged();
            }
        }

        public PopupWindowViewModel(IPopupViewModel _viewModel)
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                return;
            }

            Debug.Assert(_viewModel != null);
            ViewModel = _viewModel;
        }
    }
}
