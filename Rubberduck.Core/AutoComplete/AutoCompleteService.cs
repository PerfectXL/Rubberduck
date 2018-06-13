﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Rubberduck.Settings;
using Rubberduck.VBEditor;
using Rubberduck.VBEditor.Events;
using Rubberduck.VBEditor.WindowsApi;

namespace Rubberduck.AutoComplete
{
    public class AutoCompleteService : IDisposable
    {
        private readonly IGeneralConfigService _configService;
        private readonly List<IAutoComplete> _autoCompletes;

        private AutoCompleteSettings _settings;

        public AutoCompleteService(IGeneralConfigService configService, IAutoCompleteProvider provider)
        {
            _configService = configService;
            _autoCompletes = provider.AutoCompletes.ToList();

            _configService.SettingsChanged += ConfigServiceSettingsChanged;
            VBENativeServices.KeyDown += HandleKeyDown;
        }

        private void ConfigServiceSettingsChanged(object sender, ConfigurationChangedEventArgs e)
        {
            var config = _configService.LoadConfiguration();
            ApplyAutoCompleteSettings(config);
        }
        
        public void ApplyAutoCompleteSettings(Configuration config)
        {
            _settings = config.UserSettings.AutoCompleteSettings;
            foreach (var autoComplete in _autoCompletes)
            {
                var setting = config.UserSettings.AutoCompleteSettings.AutoCompletes.FirstOrDefault(s => s.Key == autoComplete.GetType().Name);
                if (setting != null && autoComplete.IsEnabled != setting.IsEnabled)
                {
                    autoComplete.IsEnabled = setting.IsEnabled;
                    continue;
                }
            }
        }

        private void HandleKeyDown(object sender, AutoCompleteEventArgs e)
        {
            var module = e.CodeModule;
            var qualifiedSelection = module.GetQualifiedSelection();
            var selection = qualifiedSelection.Value.Selection;

            if (e.Keys != Keys.None && selection.LineCount > 1 || selection.StartColumn != selection.EndColumn)
            {
                return;
            }

            var currentContent = module.GetLines(selection);

            var handleDelete = e.Keys == Keys.Delete && selection.EndColumn <= currentContent.Length;
            var handleBackspace = e.Keys == Keys.Back && selection.StartColumn > 1;
            foreach (var autoComplete in _autoCompletes.Where(auto => auto.IsEnabled))
            {
                if (handleDelete || handleBackspace)
                {
                    if (DeleteAroundCaret(e, autoComplete))
                    {
                        break;
                    }
                }
                else
                {
                    if (autoComplete.Execute(e, _settings))
                    {
                        break;
                    }
                }
            }
        }

        private bool DeleteAroundCaret(AutoCompleteEventArgs e, IAutoComplete autoComplete)
        {
            if (autoComplete.IsInlineCharCompletion)
            {
                var code = e.CurrentLine;
                // If caret LHS is the AC input token and RHS is the AC output token, we can remove both.
                // Substring index is 0-based. Selection from code pane is 1-based.
                // LHS should be at StartColumn - 2, RHS at StartColumn - 1.
                var caretLHS = code.Substring(Math.Max(0, e.CurrentSelection.StartColumn - 2), 1);
                var caretRHS = code.Length >= e.CurrentSelection.StartColumn
                    ? code.Substring(e.CurrentSelection.StartColumn - 1, 1)
                    : string.Empty;

                if (caretLHS == autoComplete.InputToken && caretRHS == autoComplete.OutputToken)
                {
                    var left = code.Substring(0, e.CurrentSelection.StartColumn - 2);
                    var right = code.Substring(e.CurrentSelection.StartColumn);
                    using (var pane = e.CodeModule.CodePane)
                    {
                        e.CodeModule.ReplaceLine(e.CurrentSelection.StartLine, left + right);
                        pane.Selection = new Selection(e.CurrentSelection.StartLine, e.CurrentSelection.StartColumn - 1);
                        e.Handled = true;
                    }
                    return true;
                }
            }
            return false;
        }

        public void Dispose()
        {
            VBENativeServices.KeyDown -= HandleKeyDown;
            if (_configService != null)
            {
                _configService.SettingsChanged -= ConfigServiceSettingsChanged;
            }

            _autoCompletes.Clear();
        }
    }
}
