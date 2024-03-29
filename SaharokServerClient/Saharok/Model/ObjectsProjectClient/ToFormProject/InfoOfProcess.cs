﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace Saharok
{
    [Serializable]
    public class InfoOfProcess : ISerializable, INotifyPropertyChanged
    {
        private static InfoOfProcess instance;
        private static object syncRoot = new Object();

        private InfoOfProcess()
        { }

        public static InfoOfProcess GetInstance()
        {
            if (instance == null)
            {
                lock (syncRoot)
                {
                    if (instance == null)
                        instance = new InfoOfProcess();
                }
            }
            return instance;
        }
        public static void SetInstance(InfoOfProcess infoOfProcess)
        {
            infoOfProcess.PropertyChanged = instance.PropertyChanged;
            instance = infoOfProcess;
            instance.OnPropertyChanged(nameof(TotalFormsFiles));// обновим связанные данные на форме
        }

        public static void RefreshInstance()
        {

            instance = new InfoOfProcess();
        }

        private int totalFormsFiles;
        public int TotalFormsFiles
        {
            get
            {
                lock (syncRoot)
                {
                    return totalFormsFiles;
                }
            }
            set
            {
                lock (syncRoot)
                {
                    totalFormsFiles = value;
                    OnPropertyChanged(nameof(TotalFormsFiles));
                }
            }
        }

        private int totalFormsSections;
        public int TotalFormsSections
        {
            get
            {
                lock (syncRoot)
                {
                    return totalFormsSections;
                }
            }
            set
            {
                lock (syncRoot)
                {
                    totalFormsSections = value;
                    OnPropertyChanged(nameof(TotalFormsSections));
                }
            }
        }

        private int completeFormsFiles;
        public int CompleteFormsFiles
        {
            get
            {
                lock (syncRoot)
                {
                    return completeFormsFiles;
                }
            }
            set
            {
                lock (syncRoot)
                {
                    completeFormsFiles = value;
                    OnPropertyChanged(nameof(CompleteFormsFiles));
                }
            }
        }

        private int completeFormsSections;
        public int CompleteFormsSections
        {
            get
            {
                lock (syncRoot)
                {
                    return completeFormsSections;
                }
            }
            set
            {
                lock (syncRoot)
                {
                    completeFormsSections = value;
                    OnPropertyChanged(nameof(CompleteFormsSections));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected InfoOfProcess(SerializationInfo info, StreamingContext context)
        {
            FieldsSerializble.GetValue(this, info);
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            FieldsSerializble.AddValue(this, info);
        }
    }
}
