using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FileGetter;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using WinSCP;

namespace Common.SFTP
{
    public class FileObject : ViewModelBase
    {
        public FileObject()
        {
            m_state = FileObjectState.NONE;
        }

        public FileObject(RemoteFileInfo _file, string _root)
        {
            Debug.Assert(_file != null);
            Debug.Assert(!string.IsNullOrWhiteSpace(_root));

            m_state = FileObjectState.NONE;

            Root = _root;
            FullName = _file.FullName;
            Size = _file.Length;
        }

        public void Refresh()
        {
        }

        public void UpdateProgress(double _percentCompleted, int _cps)
        {
            PercentCompleted = _percentCompleted;

            Cps = _cps;
        }

        public event EventHandler StateChanged;

        private string GetDisplayCpsText(FileObjectState _state)
        {
            string text;
            switch (_state)
            {
                case FileObjectState.IGNORE:
                case FileObjectState.NONE:
                    text = "...";
                    break;
                case FileObjectState.PENDING:
                    text = " - mbps";
                    break;
                case FileObjectState.IN_PROGRESS:
                    var speedKbs = (float) Cps / 1000;
                    var speedMbs = speedKbs / 1000;
                    text = $"{speedMbs:n2} mbps";
                    break;
                case FileObjectState.COMPLETED:
                    text = "";
                    break;
                case FileObjectState.CANCELLED:
                    text = "cancelled";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return text;
        }

        public static DirectoryObjectState GetState(IEnumerable<FileObjectState> _fileStates)
        {
            Debug.Assert(_fileStates != null);

            var fileObjectStates = _fileStates as FileObjectState[] ?? _fileStates.ToArray();

            var anyNone = fileObjectStates.Any(_state => _state == FileObjectState.NONE);
            var anyCompleted = fileObjectStates.Any(_state => _state == FileObjectState.COMPLETED);
            var anyInProgress = fileObjectStates.Any(_state =>
                _state == FileObjectState.IN_PROGRESS || _state == FileObjectState.PENDING);

            DirectoryObjectState state;

            if (anyInProgress)
            {
                state = DirectoryObjectState.IN_PROGRESS;
            }
            else
            {
                if (anyCompleted)
                    state = anyNone ? DirectoryObjectState.INCOMPLETE : DirectoryObjectState.COMPLETED;
                else
                    state = DirectoryObjectState.NONE;
            }
            return state;
        }

        public RelayCommand CancelCommand => new RelayCommand(() => Cancel = true);

        public string FullName { get; set; }
        public string Root { get; set; }

        public string RelativeName
        {
            get
            {
                var name = FullName.Substring(Root.Length + 1, FullName.Length - Root.Length - 1);
                return name;
            }
        }

        public FileObjectState State
        {
            get => m_state;
            set
            {
                if (m_state != value)
                {
                    m_state = value;

                    RaisePropertyChanged();
                    RaisePropertyChanged("IsCompleted");
                    RaisePropertyChanged("IsInProgress");
                    RaisePropertyChanged("DisplayCPS");

                    StateChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public double PercentCompleted
        {
            get => m_percentCompleted;
            set
            {
                m_percentCompleted = value;
                RaisePropertyChanged();
            }
        }

        public int Cps
        {
            get => m_cps;
            private set
            {
                m_cps = value;
                RaisePropertyChanged();
                RaisePropertyChanged("DisplayCps");
            }
        }

        public bool Cancel { get; set; }

        public long Size { get; }

        public string DisplayCps
        {
            get
            {
                var text = GetDisplayCpsText(State);
                return text;
            }
        }

        public bool IsCompleted => State == FileObjectState.COMPLETED;
        public bool IsInProgress => State == FileObjectState.IN_PROGRESS || State == FileObjectState.PENDING;
        private int m_cps;
        private double m_percentCompleted;

        private FileObjectState m_state;
    }
}