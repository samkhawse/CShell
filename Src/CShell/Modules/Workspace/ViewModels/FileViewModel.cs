﻿#region License
// CShell, A Simple C# Scripting IDE
// Copyright (C) 2013  Arnova Asset Management Ltd., Lukas Buhler
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using CShell.Framework.Results;
using CShell.Modules.Workspace.Results;
using CShell.Util;
using Caliburn.Micro;
using Microsoft.Win32;

namespace CShell.Modules.Workspace.ViewModels
{
    public class FileViewModel : TreeViewModel
    {
        protected readonly FileInfo fileInfo;

        public FileViewModel(string filePath)
            :this(new FileInfo(filePath))
        { }

        public FileViewModel(FileInfo info)
        {
            fileInfo = info;
            DisplayName = fileInfo.Name;
            IsEditable = true;
        }

        public FileInfo FileInfo
        {
            get { return fileInfo; }
        }

        public virtual string ToolTip
        {
            get { return RelativePath; }
        }

        public string RelativePath
        {
            get { return PathHelper.ToRelativePath(Environment.CurrentDirectory, fileInfo.FullName); }
        }

        public string FileExtension
        {
            get { return fileInfo.Extension; }
        }

        public override Uri IconSource
        {
            get
            {
                switch (FileExtension)
                {
                    case ".cshell":
                    case ".cs":
                    case ".csx":
                        return new Uri("pack://application:,,,/CShell;component/Resources/Icons/Icons.16x16.CSFile.png");
                    case ".txt":
                    case ".log":
                        return new Uri("pack://application:,,,/CShell;component/Resources/Icons/Icons.16x16.TextFileIcon.png");
                    default:
                        return new Uri("pack://application:,,,/CShell;component/Resources/Icons/Icons.16x16.MiscFiles.png");
                }
            }
        }

        public IEnumerable<IResult> Delete()
        {
            if (!IsEditable)
                return null;
            var result = MessageBox.Show("Are you sure you want to delete this file?", "Delete", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if(result == MessageBoxResult.Yes)
                fileInfo.Delete();
            return null;
        }

        public IEnumerable<IResult> Rename()
        {
            if (!IsEditable)
                return null;
            IsInEditMode = true;
            return null;
        }

        protected override void EditModeFinished()
        {
            try
            {
                if (DisplayName != null && DisplayName != fileInfo.Name)
                    fileInfo.MoveTo(Path.Combine(fileInfo.DirectoryName, DisplayName));
                else
                    DisplayName = fileInfo.Name;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message.Substring(0, ex.Message.IndexOf(".") + 1), "Rename", MessageBoxButton.OK, MessageBoxImage.Warning);
                DisplayName = fileInfo.Name;
            }
        }

       

        public IEnumerable<IResult> AddFileReferences()
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = CShell.Constants.AssemblyFileFilter;
            dialog.Multiselect = true;
            yield return Show.Dialog(dialog);
            if (dialog.FileNames != null && dialog.FileNames.Length > 0)
            {
                yield return new AddReferencesResult(dialog.FileNames){FilePath = fileInfo.FullName};
            }
        }

        public IEnumerable<IResult> AddGacReferences()
        {
            var windowSettings = new Dictionary<string, object> { { "SizeToContent", SizeToContent.Manual }, { "Width", 500.0 }, { "Height", 500.0 } };
            var dialog = new AssemblyGacViewModel();
            yield return Show.Dialog(dialog, windowSettings);
            var selectedAssemblies = dialog.SelectedAssemblies.Select(item => item.AssemblyName).ToArray();
            if (selectedAssemblies.Length > 0)
            {
                yield return new AddReferencesResult(selectedAssemblies){FilePath = fileInfo.FullName};
            }
        }

        public bool CanAddFileReferences
        {
            get { return fileInfo.Extension.ToLower() == ".csx"; }
        }

        public bool CanAddGacReferences
        {
            get { return fileInfo.Extension.ToLower() == ".csx"; }
        }

    }//end class
}
