﻿using DevZest.Data;
using DevZest.Data.Presenters;
using DevZest.Data.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Input;

namespace FileExplorer
{
    public sealed class CurrentDirectoryPresenter : DirectoryPresenter<CurrentDirectoryPresenter.Item>,
        InPlaceEditor.ICommandService,
        DataView.ICommandService
    {
        public sealed class Item : Model
        {
        }

        public CurrentDirectoryPresenter(DataView dataView, DirectoryTreePresenter directoryTree)
            : base(directoryTree)
        {
            _directoryTree = directoryTree;
            _currentDirectory = NewScalar<string>(null, StringComparer.OrdinalIgnoreCase).AddValidator(ValidateDirectory);
            CurrentDirectory = DirectoryTreePresenter.CurrentPath;

            var dataSet = DataSet<Item>.Create();
            Show(dataView, dataSet);
        }

        private readonly DirectoryTreePresenter _directoryTree;

        private static string ValidateDirectory(string path)
        {
            if (string.IsNullOrEmpty(path))
                return "Directory can't be empty";
            if (!Directory.Exists(path))
                return "Directory does not exist";
            else
                return null;
        }

        private readonly Scalar<string> _currentDirectory;
        protected sealed override string CurrentDirectory
        {
            get { return _currentDirectory.GetValue(true); }
            set { _currentDirectory.SetValue(value, true); }
        }

        protected override void BuildTemplate(TemplateBuilder builder)
        {
            builder.GridColumns("*")
                .GridRows("Auto", "0")
                .RowRange(0, 1, 0, 1)
                .AddBinding(0, 0, _currentDirectory.BindToTextBox().MergeIntoInPlaceEditor(_currentDirectory.BindToTextBlock("{0} (Click to edit)")));
        }

        IEnumerable<CommandEntry> InPlaceEditor.ICommandService.GetCommandEntries(InPlaceEditor inPlaceEditor)
        {
            yield return RowView.Commands.BeginEdit.Bind(BeginEdit, CanBeginEdit, new MouseGesture(MouseAction.LeftClick));
        }

        private void CanBeginEdit(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !ScalarContainer.IsEditing;
            if (!e.CanExecute)
                e.ContinueRouting = true;
        }

        private void BeginEdit(object sender, ExecutedRoutedEventArgs e)
        {
            ScalarContainer.BeginEdit();
        }

        IEnumerable<CommandEntry> DataView.ICommandService.GetCommandEntries(DataView dataView)
        {
            var baseService = this.GetRegisteredService<DataView.ICommandService>();
            foreach (var entry in baseService.GetCommandEntries(dataView))
            {
                if (entry.Command == DataView.Commands.CancelEditScalars)
                    yield return entry.ReplaceWith(new KeyGesture(Key.Escape));
                else if (entry.Command == DataView.Commands.EndEditScalars)
                    yield return entry.ReplaceWith(new KeyGesture(Key.Enter));
                else
                    yield return entry;
            }
        }

        protected override bool ConfirmEndEditScalars()
        {
            var path = _currentDirectory.GetValue();
            _directoryTree.Select(path);
            ScalarContainer.CancelEdit();
            return false;
        }
    }
}
